using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Assets.Scripts.Contents;
using Assets.Scripts.Contents.Object;

using Clnt;

using Google.Protobuf;

using ServerCore;

using UnityEngine;

namespace Packet
{
    public class PacketHandler
    {
        public static void S_Chat_Handler(Session s, IMessage pkt)
        {
            ClntSession session = s as ClntSession;
            S_Chat packet = pkt as S_Chat;

            Interaction.Instance.OnRecvChat(packet.Id, packet.Str);
            GameObject go = ObjectManager.Instance.GetObj(packet.Id);
            PlayerObject p =go.GetComponent<PlayerObject>();
            p.Chat(packet.Str);
        }
        public static void S_SignUp_Handler(Session s, IMessage pkt)
        {
            ClntSession session = s as ClntSession;
            S_SignUp packet = pkt as S_SignUp;
            ushort state = (ushort)packet.State;

            Interaction.Instance.SignUpCompleted(state);
            NetworkManager._pending = false;
        }
        public static void S_SignIn_Handler(Session s, IMessage pkt)
        {
            ClntSession session = s as ClntSession;
            S_SignIn packet = pkt as S_SignIn;
            
            ushort state = (ushort)packet.State;
            MapManager temp = MapManager.Instance;
            Interaction.Instance.SignInCompleted(state);
            NetworkManager._pending = false;
        }
        public static void S_Enter_Handler(Session s, IMessage pkt)
        {
            ClntSession session = s as ClntSession;
            S_Enter packet = pkt as S_Enter;

            MapManager.Instance.SetMap(packet.MapId);

            ObjectManager.Instance.MakeMyPlayerObj(packet.ObjInfo);
        }
        public static void S_Spawn_Handler(Session s, IMessage pkt)
        {
            ClntSession session = s as ClntSession;
            S_Spawn packet = pkt as S_Spawn;

            foreach(ObjectInfo info in packet.ObjInfo)
            {
               ObjectManager.Instance.MakeObj(info);
            }
        }

        public static void S_Move_Handler(Session s, IMessage pkt)
        {
            ClntSession session = s as ClntSession;
            S_Move packet = pkt as S_Move;

            GameObject go = ObjectManager.Instance.GetObj(packet.ObjId);
            if (go == null)
                return;
            MovingObject movingObj = go.GetComponent<MovingObject>();
            if(movingObj._pos.X == packet.Pos.X && movingObj._pos.Y == packet.Pos.Y)
            {
                movingObj._pos = packet.Pos;
                return;
            }
            if(movingObj._type == ObjectType.Player)
            {
                MapManager.Instance.ClearObjPos(movingObj._pos);
            }
            movingObj._state = ObjectState.Moving;
            movingObj._pos = packet.Pos;
            if (movingObj._type == ObjectType.Player)
            {
                MapManager.Instance.SetObjPos(movingObj as PlayerObject);
            }

        }
        public static void S_DeSpawn_Handler(Session s, IMessage pkt)
        {
            ClntSession session = s as ClntSession;
            S_DeSpawn packet = pkt as S_DeSpawn;

            ObjectManager.Instance.RemoveObj(packet.ObjId);
        }
        public static void S_Leave_Handler(Session s, IMessage pkt)
        {
            ClntSession session = s as ClntSession;
            S_Leave packet = pkt as S_Leave;

            MapManager.Instance.RemoveMap();
            ObjectManager.Instance.RemoveAll();
        }
        public static void S_Battle_Handler(Session s, IMessage pkt)
        {
            ClntSession session = s as ClntSession;
            S_Battle packet = pkt as S_Battle;

            MonsterCp mcp = new MonsterCp();
            mcp._cp = packet.EnemyCp;
            BattleManager.Instance.SetBattle(ObjectManager.Instance._myPlayer._mainMon, mcp);
        }
        public static void S_MonList_Handler(Session s, IMessage pkt)
        {
            ClntSession session = s as ClntSession;
            S_MonList packet = pkt as S_MonList;
            MyPlayerObject myP = ObjectManager.Instance._myPlayer;

            myP.RemoveMonAll();
            foreach (var pairV in packet.Idcps)
            {
                MonsterCp mon = new MonsterCp();
                mon._cp = pairV.Cp;
                mon._objId = pairV.ObjId;
                myP.AddMon(mon);
                if (mon._objId == packet.MainId)
                    myP._mainMon = mon;
            }
        }
        public static void S_AttackResult_Handler(Session s, IMessage pkt)
        {
            ClntSession session = s as ClntSession;
            S_AttackResult packet = pkt as S_AttackResult;

            BattleManager.Instance.OnBattle(packet);
            NetworkManager._pending = false;
        }
        public static void S_AlterMainMon_Handler(Session s, IMessage pkt)
        {
            ClntSession session = s as ClntSession;
            S_AlterMainMon packet = pkt as S_AlterMainMon;

            ObjectManager.Instance._myPlayer.AlterMainMon(packet.Id);
        }
        public static void S_ItemList_Handler(Session s, IMessage pkt)
        {
            ClntSession session = s as ClntSession;
            S_ItemList packet = pkt as S_ItemList;

            ObjectManager.Instance._myPlayer._hpPotionCnt = packet.HpPotion;
        }
        public static void S_UsePotion_Handler(Session s, IMessage pkt)
        {
            ClntSession session = s as ClntSession;
            S_UsePotion packet = pkt as S_UsePotion;

            ObjectManager.Instance._myPlayer._hpPotionCnt--;
            ObjectManager.Instance._myPlayer._mainMon.UsePotion();
        }
        public static void S_NotifyBattleRequest_Handler(Session s, IMessage pkt)
        {
            ClntSession session = s as ClntSession;
            S_NotifyBattleRequest packet = pkt as S_NotifyBattleRequest;

            Interaction.Instance._opponentId = packet.Opponent;
            Interaction.Instance.MakeNotificationWindow($"{packet.Opponent}님의 대결 신청",5);
        }
        public static void S_LeftOpponent_Handler(Session s, IMessage pkt)
        {
            ClntSession session = s as ClntSession;
            S_LeftOpponent packet = pkt as S_LeftOpponent;

            BattleManager.Instance.OnBattleCompleted();
        }
        
    } 
}
