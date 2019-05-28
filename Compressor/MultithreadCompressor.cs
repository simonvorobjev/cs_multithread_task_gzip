using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Compressor
{
    class MultithreadCompressor : BaseCompressor
    {
        private int _chunkSize;

        public MultithreadCompressor(string inputFile, string outputFile, int chunkSize) : base(inputFile, outputFile)
        {
            _chunkSize = chunkSize;
        }

        public override void Calculate()
        {
            _calculationInProgress = true;
            _writer = new Thread(new ThreadStart(WriteToFile));
            _writer.Start();
            using (FileStream _inputFileStream = new FileStream(_inputFile, FileMode.Open))
            {
                int dataRead;
                byte[] buffer = new byte[_chunkSize];
                int currentThread = 0;
                int currentBlock = 0;
                while ((dataRead = _inputFileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (_cancelled)
                    {
                        break;
                    }
                    currentBlock++;
                    while (true)
                    {
                        if (_cancelled)
                        {
                            break;
                        }
                        if (_threadReady[currentThread])
                        {
                            _threadReady[currentThread] = false;
                            if ((_threads[currentThread] != null) && _threads[currentThread].ThreadState.Equals(ThreadState.Running))
                            {
                                _threads[currentThread].Join();
                            }
                            _threads[currentThread] = new Thread(new ParameterizedThreadStart(CompressTask));
                            byte[] copyBuffer = new byte[buffer.Length];
                            buffer.CopyTo(copyBuffer, 0);
                            CompressBuffer cbuf = new CompressBuffer(copyBuffer, dataRead, currentThread, currentBlock);
                            _threads[currentThread].Start(cbuf);
                            currentThread = (currentThread + 1) % _threadsNumber;
                            break;
                        }
                        else
                        {
                            currentThread = (currentThread + 1) % _threadsNumber;
                        }
                    }
                }
            }
            for (int i = 0; i < _threadsNumber; i++)
            {
                if (_threads[i].ThreadState.Equals(ThreadState.Running))
                {
                    _threads[i].Join();
                }
            }
            _calculationInProgress = false;
            _writer.Join();
        }

        protected override void CompressTask(object inputBuffer)
        {
            CompressBuffer cbuffer = (CompressBuffer)inputBuffer;
            using (MemoryStream _memoryStream = new MemoryStream())
            {
                using (GZipStream cs = new GZipStream(_memoryStream, CompressionMode.Compress))
                {
                    cs.Write(cbuffer.buffer, 0, cbuffer.actualLength);
                }

                byte[] compressedData = _memoryStream.ToArray();
                byte[] compressedDataWithLength = new byte[compressedData.Length + 8];
                byte[] lengthCompressed = BitConverter.GetBytes(compressedData.Length);
                byte[] lengthOriginal = BitConverter.GetBytes(cbuffer.actualLength);
                lengthCompressed.CopyTo(compressedDataWithLength, 0);
                lengthOriginal.CopyTo(compressedDataWithLength, 4);
                compressedData.CopyTo(compressedDataWithLength, 8);
                _mtxBlocks.WaitOne();
                _blocks.Add(cbuffer.blockNumber, compressedDataWithLength);
                _mtxBlocks.ReleaseMutex();
            }
            _threadReady[cbuffer.threadNumber] = true;
        }

        protected override void WriteToFile()
        {
            int blockCounter = 1;
            while (_calculationInProgress || (_blocks.Count > 0))
            {
                if (_cancelled)
                {
                    break;
                }
                _mtxBlocks.WaitOne();
                if (_blocks.ContainsKey(blockCounter))
                {
                    using (FileStream _outputFileStream = new FileStream(_outputFile, FileMode.Append))
                    {
                        _outputFileStream.Write(_blocks[blockCounter], 0, _blocks[blockCounter].Length);
                    }
                    _blocks.Remove(blockCounter);
                    _mtxBlocks.ReleaseMutex();
                    blockCounter++;
                }
                else
                {
                    _mtxBlocks.ReleaseMutex();
                    Thread.Sleep(10);
                }
            }
        }
    }
}
