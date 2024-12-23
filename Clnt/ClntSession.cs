using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Google.Protobuf;

using Packet;

using ServerCore;

namespace Clnt
{
    public class ClntSession : PacketSession
    {
        public void Send(IMessage pkt)
        {
            string pktName = pkt.Descriptor.Name;
            PktId pktid = (PktId)Enum.Parse(typeof(PktId), pktName.Replace("_", string.Empty));

            ushort size = (ushort)(pkt.CalculateSize() + 4);

            byte[] buf = new byte[size];
            BitConverter.TryWriteBytes(new Span<byte>(buf, 0, sizeof(ushort)), (ushort)size);
            BitConverter.TryWriteBytes(new Span<byte>(buf, 2, sizeof(ushort)), (ushort)pktid);
            Array.Copy(pkt.ToByteArray(), 0, buf, 4, size - 4);
            Send(new ArraySegment<byte>(buf));

            //var buf = SendBufferHelper.Open(size);
            //BitConverter.TryWriteBytes(new Span<byte>(buf.Array, buf.Offset, sizeof(ushort)), (ushort)size);
            //BitConverter.TryWriteBytes(new Span<byte>(buf.Array, buf.Offset + 2, sizeof(ushort)), (ushort)pktid);
            //Array.Copy(pkt.ToByteArray(), 0, buf.Array, buf.Offset + 4, size - 4);
            //Send(SendBufferHelper.Close(size));
        }
        public override void OnPacket(ArraySegment<byte> segment)
        {
            PacketManager.Instance.OnPacket(this, segment);
        }
        public override void OnConnected()
        {
            //C_Chat pkt = new C_Chat();
            //pkt.Str = $"connect!";
            //Send(pkt);
        }

        public override void OnDisconnected(EndPoint point)
        {
            smanager.OutS(this);
        }



        public override void OnSend()
        {
        }
    }
}
