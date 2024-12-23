using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Protobuf;

using Server.Contents.GameData;

namespace Server.Contents.Object
{
    public class MyMonster
    {
        public int _objId;
        
        public MyMonster(int monNum)
        {
            _objId = ObjectManager.Instance.GenObjId();
            switch (monNum)
            {
                case 0:
                    break;
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
        public ObjectCP _cp = new ObjectCP();
        public void UsePotion()
        {
            _cp.Hp = Math.Min(_cp.Hp + GameDataManager.HpPotion, _cp.MaxHp); 
        }
    }
}
