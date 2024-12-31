using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Protobuf;

using ServerCore;

namespace Assets.Scripts.Network
{
    public class PacketQueueItem
    {
        Action<Session, IMessage> _act;
        Session _session;
        IMessage _pkt;
        public PacketQueueItem(Action<Session, IMessage> act, Session session, IMessage pkt)
        {
            _act = act;
            _session = session;
            _pkt = pkt;
        }
        public void Execute()
        {
            _act(_session, _pkt);
        }
    }

    public class PacketQueue
    {
        public static Queue<PacketQueueItem> _queue = new Queue<PacketQueueItem>();
        static object _lock = new object();
        public static void Push(PacketQueueItem item)
        {
            lock (_lock)
            {
                _queue.Enqueue(item);
            }
        }
        public static Queue<PacketQueueItem> PopAll()
        {
            Queue<PacketQueueItem> ret = new Queue<PacketQueueItem>();
            lock (_lock)
            {
                while(_queue.Count > 0)
                {
                    ret.Enqueue(_queue.Dequeue());
                }
            }
            return ret;
        }
    }
}
