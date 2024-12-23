using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Protobuf;

using Server.Session;

namespace Server.Contents.Object
{
    public class Player : MovingObject
    {
        public ServerSession _session;
        public MyMonster _mainMon = null;
        public Dictionary<int, MyMonster> _mons = new Dictionary<int, MyMonster>();
        public static int _maxMonCnt = 3;
        public int _battleRoomId;
        public int _currentMonCnt { get { return _mons.Count; } }
        public int _hpPotionCnt = 0;
        public Player()
        {
            _type = ObjectType.Player;
        }
        
        public void GetExp(int exp)
        {
            ObjectCP cp = _mainMon._cp;
            cp.Exp += exp;
            if (cp.Exp >= cp.MaxExp)
                LevelUp();
        }
        public void LevelUp()
        {
            ObjectCP cp = _mainMon._cp;
            cp.MaxHp += cp.HpIncrease;
            cp.Hp += cp.HpIncrease;
            cp.Damage += cp.DamageIncrease;
            cp.Exp = 0;
            cp.Level++;
        }
        public void SendMonList()
        {
            if (_mainMon == null)
                return;
            S_MonList monListPacket = new S_MonList();
            monListPacket.MainId = _mainMon._objId;
            foreach (MyMonster myMon in _mons.Values)
            {
                IdCp idcp = new IdCp();
                idcp.ObjId = myMon._objId;
                idcp.Cp = myMon._cp;
                monListPacket.Idcps.Add(idcp);
            }
            _session.Send(monListPacket);
        }
        public void AddMon(MyMonster m)
        {
            if (_mons.Count == _maxMonCnt)
                return;
            _mons.Add(m._objId, m);
        }
        public void RemoveMon(MyMonster m)
        {
            _mons.Remove(m._objId);
        }
        public void AlterMainMon(int id = -1)
        {
            if(id == -1)
            {
                _mainMon = null;
                foreach (MyMonster m in _mons.Values)
                {
                    if (m._cp.Hp > 0)
                    {
                        _mainMon = m;
                        S_AlterMainMon packet = new S_AlterMainMon();
                        packet.Id = _mainMon._objId;
                        _session.Send(packet);
                        break;
                    }
                }
            }
            else
            {
                MyMonster m = null;
                if (_mons.TryGetValue(id, out m) == false)
                    return;
                _mainMon = m;
                S_AlterMainMon packet = new S_AlterMainMon();
                packet.Id = _mainMon._objId;
                _session.Send(packet);
            }
        }
    }
}
