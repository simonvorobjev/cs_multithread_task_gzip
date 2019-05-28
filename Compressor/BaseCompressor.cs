using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Compressor
{
    public abstract class BaseCompressor
    {
        protected string _inputFile, _outputFile;
        protected Thread[] _threads;
        protected List<bool> _threadReady;
        protected int _threadsNumber = Environment.ProcessorCount;
        protected Dictionary<int, byte[]> _blocks;
        protected Mutex _mtxBlocks = new Mutex();
        protected bool _calculationInProgress;
        protected Thread _writer;
        protected bool _cancelled;

        protected BaseCompressor(string inputFile, string outputFile)
        {
            _inputFile = inputFile;
            _outputFile = outputFile;
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
            _threads = new Thread[_threadsNumber];
            _threadReady = new List<bool>();
            for (int i = 0; i < _threadsNumber; i++)
            {
                _threadReady.Add(true);
            }
            _calculationInProgress = false;
            _blocks = new Dictionary<int, byte[]>();
            _cancelled = false;
        }

        public abstract void Calculate();

        protected abstract void WriteToFile();

        protected abstract void CompressTask(object buffer);

        public void CancelJob()
        {
            _cancelled = true;
        }
    }
}
