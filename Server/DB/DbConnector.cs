using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DB
{
    public class DbConnector
    {
        public OdbcConnection _connection;
        public OdbcCommand _command;
        public DbConnector()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(ConfigurationManager.ConnectionStrings["DBconnect"].ConnectionString);
            string op = sb.ToString();
            _connection = new OdbcConnection(op);
            _connection.Open();

            _command = _connection.CreateCommand();
        }
        public void Dispose()
        {
            if(_command != null)
                _command.Dispose();
            if (_connection != null)
                _connection.Dispose();
        }
    }
}
