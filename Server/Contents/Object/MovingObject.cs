using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
namespace Server.Contents.Object
{
    public class MovingObject : GameObject
    {
        protected float _speed;
        public AttackType _attackType = AttackType.Nothing; 
        
        public MovingObject()
        {
            _pos.Dir = ObjectDir.Down;
        }
    }
}
