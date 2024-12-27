using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Protobuf;
using Server.Contents.Object;
using Server.Contents.Room;
using Server.DB;
using Server.Session;
using ServerCore;

namespace Packet
{
    public class PacketHandler
    {
        public static void C_Chat_Handler(Session s , IMessage pkt)
        {
            ServerSession session = s as ServerSession;
            C_Chat packet = pkt as C_Chat;

            GameRoom room = session._player._room;
            if (room != null)
            {
                S_Chat chatPacket = new S_Chat();
                chatPacket.Id = session._player._objId;
                chatPacket.Str = packet.Str;
                room.Push(room.BroadCast, chatPacket);
            }
        }
        public static void C_SignUp_Handler(Session s, IMessage pkt)
        {
            ServerSession session = s as ServerSession;
            C_SignUp packet = pkt as C_SignUp;

            DbConnector con = DbPool.Instance.Pop();
            con._command.CommandText = $"select count(*) as cnt from user_login where username = \"{packet.Id}\"";

            bool exist = false;
            using (OdbcDataReader reader = con._command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if(reader.GetInt32(reader.GetOrdinal("cnt"))>0)
                        exist = true;
                }
            }

            S_SignUp replyPkt = new S_SignUp();
            if(exist)
            {
                replyPkt.State = 1;
            }
            else
            {
                con._command.CommandText = $"insert into user_login (username,password) values (\"{packet.Id}\",\"{packet.Password}\")";
                if (con._command.ExecuteNonQuery() > 0)
                {
                    replyPkt.State = 0;
                }
                else
                {
                    replyPkt.State = 2;
                }
            }
            DbPool.Instance.Push(con);

            session.Send(replyPkt);
        }

        public static void C_SignIn_Handler(Session s, IMessage pkt)
        {
            ServerSession session = s as ServerSession;
            C_SignIn packet = pkt as C_SignIn;

            DbConnector con = DbPool.Instance.Pop();

            con._command.CommandText = $"select * from user_login where username = \"{packet.Id}\"";
            ushort state = 1;
            using(OdbcDataReader reader =  con._command.ExecuteReader())
            {
                while(reader.Read())
                {
                    string password =  reader.GetString(reader.GetOrdinal("password"));
                    if (password == packet.Password)
                    {
                        state = 0;
                        session._dataBaseId = reader.GetInt32(reader.GetOrdinal("userid"));
                    }
                }
            }
            S_SignIn replyPkt = new S_SignIn();
            replyPkt.State = state;
            session.Send(replyPkt);

            if (state != 0)
                return;
            GameRoom room = RoomManager.Instance.GetRoom(1);

            Player player = new Player();
            player._session = session;
            session._player = player;

            using(OdbcCommand potionCommand = con._connection.CreateCommand())
            {
                potionCommand.CommandText = $"select * from items where ownerid = {session._dataBaseId}";
                using(OdbcDataReader potionReader = potionCommand.ExecuteReader())
                {
                    bool exist = false;
                    while(potionReader.Read())
                    {
                        exist = true;
                        session._player._hpPotionCnt = potionReader.GetInt32(potionReader.GetOrdinal("hppotion"));
                    }
                    if(exist == false)
                    {
                        session._player._hpPotionCnt = 3;
                        using(OdbcCommand command = con._connection.CreateCommand())
                        {
                            command.CommandText = $"insert into items (ownerid,hppotion) values({session._dataBaseId},3)";
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }

            con._command.CommandText = $"select * from pets where ownerid = {session._dataBaseId}";
            using(OdbcDataReader reader = con._command.ExecuteReader())
            {
                int cnt = 0;
                while (reader.Read())
                {
                    cnt++;
                    MyMonster m = new MyMonster(0);
                    m._cp.Hp = reader.GetInt32(reader.GetOrdinal("Hp"));
                    m._cp.MaxHp = reader.GetInt32(reader.GetOrdinal("maxHp"));
                    m._cp.HpIncrease = reader.GetInt32(reader.GetOrdinal("HpIncrease"));
                    m._cp.Damage = reader.GetInt32(reader.GetOrdinal("Damage"));
                    m._cp.DamageIncrease = reader.GetInt32(reader.GetOrdinal("DamageIncrease"));
                    m._cp.MonNum = reader.GetInt32(reader.GetOrdinal("MonNum"));
                    m._cp.Exp = reader.GetInt32(reader.GetOrdinal("Exp"));
                    m._cp.MaxExp = reader.GetInt32(reader.GetOrdinal("MaxExp"));
                    m._cp.RewardExp = reader.GetInt32(reader.GetOrdinal("RewardExp"));
                    m._cp.Level = reader.GetInt32(reader.GetOrdinal("Level"));

                    player.AddMon(m);
                    if (player._mainMon == null)
                        player._mainMon = m;
                }
                if (cnt == 0)
                {
                    MyMonster m = new MyMonster(1);
                    
                    using(OdbcCommand command = con._connection.CreateCommand())
                    {
                        command.CommandText = $"insert into pets (ownerid,maxhp,hp,hpIncrease,damage,damageIncrease,monNum,level,exp,maxExp,rewardExp) values({session._dataBaseId},{m._cp.MaxHp},{m._cp.Hp},{m._cp.HpIncrease},{m._cp.Damage},{m._cp.DamageIncrease},{m._cp.MonNum},{m._cp.Level},{m._cp.Exp},{m._cp.MaxExp},{m._cp.RewardExp})";
                        command.ExecuteNonQuery();
                    }
                    player.AddMon(m);
                    player._mainMon = m;
                }
            }

            DbPool.Instance.Push(con);
            room.Push(room.Enter, player);
        }
        public static void C_Move_Handler(Session s, IMessage pkt)
        {
            ServerSession session = s as ServerSession;
            C_Move packet = pkt as C_Move;

            GameRoom room = session._player._room;
            if (room == null)
                return;
            room.Push(room.Move,session, packet.Pos);
        }
        public static void C_AttackType_Handler(Session s, IMessage pkt)
        {
            ServerSession session = s as ServerSession;
            C_AttackType packet = pkt as C_AttackType;

            GameRoom room = session._player._room;
            if(room == null)
                return;
            
            session._player._attackType = packet.AttackType;
            room.Push(room.OnBattle, session._player._battleRoomId);
        }
        public static void C_BattleCompleted_Handler(Session s, IMessage pkt)
        {
            ServerSession session = s as ServerSession;
            C_BattleCompleted packet = pkt as C_BattleCompleted;

            GameRoom room = session._player._room;
            if(room == null)
                return;
            room.Push(room.ResetPlayer, session._player._objId);
        }
        public static void C_AlterMainMon_Handler(Session s, IMessage pkt)
        {
            ServerSession session = s as ServerSession;
            C_AlterMainMon packet = pkt as C_AlterMainMon;

            GameRoom room = session._player._room;
            if (room == null)
                return;
            room.Push(room.AlterMainMon, session._player._objId,packet.Id);
        }
        public static void C_UsePotion_Handler(Session s, IMessage pkt)
        {
            ServerSession session = s as ServerSession;
            C_UsePotion packet = pkt as C_UsePotion;

            GameRoom room = session._player._room;
            if (room == null)
                return;
            room.Push(room.UsePotion, session);
        }
        public static void C_RequestBattle_Handler(Session s, IMessage pkt)
        {
            ServerSession session = s as ServerSession;
            C_RequestBattle packet = pkt as C_RequestBattle;

            GameRoom room = session._player._room;
            if (room == null)
                return;
            room.Push(room.RequestBattle, session._player._objId,packet.Opponent);
        }
        public static void C_ResponeBattleRequest_Handler(Session s, IMessage pkt)
        {
            ServerSession session = s as ServerSession;
            C_ResponeBattleRequest packet = pkt as C_ResponeBattleRequest;

            GameRoom room = session._player._room;
            if (room == null)
                return;
            room.Push(room.MakeBattleRoom,packet.P1,packet.P2);
        }
        
    }
}
