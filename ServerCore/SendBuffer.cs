using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class SendBufferHelper
    {
        public static ThreadLocal<SendBuffer> _currentBuf = new ThreadLocal<SendBuffer>();
        public static int chunkSize = 4096 * 10000;
        public static ArraySegment<byte> Open(int reserveSize)
        {
            if(_currentBuf.Value == null)
                _currentBuf.Value = new SendBuffer(chunkSize);
            if(reserveSize > _currentBuf.Value._freeSize)
                _currentBuf.Value = new SendBuffer(chunkSize);
            return _currentBuf.Value.Open(reserveSize);
        }
        public static ArraySegment<byte> Close(int usedSize)
        {
            return _currentBuf.Value.Close(usedSize);
        }
    }

    public class SendBuffer
    {
        public byte[] _buf;
        public int _usedSize = 0;
        public int _freeSize { get { return _buf.Length - _usedSize; } }
        public SendBuffer(int size)
        {
            _buf = new byte[size];
        }
        public ArraySegment<byte> Open(int reserveSize)
        {
            if (reserveSize > _freeSize)
                return null;
            return new ArraySegment<byte>(_buf,_usedSize, reserveSize);
        }
        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buf,_usedSize, usedSize);
            _usedSize += usedSize;
            return segment;
        }
    }
}
