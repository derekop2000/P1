using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static int _headerSize = 2;
        public override void OnConnected() { }

        public override void OnDisconnected(EndPoint point) { }

        public override int OnRecv(ArraySegment<byte> buf)
        {
            int processLen = 0;
            while (true)
            {
                if (buf.Count - processLen < _headerSize)
                    break;
                ushort pktSize = BitConverter.ToUInt16(buf.Array, buf.Offset + processLen);
                if (buf.Count - processLen < pktSize)
                    break;
                OnPacket(new ArraySegment<byte>(buf.Array, buf.Offset + processLen, pktSize));
                processLen += pktSize;
            }
            return processLen;
        }

        public override void OnSend() { }
        public abstract void OnPacket(ArraySegment<byte> segment);
    }

    abstract public class Session
    {
        public Socket _sock;
        public SocketAsyncEventArgs _recvEvent;
        public SocketAsyncEventArgs _sendEvent;
        public RecvBuffer _recvBuffer;
        public int _disconnected = 0;
        Queue<ArraySegment<byte>> _sendQueue;
        List<ArraySegment<byte>> _pendingList;
        bool _isPending = false;
        object _lock = new object();

        public abstract void OnConnected();
        public abstract void OnDisconnected(EndPoint point);
        public abstract int OnRecv(ArraySegment<byte> buf);
        public abstract void OnSend();
        public void Init(Socket sock)
        {
            _sock = sock;

            _recvBuffer = new RecvBuffer(4096);

            _recvEvent = new SocketAsyncEventArgs();
            _sendEvent = new SocketAsyncEventArgs();

            _recvEvent.Completed += OnRecvCompleted;
            _sendEvent.Completed += OnSendCompleted;

            _sendQueue = new Queue<ArraySegment<byte>>();
            _pendingList = new List<ArraySegment<byte>>();

            RegisterRecv();
        }
        public void Send(ArraySegment<byte> segment)
        {
            bool pending = false;
            lock(_lock)
            {
                _sendQueue.Enqueue(segment);
                if(_isPending==false)
                {
                    _isPending = pending = true;
                }
            }
            if(pending)
                RegisterSend();
        }
        void RegisterRecv()
        {
            _recvBuffer.Clean();
            _recvEvent.SetBuffer(_recvBuffer._writeSegment);
            bool pending = _sock.ReceiveAsync(_recvEvent);
            if(pending == false)
            {
                OnRecvCompleted(null,_recvEvent);
            }
        }
        void OnRecvCompleted(object obj,SocketAsyncEventArgs e)
        {
            if(e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                if (_recvBuffer.OnWrite(_recvEvent.BytesTransferred) == false)
                {
                    Disconnect();
                    return;
                }

                int readBytes = OnRecv(_recvBuffer._dataSegment);

                if (_recvBuffer.OnRead(readBytes) == false)
                {
                    Disconnect();
                    return;
                }
                RegisterRecv();
            }
            else
            {
                Disconnect();
            }
        }
        void RegisterSend()
        {
            lock(_lock)
            {
                while(_sendQueue.Count > 0)
                {
                    _pendingList.Add(_sendQueue.Dequeue());
                }
            }
            _sendEvent.BufferList = _pendingList;
            try
            {
                bool pending = _sock.SendAsync(_sendEvent);
                if(pending == false)
                {
                    OnSendCompleted(null, _sendEvent);
                }
            }
            catch (SocketException e)
            {
                ;
            }
        }
        void OnSendCompleted(object obj, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {

                _sendEvent.BufferList.Clear();
                _pendingList.Clear();

                OnSend();

                lock(_lock)
                {
                    if (_sendQueue.Count > 0)
                        RegisterSend();
                    else
                        _isPending = false;
                }
            }
            else
            {
                Disconnect();
            }
        }
        void Disconnect()
        {
            if(Interlocked.Exchange(ref _disconnected,1)==0)
            {
                try
                {
                    OnDisconnected(_sock.RemoteEndPoint);
                    _sock.Shutdown(SocketShutdown.Both);
                    _sock.Close();
                }
                catch(SocketException e)
                {

                }
            }
        }
    }
}
