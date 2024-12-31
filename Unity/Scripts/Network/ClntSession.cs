using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Network;
using Google.Protobuf;

using Packet;

using ServerCore;

namespace Clnt
{
    public class ClntSession : PacketSession
    {
        string ToEnumName(string name)
        {
            string ret = "";
            string[] names = name.Split('_');
            ret += names[0];
            ret += (names[1].Substring(0,1) + names[1].Substring(1).ToLower());
            return ret;
        }
        public void Send(IMessage pkt)
        {
            
            string pktName = pkt.Descriptor.Name;
            PktId pktid = (PktId)Enum.Parse(typeof(PktId), ToEnumName(pktName));

            ushort size = (ushort)(pkt.CalculateSize() + 4);

            byte[] buf = new byte[size];
            BitConverter.TryWriteBytes(new Span<byte>(buf, 0, sizeof(ushort)), (ushort)size);
            BitConverter.TryWriteBytes(new Span<byte>(buf, 2, sizeof(ushort)), (ushort)pktid);
            Array.Copy(pkt.ToByteArray(), 0, buf, 4, size - 4);
            Send(new ArraySegment<byte>(buf));

            //var buf = SendBufferHelper.Open(size);
            //Array.Copy(BitConverter.GetBytes((ushort)size), 0, buf.Array, buf.Offset, sizeof(ushort));
            //Array.Copy(BitConverter.GetBytes((ushort)pktid), 0, buf.Array, buf.Offset + 2, sizeof(ushort));
            //Array.Copy(pkt.ToByteArray(), 0, buf.Array, buf.Offset + 4, size - 4);
            //Send(SendBufferHelper.Close(size));
        }

        public override void OnPacket(ArraySegment<byte> segment)
        {
            PacketManager.Instance.OnPacket(this, segment);
        }
        public override void OnConnected()
        {
            PacketManager.Push = (a, s, p) => { PacketQueue.Push(new PacketQueueItem(a, s, p)); };
        }

        public override void OnDisconnected(EndPoint point) { }



        public override void OnSend() { }
    }
}
