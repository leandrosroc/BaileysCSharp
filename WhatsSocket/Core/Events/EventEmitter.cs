﻿using Proto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatsSocket.Core.Models;
using WhatsSocket.Core.Stores;
using WhatsSocket.Core.WABinary;

namespace WhatsSocket.Core.Events
{

    public delegate void EventEmitterHandler<T>(T args);
    public class EventEmitter
    {
        private object locker = new object();
        Dictionary<string, Dictionary<EmitType, IEventStore>> GroupedEvents;


        public EventEmitter()
        {
            GroupedEvents = new Dictionary<string, Dictionary<EmitType, IEventStore>>();
        }

        public BaseSocket Sender { get; }

        public bool Flush(bool force = false)
        {
            lock (locker)
            {
                if (buffersInProgress == 0)
                {
                    return false;
                }

                if (!force)
                {
                    buffersInProgress--;
                    if (buffersInProgress > 0)
                    {
                        return false;
                    }
                }
            }


            foreach (var item in GroupedEvents)
            {
                foreach (var store in item.Value)
                {
                    store.Value.Flush();
                }
            }

            return true;
        }

        public long buffersInProgress = 0;

        public void Buffer()
        {
            lock (locker)
            {
                buffersInProgress++;
            }
        }


        public string[] BufferableEvent = [


            $"{typeof(MessagingHistory)}.{EmitType.Set}",

            $"{typeof(ChatModel)}.{EmitType.Upsert}",
            $"{typeof(ChatModel)}.{EmitType.Upsert}",
            $"{typeof(ChatModel)}.{EmitType.Delete}",

            $"{typeof(ContactModel)}.{EmitType.Upsert}",
            $"{typeof(ContactModel)}.{EmitType.Update}",

            $"{typeof(MessageUpsertModel)}.{EmitType.Upsert}",
            $"{typeof(MessageModel)}.{EmitType.Upsert}",
            $"{typeof(MessageModel)}.{EmitType.Update}",
            $"{typeof(MessageModel)}.{EmitType.Delete}",
            $"{typeof(MessageModel)}.{EmitType.Reaction}",

            //MessageUpsertModel

            //$"MessageReceipt.{EmitType.Update}",
            //$"Group.{EmitType.Reaction}",

            ];

        public bool Emit<T>(EmitType type, params T[] args)
        {
            var eventkey = $"{typeof(T)}.{type}";
            if (!GroupedEvents.ContainsKey(eventkey))
            {
                GroupedEvents[eventkey] = new Dictionary<EmitType, IEventStore>();
            }
            var events = GroupedEvents[eventkey];
            if (!events.ContainsKey(type))
            {
                events[type] = new EventStore<T>(Sender, BufferableEvent.Contains($"{typeof(T)}.{type}"));
            }
            var store = (EventStore<T>)events[type];
            store.Append(args);
            return true;
        }

        public EventStore<T> On<T>(EmitType type)
        {
            var eventkey = $"{typeof(T)}.{type}";
            if (!GroupedEvents.ContainsKey(eventkey))
            {
                GroupedEvents[eventkey] = new Dictionary<EmitType, IEventStore>();
            }
            var events = GroupedEvents[eventkey];
            if (!events.ContainsKey(type))
            {
                events[type] = new EventStore<T>(Sender, BufferableEvent.Contains($"{typeof(T)}.{type}"));
            }
            var store = (EventStore<T>)events[type];
            return store;
        }

        /** Receive an update on a call, including when the call was received, rejected, accepted */
        //'call': WACallEvent[]
        //'labels.edit': Label
        //'labels.association': { association: LabelAssociation, type: 'add' | 'remove' }
    }
}
