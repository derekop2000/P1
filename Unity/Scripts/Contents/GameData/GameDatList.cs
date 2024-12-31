using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Protobuf;

namespace Assets.Scripts.Contents.GameData
{
    public class GameDatList
    {
        public static ObjectCP M1 = new ObjectCP()
        {
            MonNum = 1,
            MaxHp = 50,
            Hp = 50,
            HpIncrease = 5,
            Damage = 15,
            DamageIncrease = 3,
            Level = 1,
            Exp = 0,
            MaxExp = 100,
            RewardExp = 20,
        };
        public static ObjectCP M2 = new ObjectCP()
        {
            MonNum = 2,
            MaxHp = 70,
            Hp = 70,
            HpIncrease = 10,
            Damage = 15,
            DamageIncrease = 5,
            Level = 1,
            Exp = 0,
            MaxExp = 100,
            RewardExp = 30,
        };
        public static ObjectCP M3 = new ObjectCP()
        {
            MonNum = 3,
            MaxHp = 90,
            Hp = 90,
            HpIncrease = 10,
            Damage = 35,
            DamageIncrease = 7,
            Level = 1,
            Exp = 0,
            MaxExp = 100,
            RewardExp = 45,
        };
        public static int HpPotion = 50;
    }
}
