using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Listener
    {
        public SocketAsyncEventArgs _e = null;
        Func<Session> _getSession;
        public Socket _listenSock = null;
        public void Init(IPEndPoint endPoint, Func<Session> getSession)
        {
            _getSession= getSession;

            Socket listenSock = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSock.Bind(endPoint);
            listenSock.Listen(100);

            _listenSock = listenSock;

            _e = new SocketAsyncEventArgs();
            _e.Completed += OnAcceptCompleted;

            RegisterAccept();
        }
        public void RegisterAccept()
        {
            _e.AcceptSocket = null;
            bool pending = _listenSock.AcceptAsync(_e);
            if(pending==false)
            {
                OnAcceptCompleted(null, _e);
            }
        }
        public void OnAcceptCompleted(object obj, SocketAsyncEventArgs e)
        {
            if(_e.SocketError == SocketError.Success)
            {
                Session s = _getSession();
                s.Init(_e.AcceptSocket);
                s.OnConnected();
            }
            RegisterAccept();
        }
    }
}
