using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Compressor
{
    class MultithreadDecompressor : BaseCompressor
    {

        public MultithreadDecompressor(string inputFile, string outputFile) : base(inputFile, outputFile)
        {
        }

        public override void Calculate()
        {
            _calculationInProgress = true;
            _writer = new Thread(new ThreadStart(WriteToFile));
            _writer.Start();
            using (FileStream _inputFileStream = new FileStream(_inputFile, FileMode.Open))
            {
                int currentThread = 0;
                int currentBlock = 0;
                while (_inputFileStream.Position < _inputFileStream.Length)
                {
                    if (_cancelled)
                    {
                        break;
                    }
                    currentBlock++;
                    byte[] lengthBytes = new byte[4];
                    _inputFileStream.Read(lengthBytes, 0, 4);
                    var lengthCompressed = BitConverter.ToInt32(lengthBytes, 0);
                    byte[] _compressedBuffer = new byte[lengthCompressed];
                    _inputFileStream.Read(lengthBytes, 0, 4);
                    var lengthOriginal = BitConverter.ToInt32(lengthBytes, 0);
                    _inputFileStream.Read(_compressedBuffer, 0, lengthCompressed);
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
                            byte[] copyBuffer = new byte[_compressedBuffer.Length];
                            _compressedBuffer.CopyTo(copyBuffer, 0);
                            _threads[currentThread].Start(new CompressBuffer(copyBuffer, lengthOriginal, currentThread, currentBlock));
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
                if ((_threads[i] != null) && _threads[i].ThreadState.Equals(ThreadState.Running))
                {
                    _threads[i].Join();
                }
            }
            _calculationInProgress = false;
            _writer.Join();
        }

        protected override void CompressTask(object decompressBuffer)
        {
            CompressBuffer dbuffer = (CompressBuffer)decompressBuffer;
            using (GZipStream gzipStream = new GZipStream(new MemoryStream(dbuffer.buffer), CompressionMode.Decompress))
            {
                var result = new byte[dbuffer.actualLength];
                gzipStream.Read(result, 0, dbuffer.actualLength);
                _mtxBlocks.WaitOne();
                _blocks.Add(dbuffer.blockNumber, result);
                _mtxBlocks.ReleaseMutex();
            }
            _threadReady[dbuffer.threadNumber] = true;
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
