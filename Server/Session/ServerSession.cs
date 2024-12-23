using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Google.Protobuf;

using Packet;
using Server.Contents.Object;
using Server.Contents.Room;
using Server.DB;
using ServerCore;

namespace Server.Session
{
    public class ServerSession : PacketSession
    {
        public int _dataBaseId;
        public Player _player=null;
        string ToEnumName(string name)
        {
            string ret = "";
            string[] names = name.Split('_');
            ret += names[0];
            ret += names[1].Substring(0, 1) + names[1].Substring(1).ToLower();
            return ret;
        }

        public void Send(IMessage pkt)
        {
            string pktName = pkt.Descriptor.Name;
            PktId pktid = (PktId)Enum.Parse(typeof(PktId), ToEnumName(pktName));

            ushort size = (ushort)(pkt.CalculateSize() + 4);

            byte[] buf = new byte[size];
            BitConverter.TryWriteBytes(new Span<byte>(buf, 0, sizeof(ushort)), size);
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
            Console.WriteLine(_sock.RemoteEndPoint.ToString());
        }

        public override void OnDisconnected(EndPoint point)
        {
            if (_player == null)
                return;
            GameRoom room = _player._room;
            if (room != null)
            {
                room.Push(room.Leave, _player);
            }

            { // DB
                DbConnector con = DbPool.Instance.Pop();
                con._command.CommandText = $"delete from pets where ownerid = {_dataBaseId}";
                con._command.ExecuteNonQuery();

                foreach(MyMonster m in _player._mons.Values)
                {
                    con._command.CommandText = $"insert into pets (ownerid,hp,hpIncrease,damage,damageIncrease,monNum,level,exp,maxExp,rewardExp,maxHp) values({_dataBaseId},{m._cp.Hp},{m._cp.HpIncrease},{m._cp.Damage},{m._cp.DamageIncrease},{m._cp.MonNum},{m._cp.Level},{m._cp.Exp},{m._cp.MaxExp},{m._cp.RewardExp},{m._cp.MaxHp})";
                    con._command.ExecuteNonQuery();
                }

                using(OdbcCommand command = con._connection.CreateCommand())
                {
                    command.CommandText = $"update items set hppotion = {_player._hpPotionCnt} where ownerid = {_dataBaseId}";
                    command.ExecuteNonQuery();
                }

                DbPool.Instance.Push(con);
            }
            _player = null;
        }
        
        public override void OnSend()
        {
        }
    }
}
