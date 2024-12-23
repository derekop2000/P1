using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DB
{
    public class DbPool
    {
        public static DbPool Instance { get { return _dbPool; } }
        private static DbPool _dbPool = new DbPool();
        object _lock = new object();
        Queue<DbConnector> _q = new Queue<DbConnector>();
        public void Push(DbConnector con)
        {
            lock (_lock)
            {
                _q.Enqueue(con);
            }
        }
        public DbConnector Pop()
        {
            lock (_lock)
            {
                if(_q.Count == 0)
                    _q.Enqueue(new DbConnector());
                return _q.Dequeue();
            }
        }
        public void Dispose()
        {
            lock (_lock)
            {
                while(_q.Count > 0)
                {
                    DbConnector current = _q.Dequeue();
                    current.Dispose();
                }
            }
        }
    }
}
