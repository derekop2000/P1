using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Utils
{
    public class Pair<T1,T2>
    {
        public T1 _first { get; set; }
        public T2 _second { get; set; }
        public Pair(T1 first, T2 second)
        {
            _first = first;
            _second = second;
        }
    }
}
