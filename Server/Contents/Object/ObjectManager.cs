using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Contents.Object
{
    public class ObjectManager
    {
        public static ObjectManager Instance = new ObjectManager();
        
        object _lock = new object();
        static int _objNum = 0;
        public int GenObjId()
        {
            int id;
            lock (_lock)
            {
                id = _objNum++;
            }
            return id;
        }
    }
}
