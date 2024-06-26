﻿using Newtonsoft.Json.Linq;
using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WhatsSocket.Core.Events;
using WhatsSocket.Core.Extensions;
using WhatsSocket.Core.Helper;
using WhatsSocket.Core.Models;
using WhatsSocket.Core.NoSQL;
using WhatsSocket.Core.Signal;
using WhatsSocket.Core.Stores;
using WhatsSocket.Core.WABinary;

namespace WhatsSocket.Core.Utils
{
    public class ProcessMessageUtil
    {
        public static void CleanMessage(WebMessageInfo message, string meId)
        {
            message.Key.RemoteJid = JidUtils.JidNormalizedUser(message.Key.RemoteJid);
            var participant = message.Key.Participant != null ? JidUtils.JidNormalizedUser(message.Key.Participant) : null;
            if (participant != null)
            {
                message.Key.Participant = participant;
            }
            else
            {
                message.Key.ClearParticipant();
            }
            var content = MessageUtil.NormalizeMessageContent(message.Message);

            if (content?.ReactionMessage != null)
            {
                NormalizeKey(message, meId, content.ReactionMessage.Key);
            }
            if (content?.PollUpdateMessage != null)
            {
                NormalizeKey(message, meId, content.PollUpdateMessage.PollCreationMessageKey);
            }
        }


        internal static async Task ProcessMessage(WebMessageInfo message, bool shouldProcessHistoryMsg, AuthenticationCreds? creds, BaseKeyStore keyStore, EventEmitter ev)
        {
            var meId = creds.Me.ID;
            var chat = new ChatModel()
            {
                ID = JidUtils.JidNormalizedUser(GetChatID(message.Key))
            };
            var isRealMessage = IsRealMessage(message, meId);

            if (isRealMessage)
            {
                chat.ConversationTimestamp = message.MessageTimestamp;
                if (ShouldIncrementChatUnread(message))
                {
                    chat.UnreadCount = chat.UnreadCount + 1;
                }
            }
            var content = MessageUtil.NormalizeMessageContent(message.Message);
            // unarchive chat if it's a real message, or someone reacted to our message
            // and we've the unarchive chats setting on
            if ((isRealMessage || content?.ReactionMessage?.Key?.FromMe == true) && creds.AccountSettings.UnarchiveChats)
            {
                chat.Archived = false;
                chat.ReadOnly = false;
            }

            //TODO Impmlement below
            var protocolMsg = content?.ProtocolMessage;
            if (protocolMsg != null)
            {
                switch (content?.ProtocolMessage.Type)
                {
                    case Message.Types.ProtocolMessage.Types.Type.HistorySyncNotification:
                        {
                            var histNotification = content.ProtocolMessage.HistorySyncNotification;
                            var process = shouldProcessHistoryMsg;
                            var isLatest = creds.ProcessedHistoryMessages.Count == 0;

                            if (process)
                            {
                                creds.ProcessedHistoryMessages.Add(new ProcessedHistoryMessage()
                                {
                                    Key = message.Key,
                                    MessageTimestamp = message.MessageTimestamp
                                });
                                ev.Emit(EmitType.Update, creds);
                                //ev.CredsUpdate(creds);

                                var data = await HistoryUtil.DownloadAndProcessHistorySyncNotification(histNotification);
                                ev.Emit(EmitType.Set, data);
                                //ev.MessageHistorySet((data.contacts, data.chats, data.messages, isLatest));
                            }
                        }
                        break;
                    case Message.Types.ProtocolMessage.Types.Type.AppStateSyncKeyShare:
                        {
                            var newAppStateSyncKeyId = creds.MyAppStateKeyId;
                            var keys = protocolMsg.AppStateSyncKeyShare.Keys;
                            foreach (var item in keys)
                            {
                                var id = item.KeyId.KeyId.ToBase64();
                                var keyData = new AppStateSyncKeyStructure(item.KeyData);
                                keyStore.Set<AppStateSyncKeyStructure>(id, keyData);
                                newAppStateSyncKeyId = id;
                            }
                            creds.MyAppStateKeyId = newAppStateSyncKeyId;
                            ev.Emit(EmitType.Update, creds);
                            //ev.CredsUpdate(creds);
                        }
                        break;
                    case Message.Types.ProtocolMessage.Types.Type.Revoke:
                        ev.Emit(EmitType.Update, [MessageUpdate.FromRevoke(message, protocolMsg)]);
                        //ev.MessageUpdated([MessageUpdate.FromRevoke(message, protocolMsg)]);
                        break;
                    case Message.Types.ProtocolMessage.Types.Type.EphemeralSetting:
                        chat.EphemeralSettingTimestamp = message.MessageTimestamp;
                        chat.EphemeralExpiration = protocolMsg.EphemeralExpiration;
                        break;
                    case Message.Types.ProtocolMessage.Types.Type.PeerDataOperationRequestMessage:
                        var response = protocolMsg.PeerDataOperationRequestResponseMessage;
                        if (response != null)
                        {
                            var peerDataOperationResult = response.PeerDataOperationResult;
                            foreach (var result in peerDataOperationResult)
                            {
                                var retryResponse = result.PlaceholderMessageResendResponse;
                                if (retryResponse != null)
                                {
                                    var webMessageInfo = WebMessageInfo.Parser.ParseFrom(retryResponse.WebMessageInfoBytes);
                                    ev.Emit(EmitType.Update, [new MessageUpdate()
                                    {
                                        Key = webMessageInfo.Key,
                                        Update = new MessageUpdateModel()
                                        {
                                            Message = webMessageInfo.Message
                                        }
                                    }]);
                                    //ev.MessageUpdated([new MessageUpdate()
                                    //{
                                    //    Key = webMessageInfo.Key,
                                    //    Update = new MessageUpdateModel()
                                    //    {
                                    //        Message = webMessageInfo.Message
                                    //    }
                                    //}]);
                                }
                            }
                        }
                        break;
                }
            }
            else if (content?.ReactionMessage != null)
            {
                ev.Emit(EmitType.Reaction, new MessageReactionModel(content.ReactionMessage, content.ReactionMessage.Key));
                //ev.MessageReaction(content.ReactionMessage, content.ReactionMessage.Key);
            }
            else if (message.HasMessageStubType)
            {
                var jid = message.Key.RemoteJid;
                var participants = new List<string>();

                var emitParticipantsUpdate = new Action<string>(action =>
                {
                    ev.Emit(EmitType.Update, new GroupParticipantUpdateModel(jid, message.Participant, action));
                    //ev.GroupParticipantUpdate(jid, message.Participant, action);
                });
                var emitGroupUpdate = new Action<GroupMetadataModel>(update =>
                {
                    ev.Emit(EmitType.Update, new GroupUpdateModel(jid, update));
                    //ev.GroupUpdate(jid, update);
                });

                var participantsIncludesMe = new Func<bool>(() =>
                {
                    return participants.Any(x => JidUtils.AreJidsSameUser(meId, x));
                });

                switch (message.MessageStubType)
                {
                    case WebMessageInfo.Types.StubType.GroupParticipantLeave:
                    case WebMessageInfo.Types.StubType.GroupParticipantRemove:
                        emitParticipantsUpdate("remove");
                        if (participantsIncludesMe())
                        {
                            chat.ReadOnly = true;
                        }
                        break;
                    case WebMessageInfo.Types.StubType.GroupParticipantAdd:
                    case WebMessageInfo.Types.StubType.GroupParticipantInvite:
                    case WebMessageInfo.Types.StubType.GroupParticipantAddRequestJoin:
                        participants = message.MessageStubParameters.ToList();
                        if (participantsIncludesMe())
                        {
                            chat.ReadOnly = true;
                        }
                        emitParticipantsUpdate("add");
                        break;
                    case WebMessageInfo.Types.StubType.GroupParticipantDemote:
                        emitParticipantsUpdate("demote");
                        break;
                    case WebMessageInfo.Types.StubType.GroupParticipantPromote:
                        emitParticipantsUpdate("promote");
                        break;
                    case WebMessageInfo.Types.StubType.GroupChangeAnnounce:
                        emitGroupUpdate(new GroupMetadataModel() { Announce = (message.MessageStubParameters[0] == "true" || message.MessageStubParameters[0] == "on") });
                        break;
                    case WebMessageInfo.Types.StubType.GroupChangeRestrict:
                        emitGroupUpdate(new GroupMetadataModel() { Restrict = (message.MessageStubParameters[0] == "true" || message.MessageStubParameters[0] == "on") });
                        break;
                    case WebMessageInfo.Types.StubType.GroupChangeSubject:
                        chat.Name = message.MessageStubParameters[0];
                        emitGroupUpdate(new GroupMetadataModel() { Subject = chat.Name });
                        break;
                    case WebMessageInfo.Types.StubType.GroupChangeInviteLink:
                        emitGroupUpdate(new GroupMetadataModel() { InviteCode = message.MessageStubParameters[0] });
                        break;
                    case WebMessageInfo.Types.StubType.GroupMemberAddMode:
                        emitGroupUpdate(new GroupMetadataModel() { MemberAddMode = message.MessageStubParameters[0] == "all_member_add" });
                        break;
                    case WebMessageInfo.Types.StubType.GroupMembershipJoinApprovalMode:
                        emitGroupUpdate(new GroupMetadataModel() { JoinApprovalMode = (message.MessageStubParameters[0] == "true" || message.MessageStubParameters[0] == "on") });
                        break;
                    default:
                        break;
                }
            }
            else if (content?.PollUpdateMessage != null)
            {

            }

            ev.Emit(EmitType.Update, chat);
            //ev.ChatsUpdate([chat]);
        }

        private static bool ShouldIncrementChatUnread(WebMessageInfo message)
        {
            return (!message.Key.FromMe && message.MessageStubType == WebMessageInfo.Types.StubType.Unknown);
        }

        private static bool IsRealMessage(WebMessageInfo message, string meId)
        {
            var normalizedContent = MessageUtil.NormalizeMessageContent(message.Message);

            var hasSomeContent = MessageUtil.GetContentType(normalizedContent) != null;

            return (normalizedContent != null
                || Constants.REAL_MSG_STUB_TYPES.Contains(message.MessageStubType)
                || (Constants.REAL_MSG_REQ_ME_STUB_TYPES.Contains(message.MessageStubType) & message.MessageStubParameters.Any(x => JidUtils.AreJidsSameUser(meId, x)))
                )
                & hasSomeContent
                & normalizedContent?.ProtocolMessage == null
                & normalizedContent?.ReactionMessage == null
                & normalizedContent?.PollUpdateMessage == null;
        }

        private static string GetChatID(MessageKey key)
        {
            if (JidUtils.IsBroadcast(key.RemoteJid)
                & JidUtils.IsJidStatusBroadcast(key.RemoteJid)
                & !key.FromMe)
            {
                return key.Participant;
            }
            return key.RemoteJid;
        }

        private static void NormalizeKey(WebMessageInfo message, string meId, MessageKey msgKey)
        {
            // if the reaction is from another user
            // we've to correctly map the key to this user's perspective
            if (!message.Key.FromMe)
            {
                // if the sender believed the message being reacted to is not from them
                // we've to correct the key to be from them, or some other participant
                var participant = msgKey.Participant;
                msgKey.FromMe = msgKey.FromMe == false ? JidUtils.AreJidsSameUser(!string.IsNullOrWhiteSpace(participant) ? participant : msgKey.RemoteJid, meId)
                // if the message being reacted to, was from them
                // fromMe automatically becomes false
                : false;
                // set the remoteJid to being the same as the chat the message came from
                msgKey.RemoteJid = message.Key.RemoteJid;
                // set participant of the message
                msgKey.Participant = msgKey.Participant ?? msgKey.RemoteJid;
            }
        }



    }
}
