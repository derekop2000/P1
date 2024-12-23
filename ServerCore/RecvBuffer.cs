using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class RecvBuffer
    {
        public ArraySegment<byte> _buf;
        int _readPos = 0;
        int _writePos = 0;
        int _dataSize { get { return _writePos - _readPos; } }
        int _freeSize { get { return _buf.Count - _writePos; } }
        public RecvBuffer(int bufSize)
        {
            _buf = new ArraySegment<byte>(new byte[bufSize],0,bufSize);    
        }
        public ArraySegment<byte> _dataSegment { get { return new ArraySegment<byte>(_buf.Array, _buf.Offset+_readPos, _dataSize); } }
        public ArraySegment<byte> _writeSegment { get { return new ArraySegment<byte>(_buf.Array, _buf.Offset+_writePos, _freeSize); } }
        public void Clean()
        {
            int dataSize = _dataSize;
            if(dataSize == 0)
            {
                _readPos = _writePos = 0;
            }
            else
            {
                Array.Copy(_buf.Array, _buf.Offset + _readPos, _buf.Array, _buf.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }
        public bool OnRead(int numOfBytes)
        {
            if (_dataSize < numOfBytes)
                return false;
            _readPos += numOfBytes;
            return true;
        }
        public bool OnWrite(int numOfBytes)
        {
            if(numOfBytes > _freeSize)
                return false;
            _writePos += numOfBytes;
            return true;
        }
    }
}
