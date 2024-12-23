using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace PacketTool
{
    public class StringFrame
    {
        // 0 - packetFrame

        public static string _mainFrame =

@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using ServerCore;
namespace Packet
{{
    public class PacketManager
    {{
        public static Action<Action<Session, IMessage>,Session,IMessage> Push  = null;
        private static PacketManager _packetManager = new PacketManager();
        public static PacketManager Instance  {{ get{{ return _packetManager; }} }}
        Dictionary<ushort, Action<Session, ArraySegment<byte>, ushort>> IdToMakePacket = new Dictionary<ushort, Action<Session, ArraySegment<byte>, ushort>>();
        Dictionary<ushort, Action<Session, IMessage>> IdToPacketHandler = new Dictionary<ushort, Action<Session, IMessage>>();
        public void Init()
        {{
            {0}
        }}
        public void OnPacket(Session s, ArraySegment<byte> segment)
        {{
            int cnt = 0;
            ushort packetSize = BitConverter.ToUInt16(segment.Array, segment.Offset+cnt);
            cnt += sizeof(ushort);
            ushort packetId = BitConverter.ToUInt16(segment.Array, segment.Offset + cnt);

            Action<Session, ArraySegment<byte>, ushort> makePacketFunc = null;
            if(IdToMakePacket.TryGetValue(packetId,out makePacketFunc))
                makePacketFunc(s, segment, packetId);
            
        }}
        public void MakePacket<T>(Session s,ArraySegment<byte> segment,ushort packetId) where T : IMessage, new()
        {{
            T packet = new T();
            packet.MergeFrom(segment.Array, segment.Offset + 4, segment.Count - 4);

            Action<Session, IMessage> packetHandler = null;
            if (IdToPacketHandler.TryGetValue(packetId, out packetHandler))
            {{
                if (Push != null)
                    Push(packetHandler, s, packet);
                else
                    packetHandler(s, packet);
            }}
        }}
    }}
}}
";

        // 0 - packetName(enum)
        // 1 - packetName

        public static string _packetFrame =
@"
IdToMakePacket.Add((ushort)PktId.{0}, MakePacket<{1}>);
IdToPacketHandler.Add((ushort)PktId.{0}, PacketHandler.{1}_Handler);
";
    }
}
