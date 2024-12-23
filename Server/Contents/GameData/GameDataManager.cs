using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Google.Protobuf;

namespace Server.Contents.GameData
{
    public class GameDataManager
    {
        public static string path = "../../../Utils/GameData.xml";
        public static ObjectCP M1 { get; set; }
        public static ObjectCP M2 { get; set; }
        public static ObjectCP M3 { get; set; }
        public static int HpPotion { get; set; }
        public static void Init()
        {
            XDocument doc = XDocument.Load(path);

            var objCP = doc.Element("GameData").Element("ObjectCp");

            M1 = SetObjectCP(objCP.Element("M1"));
            M2 = SetObjectCP(objCP.Element("M2"));
            M3 = SetObjectCP(objCP.Element("M3"));

            HpPotion = int.Parse(doc.Element("GameData").Element("Item").Element("HpPotion").Value);
        }

        private static ObjectCP SetObjectCP(XElement element)
        {
            return new ObjectCP
            {
                MonNum = int.Parse(element.Element("MonNum").Value),
                MaxHp = int.Parse(element.Element("MaxHp").Value),
                Hp = int.Parse(element.Element("Hp").Value),
                HpIncrease = int.Parse(element.Element("HpIncrease").Value),
                Damage = int.Parse(element.Element("Damage").Value),
                DamageIncrease = int.Parse(element.Element("DamageIncrease").Value),
                Level = int.Parse(element.Element("Level").Value),
                Exp = int.Parse(element.Element("Exp").Value),
                MaxExp = int.Parse(element.Element("MaxExp").Value),
                RewardExp = int.Parse(element.Element("RewardExp").Value)
            };
        }
    }
}
