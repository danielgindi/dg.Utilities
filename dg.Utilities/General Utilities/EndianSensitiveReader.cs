using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace dg.Utilities
{
    public class EndianSensitiveReader : IDisposable
    {
        private Stream _Stream;
        private bool _LittleEndian = true;
        private bool _LeaveOpen = false;
        private byte[] _Buffer = new byte[4];
        static private byte[] _StretchingBuffer;

        public EndianSensitiveReader(Stream input)
            : this(input, false)
        {
        }

        public EndianSensitiveReader(Stream input, bool leaveOpen)
        {
            _Stream = input;
            _LeaveOpen = leaveOpen;
        }

        public virtual void Close()
        {
            this.Dispose(true);
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stream mStream = _Stream;
                _Stream = null;
                if (mStream != null && !_LeaveOpen)
                {
                    mStream.Close();
                }
            }
            _Stream = null;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        #endregion

        #region Properties

        public bool LittleEndian
        {
            get { return _LittleEndian; }
            set { _LittleEndian = value; }
        }

        public virtual Stream BaseStream
        {
            get { return this._Stream; }
        }

        #endregion

        #region Readers

        public virtual byte[] ReadBytes(int length)
        {
            if (length > 64)
            {
                byte[] bytes = new byte[length];
                if (_Stream.Read(bytes, 0, length) == length)
                {
                    return bytes;
                }
            }
            else
            {
                if (_StretchingBuffer == null || _StretchingBuffer.Length < length)
                {
                    _StretchingBuffer = new byte[length];
                }
                if (_Stream.Read(_StretchingBuffer, 0, length) == length)
                {
                    return _StretchingBuffer;
                }
            }
            return null;
        }

        public virtual byte ReadByte()
        {
            int value = _Stream.ReadByte();
            return value == -1 ? (byte)0 : (byte)value;
        }

        public virtual sbyte ReadSByte()
        {
            int value = _Stream.ReadByte();
            return value == -1 ? (sbyte)0 : (sbyte)value;
        }

        public virtual Int16 ReadInt16()
        {
            _Stream.Read(_Buffer, 0, 2);
            if (_LittleEndian)
            {
                return (short)(_Buffer[0] | _Buffer[1] << 8);
            }
            else
            {
                return (short)(_Buffer[1] | _Buffer[0] << 8);
            }
        }

        public virtual UInt16 ReadUInt16()
        {
            _Stream.Read(_Buffer, 0, 2);
            if (_LittleEndian)
            {
                return (UInt16)(_Buffer[0] | _Buffer[1] << 8);
            }
            else
            {
                return (UInt16)(_Buffer[1] | _Buffer[0] << 8);
            }
        }

        public virtual Int32 ReadInt32()
        {
            _Stream.Read(_Buffer, 0, 4);
            if (_LittleEndian)
            {
                return (Int32)(_Buffer[0] | _Buffer[1] << 8 | _Buffer[2] << 16 | _Buffer[3] << 24);
            }
            else
            {
                return (Int32)(_Buffer[3] | _Buffer[2] << 8 | _Buffer[1] << 16 | _Buffer[0] << 24);
            }
        }

        public virtual UInt32 ReadUInt32()
        {
            _Stream.Read(_Buffer, 0, 4);
            if (_LittleEndian)
            {
                return (UInt32)(_Buffer[0] | _Buffer[1] << 8 | _Buffer[2] << 16 | _Buffer[3] << 24);
            }
            else
            {
                return (UInt32)(_Buffer[3] | _Buffer[2] << 8 | _Buffer[1] << 16 | _Buffer[0] << 24);
            }
        }

        #endregion
    }
}
