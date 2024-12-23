using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Protobuf;

using Server.Contents.Room;

namespace Server.Contents.Object
{
    public class GameObject
    {
        public GameObject()
        {
            _objId = ObjectManager.Instance.GenObjId();
            _type = ObjectType.Normal;
        }
        public ObjectInfo _objectInfo = new ObjectInfo();
        public Pos _pos
        {
            get
            {
                if(_objectInfo.ObjectPos == null)
                    _objectInfo.ObjectPos = new Pos();
                return _objectInfo.ObjectPos;
            }
            set
            {
                if (_objectInfo.ObjectPos == null)
                    _objectInfo.ObjectPos = new Pos();
                _objectInfo.ObjectPos.X = value.X;
                _objectInfo.ObjectPos.Y = value.Y;
            }
        }
        public ObjectState _state { get { return _objectInfo.ObjectState; }set { _objectInfo.ObjectState = value; } }
        public int _objId { get { return _objectInfo.ObjectId; }set { _objectInfo.ObjectId = value; } }
        public ObjectType _type { get { return _objectInfo.ObjectType; }set { _objectInfo.ObjectType = value; } }
        public string _name;
        public GameRoom _room;
        public bool _canCollision = true;
       
        public virtual void OnCollision(GameObject collisionGo)
        {

        }
    }
    ////////////////////////////////////
    public class Portal : GameObject
    {
        private int _destRoomNum;
        public Portal(int destRoomNum)
        {
            _type = ObjectType.Portal;
            _name = "Portal";
            _destRoomNum = destRoomNum;
        }
        public override void OnCollision(GameObject collisionGo)
        {
            GameRoom preRoom = collisionGo._room;
            GameRoom nextRoom = RoomManager.Instance.GetRoom(_destRoomNum);
            preRoom.Leave(collisionGo);
            nextRoom.Enter(collisionGo);
        }
    }

}
