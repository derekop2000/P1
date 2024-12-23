
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using ServerCore;
namespace Packet
{
    public class PacketManager
    {
        public static Action<Action<Session, IMessage>,Session,IMessage> Push  = null;
        private static PacketManager _packetManager = new PacketManager();
        public static PacketManager Instance  { get{ return _packetManager; } }
        Dictionary<ushort, Action<Session, ArraySegment<byte>, ushort>> IdToMakePacket = new Dictionary<ushort, Action<Session, ArraySegment<byte>, ushort>>();
        Dictionary<ushort, Action<Session, IMessage>> IdToPacketHandler = new Dictionary<ushort, Action<Session, IMessage>>();
        public void Init()
        {
            
IdToMakePacket.Add((ushort)PktId.CChat, MakePacket<C_Chat>);
IdToPacketHandler.Add((ushort)PktId.CChat, PacketHandler.C_Chat_Handler);


IdToMakePacket.Add((ushort)PktId.CSignup, MakePacket<C_SignUp>);
IdToPacketHandler.Add((ushort)PktId.CSignup, PacketHandler.C_SignUp_Handler);


IdToMakePacket.Add((ushort)PktId.CSignin, MakePacket<C_SignIn>);
IdToPacketHandler.Add((ushort)PktId.CSignin, PacketHandler.C_SignIn_Handler);


IdToMakePacket.Add((ushort)PktId.CMove, MakePacket<C_Move>);
IdToPacketHandler.Add((ushort)PktId.CMove, PacketHandler.C_Move_Handler);


IdToMakePacket.Add((ushort)PktId.CAttacktype, MakePacket<C_AttackType>);
IdToPacketHandler.Add((ushort)PktId.CAttacktype, PacketHandler.C_AttackType_Handler);


IdToMakePacket.Add((ushort)PktId.CAltermainmon, MakePacket<C_AlterMainMon>);
IdToPacketHandler.Add((ushort)PktId.CAltermainmon, PacketHandler.C_AlterMainMon_Handler);


IdToMakePacket.Add((ushort)PktId.CBattlecompleted, MakePacket<C_BattleCompleted>);
IdToPacketHandler.Add((ushort)PktId.CBattlecompleted, PacketHandler.C_BattleCompleted_Handler);


IdToMakePacket.Add((ushort)PktId.CUsepotion, MakePacket<C_UsePotion>);
IdToPacketHandler.Add((ushort)PktId.CUsepotion, PacketHandler.C_UsePotion_Handler);


IdToMakePacket.Add((ushort)PktId.CRequestbattle, MakePacket<C_RequestBattle>);
IdToPacketHandler.Add((ushort)PktId.CRequestbattle, PacketHandler.C_RequestBattle_Handler);


IdToMakePacket.Add((ushort)PktId.CResponebattlerequest, MakePacket<C_ResponeBattleRequest>);
IdToPacketHandler.Add((ushort)PktId.CResponebattlerequest, PacketHandler.C_ResponeBattleRequest_Handler);


        }
        public void OnPacket(Session s, ArraySegment<byte> segment)
        {
            int cnt = 0;
            ushort packetSize = BitConverter.ToUInt16(segment.Array, segment.Offset+cnt);
            cnt += sizeof(ushort);
            ushort packetId = BitConverter.ToUInt16(segment.Array, segment.Offset + cnt);

            Action<Session, ArraySegment<byte>, ushort> makePacketFunc = null;
            if(IdToMakePacket.TryGetValue(packetId,out makePacketFunc))
                makePacketFunc(s, segment, packetId);
            
        }
        public void MakePacket<T>(Session s,ArraySegment<byte> segment,ushort packetId) where T : IMessage, new()
        {
            T packet = new T();
            packet.MergeFrom(segment.Array, segment.Offset + 4, segment.Count - 4);

            Action<Session, IMessage> packetHandler = null;
            if (IdToPacketHandler.TryGetValue(packetId, out packetHandler))
            {
                if (Push != null)
                    Push(packetHandler, s, packet);
                else
                    packetHandler(s, packet);
            }
        }
    }
}
