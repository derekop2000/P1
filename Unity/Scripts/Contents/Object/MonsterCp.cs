using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Assets.Scripts.Contents.GameData;

using Google.Protobuf;

namespace Assets.Scripts.Contents.Object
{
    public class MonsterCp
    {
        public ObjectCP _cp;
        public int _objId;
        public void UsePotion()
        {
            _cp.Hp = Math.Min(_cp.Hp + GameDatList.HpPotion, _cp.MaxHp);
        }
    }

}
