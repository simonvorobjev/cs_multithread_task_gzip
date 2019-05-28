using System;
using System.Collections.Generic;
namespace Compressor
{
    class CompressBuffer
    {
        public byte[] buffer;
        public int actualLength;
        public int threadNumber;
        public int blockNumber;
        public CompressBuffer(byte[] buf, int len, int tnum, int blkNum)
        {
            buffer = buf;
            actualLength = len;
            threadNumber = tnum;
            blockNumber = blkNum;
        }
    }
}
