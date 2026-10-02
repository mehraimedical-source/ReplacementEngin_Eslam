using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace MEHR_PACSViewer
{
    internal sealed class WaveInRecorder : IDisposable
    {
        private const int WIM_DATA = 0x3C0;
        private const int CALLBACK_FUNCTION = 0x00030000;
        private const int BUFFER_COUNT = 4;
        private const int SAMPLE_RATE = 44100;
        private const short BITS = 16;
        private const short CHANNELS = 1;

        private IntPtr _waveIn = IntPtr.Zero;
        private WaveInProc _callback;
        private readonly List<BufferInfo> _buffers = new List<BufferInfo>();
        private FileStream _stream;
        private string _fileName;
        private volatile bool _recording;
        private volatile bool _paused;
        private volatile int _level;
        private long _dataBytes;
        private readonly object _sync = new object();

        public int Level { get { return _level; } }
        public bool IsRecording { get { return _recording; } }
        public bool IsPaused { get { return _paused; } }
        public int RecordedSeconds { get { return (int)(_dataBytes / (SAMPLE_RATE * CHANNELS * (BITS / 8))); } }

        public void Start(string fileName)
        {
            Stop(false);
            _fileName = fileName;
            _dataBytes = 0;
            _level = 0;
            _stream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            WriteWaveHeader(_stream, 0);

            WAVEFORMATEX format = new WAVEFORMATEX();
            format.wFormatTag = 1;
            format.nChannels = CHANNELS;
            format.nSamplesPerSec = SAMPLE_RATE;
            format.wBitsPerSample = BITS;
            format.nBlockAlign = (short)(CHANNELS * BITS / 8);
            format.nAvgBytesPerSec = SAMPLE_RATE * format.nBlockAlign;
            format.cbSize = 0;

            _callback = new WaveInProc(WaveCallback);
            int result = waveInOpen(out _waveIn, -1, ref format, _callback, IntPtr.Zero, CALLBACK_FUNCTION);
            Check(result, "waveInOpen");

            try
            {
                int bufferSize = format.nAvgBytesPerSec / 10;
                int hdrSize = Marshal.SizeOf(typeof(WAVEHDR));
                for (int i = 0; i < BUFFER_COUNT; i++)
                {
                    BufferInfo b = new BufferInfo();
                    b.Data = Marshal.AllocHGlobal(bufferSize);
                    b.Header = Marshal.AllocHGlobal(hdrSize);
                    WAVEHDR h = new WAVEHDR();
                    h.lpData = b.Data;
                    h.dwBufferLength = bufferSize;
                    Marshal.StructureToPtr(h, b.Header, false);
                    Check(waveInPrepareHeader(_waveIn, b.Header, hdrSize), "waveInPrepareHeader");
                    Check(waveInAddBuffer(_waveIn, b.Header, hdrSize), "waveInAddBuffer");
                    _buffers.Add(b);
                }
                _recording = true;
                _paused = false;
                Check(waveInStart(_waveIn), "waveInStart");
            }
            catch
            {
                Stop(false);
                throw;
            }
        }

        public void Pause()
        {
            if (!_recording || _paused) return;
            Check(waveInStop(_waveIn), "waveInStop");
            _paused = true;
            _level = 0;
        }

        public void Resume()
        {
            if (!_recording || !_paused) return;
            _paused = false;
            Check(waveInStart(_waveIn), "waveInStart");
        }

        public void Stop(bool keepFile)
        {
            _recording = false;
            _paused = false;
            _level = 0;

            if (_waveIn != IntPtr.Zero)
            {
                waveInReset(_waveIn);
                int hdrSize = Marshal.SizeOf(typeof(WAVEHDR));
                for (int i = 0; i < _buffers.Count; i++)
                    waveInUnprepareHeader(_waveIn, _buffers[i].Header, hdrSize);
                waveInClose(_waveIn);
                _waveIn = IntPtr.Zero;
            }

            for (int i = 0; i < _buffers.Count; i++)
            {
                if (_buffers[i].Header != IntPtr.Zero) Marshal.FreeHGlobal(_buffers[i].Header);
                if (_buffers[i].Data != IntPtr.Zero) Marshal.FreeHGlobal(_buffers[i].Data);
            }
            _buffers.Clear();

            lock (_sync)
            {
                if (_stream != null)
                {
                    if (keepFile)
                    {
                        _stream.Position = 0;
                        WriteWaveHeader(_stream, _dataBytes);
                    }
                    _stream.Close();
                    _stream = null;
                }
            }

            if (!keepFile && !String.IsNullOrEmpty(_fileName))
            {
                try { if (File.Exists(_fileName)) File.Delete(_fileName); } catch { }
            }
        }

        private void WaveCallback(IntPtr hwi, int msg, IntPtr instance, IntPtr headerPtr, IntPtr reserved)
        {
            if (msg != WIM_DATA || headerPtr == IntPtr.Zero) return;
            WAVEHDR h = (WAVEHDR)Marshal.PtrToStructure(headerPtr, typeof(WAVEHDR));

            if (_recording && !_paused && h.dwBytesRecorded > 0)
            {
                byte[] data = new byte[h.dwBytesRecorded];
                Marshal.Copy(h.lpData, data, 0, data.Length);
                lock (_sync)
                {
                    if (_stream != null)
                    {
                        _stream.Write(data, 0, data.Length);
                        _dataBytes += data.Length;
                    }
                }
                _level = CalculateLevel(data);
            }

            if (_recording && _waveIn != IntPtr.Zero)
            {
                h.dwBytesRecorded = 0;
                Marshal.StructureToPtr(h, headerPtr, false);
                waveInAddBuffer(_waveIn, headerPtr, Marshal.SizeOf(typeof(WAVEHDR)));
            }
        }

        private static int CalculateLevel(byte[] data)
        {
            if (data.Length < 2) return 0;
            double sum = 0;
            int count = data.Length / 2;
            for (int i = 0; i + 1 < data.Length; i += 2)
            {
                short sample = (short)(data[i] | (data[i + 1] << 8));
                double v = sample / 32768.0;
                sum += v * v;
            }
            double rms = Math.Sqrt(sum / count);
            if (rms <= 0.00001) return 0;
            double db = 20.0 * Math.Log10(rms);
            int level = (int)((db + 60.0) * 100.0 / 60.0);
            if (level < 0) level = 0;
            if (level > 100) level = 100;
            return level;
        }

        private static void WriteWaveHeader(Stream stream, long dataLength)
        {
            BinaryWriter w = new BinaryWriter(stream);
            w.Write(new char[] { 'R','I','F','F' });
            w.Write((int)(36 + dataLength));
            w.Write(new char[] { 'W','A','V','E' });
            w.Write(new char[] { 'f','m','t',' ' });
            w.Write(16);
            w.Write((short)1);
            w.Write(CHANNELS);
            w.Write(SAMPLE_RATE);
            w.Write(SAMPLE_RATE * CHANNELS * (BITS / 8));
            w.Write((short)(CHANNELS * BITS / 8));
            w.Write(BITS);
            w.Write(new char[] { 'd','a','t','a' });
            w.Write((int)dataLength);
            w.Flush();
        }

        private static void Check(int result, string operation)
        {
            if (result != 0) throw new InvalidOperationException(operation + " failed. WinMM error: " + result);
        }

        public void Dispose() { Stop(false); }

        private sealed class BufferInfo { public IntPtr Data; public IntPtr Header; }

        private delegate void WaveInProc(IntPtr hwi, int msg, IntPtr instance, IntPtr param1, IntPtr param2);

        [StructLayout(LayoutKind.Sequential)]
        private struct WAVEFORMATEX
        {
            public short wFormatTag; public short nChannels; public int nSamplesPerSec; public int nAvgBytesPerSec;
            public short nBlockAlign; public short wBitsPerSample; public short cbSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WAVEHDR
        {
            public IntPtr lpData; public int dwBufferLength; public int dwBytesRecorded; public IntPtr dwUser;
            public int dwFlags; public int dwLoops; public IntPtr lpNext; public IntPtr reserved;
        }

        [DllImport("winmm.dll")] private static extern int waveInOpen(out IntPtr phwi, int deviceId, ref WAVEFORMATEX format, WaveInProc callback, IntPtr instance, int flags);
        [DllImport("winmm.dll")] private static extern int waveInPrepareHeader(IntPtr hwi, IntPtr header, int size);
        [DllImport("winmm.dll")] private static extern int waveInUnprepareHeader(IntPtr hwi, IntPtr header, int size);
        [DllImport("winmm.dll")] private static extern int waveInAddBuffer(IntPtr hwi, IntPtr header, int size);
        [DllImport("winmm.dll")] private static extern int waveInStart(IntPtr hwi);
        [DllImport("winmm.dll")] private static extern int waveInStop(IntPtr hwi);
        [DllImport("winmm.dll")] private static extern int waveInReset(IntPtr hwi);
        [DllImport("winmm.dll")] private static extern int waveInClose(IntPtr hwi);
    }
}
