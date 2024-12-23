using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Protobuf;

using Server.Contents.GameData;
using Server.Contents.Room;

namespace Server.Contents.Object
{
    public class Monster : MovingObject
    {
        public ObjectCP _cp = new ObjectCP();
        long _moveTick = 0;
        
        public Monster(int monNum)
        {
            _state = ObjectState.Moving;
            _objectInfo.MonNum = monNum;
            _cp.MonNum = monNum;
            _type = ObjectType.Monster;
            _speed = 3f;

            switch (monNum)
            {
                case 1:
                    {
                        _cp = GameData.GameDataManager.M1;   
                        break;
                    }
                case 2:
                    {
                        _cp = GameData.GameDataManager.M2;
                        break;
                    }
                case 3:
                    {
                        _cp = GameData.GameDataManager.M3;
                        break;
                    }
                default:
                    break;
            }
        }
        public void Update()
        {
            switch (_state)
            {
                case ObjectState.Moving:
                    {
                        if (_moveTick > Environment.TickCount64)
                            return;
                        _moveTick = Environment.TickCount64 + (long)(1000 / _speed);
                        Random rand = new Random();
                        int randValue = rand.Next(0, 5);
                        Pos destPos = new Pos(_pos);
                        ObjectDir dir = ObjectDir.Down;
                        switch (randValue)
                        {
                            case 0:
                                {
                                    break;
                                }
                            case 1:
                                {
                                    dir = ObjectDir.Up;
                                    destPos.Y++;
                                    break;
                                }
                            case 2:
                                {
                                    dir = ObjectDir.Down;
                                    destPos.Y--;
                                    break;
                                }
                            case 3:
                                {
                                    dir = ObjectDir.Right;
                                    destPos.X++;
                                    break;
                                }
                            case 4:
                                {
                                    dir = ObjectDir.Left;
                                    destPos.X--;
                                    break;
                                }
                            default: break;
                        }
                        Map map = _room._map;
                        int moveResult = map.CanGo(this, destPos);
                        if (moveResult == 0)
                        {
                            ;
                        }
                        else if (moveResult == 1)
                        {
                            destPos.Dir = dir;
                            _room._map.ClearObjPos(_pos);
                            _pos = destPos;
                            _room._map.SetObjPos(this);
                            S_Move packet = new S_Move();
                            packet.ObjId = _objId;
                            packet.Pos = destPos;
                            _room.BroadCast(packet);
                            _state = ObjectState.Moving;
                        }
                        else if (moveResult == 2)
                        {
                            //GameObject destObj = map.GetObj(destPos);
                            //if (destObj != null && destObj._canCollision)
                            //{
                            //    OnCollision(destObj);
                            //}
                        }
                        else {; }
                        break;
                    }
                case ObjectState.Fight:
                    {
                        break;
                    }
                
            }
        }
        public override void OnCollision(GameObject collisionGo)
        {
            Player p = collisionGo as Player;
            if (p == null)
                return;
            if (p._mainMon._cp.Hp ==0)
                return;
            _canCollision = false;
            collisionGo._canCollision = false;
            _state = ObjectState.Fight;
            collisionGo._state = ObjectState.Fight;

            _room.Push(_room.MakeBattleRoom, p, this);
        }
    }
}
