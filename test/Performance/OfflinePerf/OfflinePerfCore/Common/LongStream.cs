using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfflinePerfCore.Common
{
    public abstract class LongStream : Stream
    {
        private byte[] buffer;
        private int bufferIndex;
        private int bufferLength;
        private State state;

        public LongStream()
        {
            this.buffer = new byte[1024];
            this.bufferIndex = 0;
            this.bufferLength = 0;
            this.state = State.ReadingHeader;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesInBuffer = this.bufferLength - this.bufferIndex;
            if (bytesInBuffer == 0)
            {
                while (this.state != State.End && this.bufferLength == this.bufferIndex)
                {
                    // Load from dynamic stream
                    switch (this.state)
                    {
                        case State.ReadingHeader:
                            var header = this.GetHeader();
                            this.LoadIntoBuffer(header);
                            this.state = State.ReadingBody;
                            break;
                        case State.ReadingBody:
                            var bodyPart = this.GetRecurringPart();
                            if (string.IsNullOrEmpty(bodyPart))
                            {
                                this.state = State.ReadingEnding;
                            }
                            else
                            {
                                this.LoadIntoBuffer(bodyPart);
                            }

                            break;
                        case State.ReadingEnding:
                            var ending = this.GetEnding();
                            this.LoadIntoBuffer(ending);
                            this.state = State.End;
                            break;
                        case State.End:
                        default:
                            break;
                    }
                }

                bytesInBuffer = this.bufferLength - this.bufferIndex;
            }

            var toReturn = Math.Min(count, bytesInBuffer);
            Buffer.BlockCopy(this.buffer, this.bufferIndex, buffer, offset, toReturn);
            this.bufferIndex += toReturn;
            return toReturn;
        }

        private void LoadIntoBuffer(string text)
        {
            int size = Encoding.UTF8.GetByteCount(text);
            if (size > this.buffer.Length)
            {
                this.buffer = new byte[size];
            }

            this.bufferIndex = 0;
            this.bufferLength = Encoding.UTF8.GetBytes(text, 0, text.Length, this.buffer, 0);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns the contents of the beginning of the stream.
        /// </summary>
        /// <returns>The contents to be returned when the stream starts to be read.</returns>
        protected abstract string GetHeader();

        /// <summary>
        /// Returns the contents of the repeating part of the stream.
        /// </summary>
        /// <returns>The ongoing content to be returned by the stream.</returns>
        /// <remarks>This function will be called multiple times, until it returns <code>null</code>
        /// or an empty string.</remarks>
        protected abstract string GetRecurringPart();

        /// <summary>
        /// Returns the contents of the end of the stream. It will be called after the last
        /// call to <see cref="GetRecurringPart"/>.
        /// </summary>
        /// <returns>The content to be returned at the end of the stream.</returns>
        protected abstract string GetEnding();

        enum State { ReadingHeader, ReadingBody, ReadingEnding, End }
    }
}
