using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Connecter
    {
        Func<Session> _getSession;
        public void Connect(IPEndPoint point,Func<Session> getSession, int cnt = 1)
        {
            _getSession = getSession;
            for(int i=0;i<cnt; i++)
            {
                Socket sock = new Socket(point.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                e.Completed += OnConnectCompleted;
                e.RemoteEndPoint = point;
                e.UserToken = sock;
                RegisterConnect(e);
            }
            
        }
        void RegisterConnect(SocketAsyncEventArgs e)
        {
            Socket sock = e.UserToken as Socket;
            if (sock == null)
                return;
            bool pending = sock.ConnectAsync(e);
            if (pending == false)
                OnConnectCompleted(null, e);
        }
        void OnConnectCompleted(object obj,SocketAsyncEventArgs e)
        {
            if(e.SocketError == SocketError.Success)
            {
                Session s = _getSession();
                s.Init(e.ConnectSocket);
                s.OnConnected();
            }
            else
            {

            }
        }
    }
}
