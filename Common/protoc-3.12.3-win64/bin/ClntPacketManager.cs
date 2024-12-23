
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
            
IdToMakePacket.Add((ushort)PktId.SChat, MakePacket<S_Chat>);
IdToPacketHandler.Add((ushort)PktId.SChat, PacketHandler.S_Chat_Handler);


IdToMakePacket.Add((ushort)PktId.SSignup, MakePacket<S_SignUp>);
IdToPacketHandler.Add((ushort)PktId.SSignup, PacketHandler.S_SignUp_Handler);


IdToMakePacket.Add((ushort)PktId.SSignin, MakePacket<S_SignIn>);
IdToPacketHandler.Add((ushort)PktId.SSignin, PacketHandler.S_SignIn_Handler);


IdToMakePacket.Add((ushort)PktId.SEnter, MakePacket<S_Enter>);
IdToPacketHandler.Add((ushort)PktId.SEnter, PacketHandler.S_Enter_Handler);


IdToMakePacket.Add((ushort)PktId.SSpawn, MakePacket<S_Spawn>);
IdToPacketHandler.Add((ushort)PktId.SSpawn, PacketHandler.S_Spawn_Handler);


IdToMakePacket.Add((ushort)PktId.SMove, MakePacket<S_Move>);
IdToPacketHandler.Add((ushort)PktId.SMove, PacketHandler.S_Move_Handler);


IdToMakePacket.Add((ushort)PktId.SDespawn, MakePacket<S_DeSpawn>);
IdToPacketHandler.Add((ushort)PktId.SDespawn, PacketHandler.S_DeSpawn_Handler);


IdToMakePacket.Add((ushort)PktId.SLeave, MakePacket<S_Leave>);
IdToPacketHandler.Add((ushort)PktId.SLeave, PacketHandler.S_Leave_Handler);


IdToMakePacket.Add((ushort)PktId.SBattle, MakePacket<S_Battle>);
IdToPacketHandler.Add((ushort)PktId.SBattle, PacketHandler.S_Battle_Handler);


IdToMakePacket.Add((ushort)PktId.SMonlist, MakePacket<S_MonList>);
IdToPacketHandler.Add((ushort)PktId.SMonlist, PacketHandler.S_MonList_Handler);


IdToMakePacket.Add((ushort)PktId.SAttackresult, MakePacket<S_AttackResult>);
IdToPacketHandler.Add((ushort)PktId.SAttackresult, PacketHandler.S_AttackResult_Handler);


IdToMakePacket.Add((ushort)PktId.SAltermainmon, MakePacket<S_AlterMainMon>);
IdToPacketHandler.Add((ushort)PktId.SAltermainmon, PacketHandler.S_AlterMainMon_Handler);


IdToMakePacket.Add((ushort)PktId.SItemlist, MakePacket<S_ItemList>);
IdToPacketHandler.Add((ushort)PktId.SItemlist, PacketHandler.S_ItemList_Handler);


IdToMakePacket.Add((ushort)PktId.SUsepotion, MakePacket<S_UsePotion>);
IdToPacketHandler.Add((ushort)PktId.SUsepotion, PacketHandler.S_UsePotion_Handler);


IdToMakePacket.Add((ushort)PktId.SNotifybattlerequest, MakePacket<S_NotifyBattleRequest>);
IdToPacketHandler.Add((ushort)PktId.SNotifybattlerequest, PacketHandler.S_NotifyBattleRequest_Handler);


IdToMakePacket.Add((ushort)PktId.SLeftopponent, MakePacket<S_LeftOpponent>);
IdToPacketHandler.Add((ushort)PktId.SLeftopponent, PacketHandler.S_LeftOpponent_Handler);


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
