using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Google.Protobuf;

using Server.Contents.Object;
using Server.Job;
using Server.Session;
using Server.Utils;

using ServerCore;

namespace Server.Contents.Room
{
    public class GameRoom : JobQueue
    {
        public int _mapId;
        Dictionary<int, ServerSession> _sessions = new Dictionary<int, ServerSession>();
        Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
        Dictionary<int, Monster> _mons = new Dictionary<int, Monster>();
        Dictionary<int, BattleRoom> _battleRooms = new Dictionary<int, BattleRoom>();
        public Map _map = new Map();
        public void Init(int mapId)
        {
            _mapId = mapId;
            _map.Init(mapId);

            {
                switch (mapId)
                {
                    case 1:
                        {
                            {
                                Portal portal = new Portal(2);
                                portal._pos = _map.GetRightCellPos();
                                Enter(portal);
                            }
                            break;
                        }
                    case 2:
                        {
                            {
                                Portal portal = new Portal(1);
                                portal._pos = _map.GetLeftCellPos();
                                Enter(portal);
                            }
                            {
                                Portal portal = new Portal(3);
                                portal._pos = _map.GetRightCellPos();
                                Enter(portal);
                            }
                            break;
                        }
                    case 3:
                        {
                            {
                                Portal portal = new Portal(2);
                                portal._pos = _map.GetLeftCellPos();
                                Enter(portal);

                            }
                            break;
                        }
                    default: break;
                }
            } // to do 포탈 초기화 데이터화
            {
                for (int j = 0; j < 0; j++)
                {
                    Monster m = new Monster(_mapId);
                    m._pos = _map.GetCenterCellPos();
                    Enter(m);
                }
            } // 몬스터 초기화
        }
        public void Enter(GameObject obj)
        {
            ObjectType type = obj._objectInfo.ObjectType;
            switch (type)
            {
                case ObjectType.Player:
                    {
                        _objects.Add(obj._objId, obj);
                        Player p = obj as Player;
                        _sessions.Add(obj._objId, p._session);
                        p._room = this;

                        p._pos = _map.GetCenterCellPos();
                        _map.SetObjPos(p);

                        S_Enter enterPkt = new S_Enter();
                        enterPkt.MapId = _mapId;
                        enterPkt.ObjInfo = p._objectInfo;
                        p._session.Send(enterPkt);

                        S_Spawn spawnPkt = new S_Spawn();
                        foreach (GameObject go in _objects.Values)
                        {
                            if (go._objId != p._objId)
                                spawnPkt.ObjInfo.Add(go._objectInfo);
                        }
                        foreach (GameObject m in _mons.Values)
                        {
                            if (m._objId != p._objId)
                                spawnPkt.ObjInfo.Add(m._objectInfo);
                        }
                        p._session.Send(spawnPkt);

                        p.SendMonList();

                        S_ItemList item = new S_ItemList();
                        item.HpPotion = p._hpPotionCnt;
                        p._session.Send(item);

                        break;
                    }
                case ObjectType.Monster:
                    {
                        Monster m = obj as Monster;
                        m._room = this;
                        _map.SetObjPos(m);
                        _mons.Add(m._objId, m);
                        break;
                    }
                case ObjectType.Portal:
                    {
                        Portal por = obj as Portal;
                        _map.SetObjPos(por);
                        _objects.Add(obj._objId, obj);
                        break;
                    }
                case ObjectType.Normal:
                    {
                        break;
                    }
                default:
                    break;
            }

            S_Spawn spawnPacket = new S_Spawn();
            spawnPacket.ObjInfo.Add(obj._objectInfo);

            foreach (ServerSession s in _sessions.Values)
            {
                if (obj._objId != s._player._objId)
                {
                    s.Send(spawnPacket);
                }
            }
        }
        public void Leave(GameObject go)
        {
            if (go._type == ObjectType.Player)
            {
                S_Leave leavePacket = new S_Leave();
                _sessions[go._objId].Send(leavePacket);

                if(go._state == ObjectState.Fight)
                {
                    LeaveBattleRoom(go);
                }
                //to do after leave battledRoom
            }
            _map.ClearObjPos(go._pos);
            _objects.Remove(go._objId);
            _sessions.Remove(go._objId);
            _mons.Remove(go._objId);
            S_DeSpawn packet = new S_DeSpawn();
            packet.ObjId = go._objId;
            BroadCast(packet);
        }
        public void BroadCast(IMessage pkt)
        {
            foreach (ServerSession session in _sessions.Values)
            {
                session.Send(pkt);
            }
        }
        public void BroadCast(IMessage pkt, ServerSession s)
        {
            foreach (ServerSession session in _sessions.Values)
            {
                if (session == s)
                    continue;
                session.Send(pkt);
            }
        }
        public void Respawn(GameObject go)
        {
            Leave(go);
            go._pos = _map.GetCenterCellPos();
            Enter(go);
        }
        public void Move(ServerSession s, Pos pos)
        {
            int moveResult = _map.CanGo(s._player, pos);
            S_Move packet = new S_Move();
            int objId = s._player._objId;
            packet.ObjId = objId;
            if (moveResult == 1)
            {
                _map.ClearObjPos(s._player._pos);
                s._player._pos = pos;
                _map.SetObjPos(s._player);
                packet.Pos = pos;
                BroadCast(packet, s);
            }
            else if (moveResult == 0)
            {
                s._player._pos.Dir = pos.Dir;
                packet.Pos = s._player._pos;
                BroadCast(packet, s);
            }
            else if (moveResult == 2)
            {
                GameObject destGo = _map.GetObj(pos);
                if (destGo != null && destGo._canCollision)
                    destGo.OnCollision(s._player);
            }
        }
        public void Update()
        {
            foreach (Monster m in _mons.Values)
            {
                m.Update();
            }

            foreach (BattleRoom br in _battleRooms.Values)
            {
                br.Update();
            }
        }
        #region Battle Code
        public void MakeBattleRoom(Player p, Monster m)
        {
            S_Battle packet = new S_Battle();
            packet.EnemyCp = m._cp;
            p._session.Send(packet);

            int roomId = BattleRoom._uniqueRoomId++;
            p._battleRoomId = roomId;
            BattleRoom br = new BattleRoom(this, roomId, p, m, false);
            _battleRooms.Add(roomId, br);
        }
        public void MakeBattleRoom(int p1Id,int p2Id)
        {
            ServerSession s1;
            ServerSession s2;
            if (_sessions.TryGetValue(p1Id, out s1) == false)
                return;
            if (_sessions.TryGetValue(p2Id, out s2) == false)
                return;
            Player p1 = s1._player;
            Player p2 = s2._player;

            if (p1._mainMon._cp.Hp == 0 || p2._mainMon._cp.Hp == 0)
                return;
            p1._state = p2._state = ObjectState.Fight;
            p1._canCollision = p2._canCollision = false;

            { // to p1
                S_Battle packet = new S_Battle();
                packet.EnemyCp = p2._mainMon._cp;
                p1._session.Send(packet);
            }

            { // to p2
                S_Battle packet = new S_Battle();
                packet.EnemyCp = p1._mainMon._cp;
                p2._session.Send(packet);
            }

            int roomId = BattleRoom._uniqueRoomId++;
            p1._battleRoomId = roomId;
            p2._battleRoomId = roomId;
            BattleRoom br = new BattleRoom(this, roomId, p1, p2);
            _battleRooms.Add(roomId, br);
        }
        public void LeaveBattleRoom(GameObject go)
        {
            Player p = go as Player;
            int battleRoomId = p._battleRoomId;
            BattleRoom br;
            if (_battleRooms.TryGetValue(p._battleRoomId, out br) == false)
                return;
            S_LeftOpponent packet = new S_LeftOpponent();
            if(br._p1 != null && br._p1._first != p)
            {
                br._p1._first._session.Send(packet);
            }
            if (br._p2 != null && br._p2._first != p)
            {
                br._p2._first._session.Send(packet);
            }
            OnBattleCompleted(battleRoomId);
        }
        public void OnBattle(int battleRoomId)
        {
            BattleRoom br = null;
            if (_battleRooms.TryGetValue(battleRoomId, out br) == false)
                return;
            br._remainCnt--;
            if (br._remainCnt == 0)
            {
                br._executeTick = 0;
            }
        }
        public void ResetPlayer(int objId)
        {
            GameObject go = null;
            if (_objects.TryGetValue(objId, out go) == false)
                return;
            go._canCollision = true;
            go._state = ObjectState.Idle;
        }

        public void OnBattleCompleted(int battleRoomId)
        {
            BattleRoom br = _battleRooms[battleRoomId];
            Monster m = br._m;
            if (m != null)
            {
                m._canCollision = true;
                m._state = ObjectState.Moving;
                if (m._cp.Hp == 0)
                {
                    m._cp.Hp = m._cp.MaxHp;
                    Push(Respawn, m);
                }
            }
            _battleRooms.Remove(battleRoomId);
        }

        

        class BattleRoom
        {
            public static int _uniqueRoomId = 0;
            public GameRoom _ownerGameRoom;
            public int _roomId;
            public long _executeTick = 0;
            public int _turnTime;
            public Pair<Player, ObjectCP> _p1 = null;
            public Pair<Player, ObjectCP> _p2 = null;
            public Monster _m = null;
            public int _remainCnt = 0;
            bool _pvp;
            bool _captured = false;
            public BattleRoom(GameRoom ownerGameRoom, int roomId, GameObject go1, GameObject go2, bool pvp = true)
            {
                _ownerGameRoom = ownerGameRoom;
                _pvp = pvp;
                _roomId = roomId;
                if (pvp)
                {
                    _turnTime = 15;
                    Player p1 = go1 as Player;
                    Player p2 = go2 as Player;
                    _p1 = new Pair<Player, ObjectCP>(p1, p1._mainMon._cp);
                    _p2 = new Pair<Player, ObjectCP>(p2, p2._mainMon._cp);
                    p1._attackType = AttackType.Nothing;
                    p2._attackType = AttackType.Nothing;
                }
                else
                {
                    _turnTime = 30;
                    Player p = go1 as Player;
                    _p1 = new Pair<Player, ObjectCP>(p, p._mainMon._cp);
                    Monster m = go2 as Monster;
                    _m = m;
                    p._attackType = AttackType.Nothing;
                    m._attackType = AttackType.Nothing;
                }
            }

            public void Update()
            {
                if (_executeTick > Environment.TickCount64)
                    return;
                _executeTick = Environment.TickCount64 + (long)_turnTime * 1000;

                if (_pvp)
                {
                    switch (_p1._first._attackType)
                    {
                        case AttackType.Nothing:
                            {
                                break;
                            }
                        case AttackType.Normalattack:
                            {
                                _p2._second.Hp = Math.Max(_p2._second.Hp -= _p1._second.Damage, 0);
                                break;
                            }
                        case AttackType.Skillattack:
                            {
                                _p2._second.Hp = Math.Max(_p2._second.Hp -= (int)(_p1._second.Damage * 1.5f), 0);
                                break;
                            }
                        case AttackType.Capture:
                            {
                                break;
                            }
                        default: break;
                    }
                    ////////////////////////
                    switch (_p2._first._attackType)
                    {
                        case AttackType.Nothing:
                            {
                                break;
                            }
                        case AttackType.Normalattack:
                            {
                                _p1._second.Hp = Math.Max(_p1._second.Hp -= _p2._second.Damage, 0);
                                break;
                            }
                        case AttackType.Skillattack:
                            {
                                _p1._second.Hp = Math.Max(_p1._second.Hp -= (int)(_p2._second.Damage * 1.5f), 0);
                                break;
                            }
                        case AttackType.Capture:
                            {
                                break;
                            }
                        default: break;
                    }
                    OnTurnCompleted();
                    _remainCnt = 2;
                }
                else
                {
                    switch (_p1._first._attackType)
                    {
                        case AttackType.Nothing:
                            {
                                break;
                            }
                        case AttackType.Normalattack:
                            {
                                _m._cp.Hp = Math.Max(_m._cp.Hp -= _p1._second.Damage, 0);
                                break;
                            }
                        case AttackType.Skillattack:
                            {
                                _m._cp.Hp = Math.Max(_m._cp.Hp -= (int)(_p1._second.Damage * 1.5f), 0);
                                break;
                            }
                        case AttackType.Capture:
                            {
                                if (_p1._first._currentMonCnt == Player._maxMonCnt)
                                    break;
                                Random rand = new Random();
                                int randNum = rand.Next(0, 101);
                                int probability = (int)(((_m._cp.MaxHp - _m._cp.Hp) / (float)_m._cp.MaxHp) * 100);
                                if (randNum <= probability) // capture success
                                {
                                    _captured = true;
                                }
                                break;
                            }
                        default: break;
                    }
                    if (_m._cp.Hp > 0)
                    {
                        if (_m._attackType == AttackType.Normalattack)
                        {
                            _p1._second.Hp = Math.Max(_p1._second.Hp -= _m._cp.Damage, 0);
                        }
                        else
                        {
                            _m._attackType = AttackType.Normalattack;
                        }
                    }
                    OnTurnCompleted();
                    _remainCnt = 1;
                }
            }
            public void OnTurnCompleted()
            {
                bool finish = true;
                if (_pvp)
                {
                    if (_p1._second.Hp == 0 && _p2._second.Hp == 0)
                    {
                        S_AttackResult packet = new S_AttackResult();
                        packet.Result = 3;

                        _p1._first._session.Send(packet);

                        _p2._first._session.Send(packet);
                    }
                    else if (_p1._second.Hp == 0)
                    {
                        S_AttackResult packetToP1 = new S_AttackResult();
                        packetToP1.Result = 2;
                        packetToP1.P = _p1._second;
                        packetToP1.Enemy = _p2._second;
                        _p1._first._session.Send(packetToP1);

                        S_AttackResult packetToP2 = new S_AttackResult();
                        packetToP2.Result = 1;
                        packetToP2.P = _p2._second;
                        packetToP2.Enemy = _p1._second;
                        _p2._first._session.Send(packetToP2);
                    }
                    else if (_p2._second.Hp == 0)
                    {
                        S_AttackResult packetToP1 = new S_AttackResult();
                        packetToP1.Result = 1;
                        packetToP1.P = _p1._second;
                        packetToP1.Enemy = _p2._second;
                        _p1._first._session.Send(packetToP1);

                        S_AttackResult packetToP2 = new S_AttackResult();
                        packetToP2.Result = 2;
                        packetToP2.P = _p2._second;
                        packetToP2.Enemy = _p1._second;
                        _p2._first._session.Send(packetToP2);
                    }
                    else
                    {
                        S_AttackResult packetToP1 = new S_AttackResult();
                        packetToP1.Result = 0;
                        packetToP1.P = _p1._second;
                        packetToP1.Enemy = _p2._second;
                        packetToP1.TurnTime = _turnTime;
                        _p1._first._session.Send(packetToP1);

                        S_AttackResult packetToP2 = new S_AttackResult();
                        packetToP2.Result = 0;
                        packetToP2.P = _p2._second;
                        packetToP2.Enemy = _p1._second;
                        packetToP2.TurnTime = _turnTime;
                        _p2._first._session.Send(packetToP2);

                        finish = false;
                    }
                }
                else // !pvp
                {
                    if (_captured)
                    {
                        S_AttackResult packet = new S_AttackResult();
                        packet.Result = 4;
                        _p1._first._session.Send(packet);

                        MyMonster mon = new MyMonster(0);
                        mon._cp = _m._cp;
                        mon._cp.Hp = mon._cp.MaxHp;
                        _p1._first.AddMon(mon);

                        _p1._first.SendMonList();
                    }
                    else if (_p1._second.Hp == 0)
                    {
                        S_AttackResult packet = new S_AttackResult();
                        packet.Result = 2;
                        packet.P = _p1._second;
                        packet.Enemy = _m._cp;
                        _p1._first._session.Send(packet);
                    }
                    else if (_m._cp.Hp == 0)
                    {
                        _p1._first.GetExp(_m._cp.RewardExp);
                        S_AttackResult packet = new S_AttackResult();
                        packet.Result = 1;
                        packet.P = _p1._second;
                        _p1._first._session.Send(packet);
                    }
                    else
                    {
                        S_AttackResult packet = new S_AttackResult();
                        packet.Result = 0;
                        packet.P = _p1._second;
                        packet.Enemy = _m._cp;
                        packet.TurnTime = _turnTime;
                        _p1._first._session.Send(packet);

                        finish = false;
                    }
                }
                if (_p1 != null)
                    _p1._first._attackType = AttackType.Nothing;
                if (_p2 != null)
                    _p2._first._attackType = AttackType.Nothing;

                if (finish)
                {
                    _ownerGameRoom.OnBattleCompleted(_roomId);
                }
            }
        }
        public void RequestBattle(int senderId, int receiverId)
        {
            S_NotifyBattleRequest packet = new S_NotifyBattleRequest();
            packet.Opponent = senderId;
            ServerSession senderSession;
            if (_sessions.TryGetValue(senderId, out senderSession) == false)
                return;

            ServerSession receiverSession;
            if (_sessions.TryGetValue(receiverId, out receiverSession) == false)
                return;
            receiverSession.Send(packet);
        }
        #endregion
        public void AlterMainMon(int objId, int MainNum)
        {
            GameObject go = null;
            if (_objects.TryGetValue(objId, out go) == false)
                return;
            Player p = go as Player;
            if (p == null)
                return;
            p.AlterMainMon(MainNum);
        }
        public void UsePotion(ServerSession s)
        {
            Player p = s._player;
            if (p == null)
                return;
            if (p._hpPotionCnt <= 0)
                return;
            if (p._mainMon == null)
                return;
            p._hpPotionCnt--;
            p._mainMon.UsePotion();
            S_UsePotion packet = new S_UsePotion();
            s.Send(packet);
        }
    }
}
