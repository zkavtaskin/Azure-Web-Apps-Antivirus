using System;
using System.IO;

namespace AVFilterIIS
{
    public class ReadWriteProxyStream : Stream
    {
        private readonly Stream writeOnlyStream;
        private readonly MemoryStream readWriteInterceptStream;

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return this.readWriteInterceptStream.Length; }
        }

        public override long Position
        {
            get
            {
                return this.readWriteInterceptStream.Position;
            }
            set
            {
                this.readWriteInterceptStream.Position = value;
            }
        }

        public ReadWriteProxyStream(Stream writeOnlyStream)
        {
            if (!writeOnlyStream.CanWrite)
                throw new Exception("Can't write to the write only stream");

            if (writeOnlyStream.CanRead)
                throw new Exception("You can already read this stream, you don't need to use this proxy class");

            this.writeOnlyStream = writeOnlyStream;
            this.readWriteInterceptStream = new MemoryStream();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.readWriteInterceptStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.writeOnlyStream.Write(buffer, offset, count);
            this.readWriteInterceptStream.Write(buffer, offset, count);
        }

        public override void Flush()
        {
            this.writeOnlyStream.Flush();
            this.readWriteInterceptStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
    }
}
