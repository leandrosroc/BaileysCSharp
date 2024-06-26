﻿using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Proto;
using QRCoder;
using System.Buffers;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using WhatsSocket.Core;
using WhatsSocket.Core.Curve;
using WhatsSocket.Core.Events;
using WhatsSocket.Core.Helper;
using WhatsSocket.Core.Stores;
using WhatsSocket.Core.Models.SenderKeys;
using WhatsSocket.Core.Models;
using WhatsSocket.Core.NoSQL;
using WhatsSocket.Core.Extensions;
using WhatsSocket.Core.Sockets;
using WhatsSocket.Exceptions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Google.Protobuf;

namespace WhatsSocketConsole
{
    internal class Program
    {

        static WASocket socket;
        static void Main(string[] args)
        {
            Tests.RunTests();



            //This creds file comes from the nodejs sample    





            var config = new SocketConfig()
            {
                ID = "27665245067",
            };


            var credsFile = Path.Join(config.CacheRoot, "creds.json");
            AuthenticationCreds? authentication = null;
            if (File.Exists(credsFile))
            {
                authentication = AuthenticationCreds.Deserialize(File.ReadAllText(credsFile));
            }
            authentication = authentication ?? AuthenticationUtils.InitAuthCreds();

            BaseKeyStore keys = new FileKeyStore(config.CacheRoot);

            config.Auth = new AuthenticationState()
            {
                Creds = authentication,
                Keys = keys
            };

            socket = new WASocket(config);


            var authEvent = socket.EV.On<AuthenticationCreds>(EmitType.Update);
            authEvent.Multi += AuthEvent_OnEmit;

            var connectionEvent = socket.EV.On<ConnectionState>(EmitType.Update);
            connectionEvent.Multi += ConnectionEvent_Emit;


            var messageEvent = socket.EV.On<MessageUpsertModel>(EmitType.Upsert);
            messageEvent.Single += MessageEvent_Single;

            var history = socket.EV.On<MessageHistoryModel>(EmitType.Set);
            history.Multi += History_Emit;

            var presence = socket.EV.On<PresenceModel>(EmitType.Update);
            presence.Multi += Presence_Emit;

            //socket.EV.OnCredsChange += Socket_OnCredentialsChangeArgs;
            //socket.EV.OnDisconnect += EV_OnDisconnect;
            //socket.EV.OnQR += EV_OnQR;
            //socket.EV.OnMessageUpserted += EV_OnMessageUpserted;


            socket.MakeSocket();

            Console.ReadLine();
        }

        private static async void MessageEvent_Single(MessageUpsertModel args)
        {
            if (args.Type == MessageUpsertType.Notify)
            {
                var msg = args.Messages[0];
                if (msg.Key.FromMe == false)
                {
                    var result = await socket.SendMessage(msg.Key.RemoteJid,
                        new ExtendedTextMessageModel() { Text = "oh hello there" },
                        new MessageGenerationOptionsFromContent());
                }
            }
        }

        private static void Presence_Emit(PresenceModel[] args)
        {
            Console.WriteLine(JsonConvert.SerializeObject(args[0], Formatting.Indented));
        }

        private static void History_Emit(MessageHistoryModel[] args)
        {
            messages.AddRange(args[0].Messages);
            var jsons = messages.Select(x => x.ToJson()).ToArray();
            var array = $"[\n{string.Join(",", jsons)}\n]";
            Debug.WriteLine(array);
        }

        static List<WebMessageInfo> messages = new List<WebMessageInfo>();

        private static async void MessageEvent_Emit(MessageUpsertModel[] args)
        {
            messages.AddRange(args[0].Messages);
            var jsons = messages.Select(x => x.ToJson()).ToArray();
            var array = $"[\n{string.Join(",", jsons)}\n]";
            Debug.WriteLine(array);

            if (args[0].Type == MessageUpsertType.Notify)
            {
                var msg = args[0].Messages[0];
                if (msg.Key.FromMe == false)
                {
                    //var result = await socket.SendMessage(msg.Key.RemoteJid,
                    //    new ExtendedTextMessageModel() { Text = "oh hello there" },
                    //    new MessageGenerationOptionsFromContent());
                }
            }
        }

        private static async void ConnectionEvent_Emit(ConnectionState[] args)
        {
            var connection = args[0];
            Debug.WriteLine(JsonConvert.SerializeObject(connection, Formatting.Indented));
            if (connection.QR != null)
            {
                QRCodeGenerator QrGenerator = new QRCodeGenerator();
                QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(connection.QR, QRCodeGenerator.ECCLevel.L);
                AsciiQRCode qrCode = new AsciiQRCode(QrCodeInfo);
                var data = qrCode.GetGraphic(1);
                Console.WriteLine(data);
            }
            if (connection.Connection == WAConnectionState.Close)
            {
                if (connection.LastDisconnect.Error is Boom boom && boom.Data?.StatusCode != (int)DisconnectReason.LoggedOut)
                {
                    try
                    {
                        Thread.Sleep(1000);
                        socket.MakeSocket();
                    }
                    catch (Exception)
                    {

                    }
                }
                else
                {
                    Console.WriteLine("You are logged out");
                }
            }


            if (connection.Connection == WAConnectionState.Open)
            {
            }
        }

        private static void AuthEvent_OnEmit(AuthenticationCreds[] args)
        {
            var credsFile = Path.Join(socket.SocketConfig.CacheRoot, $"creds.json");
            var json = AuthenticationCreds.Serialize(args[0]);
            File.WriteAllText(credsFile, json);
        }





        private static void EV_OnDisconnect(BaseSocket sender, DisconnectReason args)
        {
            if (args != DisconnectReason.LoggedOut)
            {
                sender.MakeSocket();
            }
            else
            {
                Directory.Delete(Path.Join(Directory.GetCurrentDirectory(), "test"), true);
                sender.NewAuth();
                sender.MakeSocket();
            }
        }




    }
}
