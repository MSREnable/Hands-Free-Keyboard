using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.HandsFree.Keyboard.ConcreteImplementations
{
    class WavePlayer
    {
        class WaveException : Exception
        {

        }

        readonly int _device;
        WaveApi.WAVEHDR _header;
        WaveApi.WAVEFORMATEX _format;
        IntPtr _hGlobal;
        WaveApi.waveOutProc _waveOutProc;

        readonly SemaphoreSlim _semaphoreInner = new SemaphoreSlim(0);

        readonly SemaphoreSlim _semaphoreOuter = new SemaphoreSlim(0);

        const bool TrimSpeach = true;

        WavePlayer(int device)
        {
            _device = device;
        }

        public static int DeviceCount => WaveApi.waveOutGetNumDevs();

        public static string GetDeviceName(int device)
        {
            var caps = new WaveApi.WAVEOUTCAPS();
            WaveApi.waveOutGetDevCaps((IntPtr)device, out caps, Marshal.SizeOf(caps));
            return caps.szPname;
        }

        public static WavePlayer Play(int device, byte[] buffer, double volume)
        {
            WavePlayer player = new WavePlayer(device);

            Debug.Assert(BitConverter.ToInt32(buffer, 0) == 0x46464952, "We have a RIFF file");

            var formatChunkOffset = 12;
            Debug.Assert(BitConverter.ToInt32(buffer, formatChunkOffset) == 0x20746D66, "We have a fmt chunk");

            player._format = new WaveApi.WAVEFORMATEX
            {
                wFormatTag = BitConverter.ToInt16(buffer, formatChunkOffset + 8),
                nChannels = BitConverter.ToInt16(buffer, formatChunkOffset + 10),
                nSamplesPerSec = BitConverter.ToInt32(buffer, formatChunkOffset + 12),
                nAvgBytesPerSec = BitConverter.ToInt32(buffer, formatChunkOffset + 16),
                nBlockAlign = BitConverter.ToInt16(buffer, formatChunkOffset + 20),
                wBitsPerSample = BitConverter.ToInt16(buffer, formatChunkOffset + 22),
                cbSize = BitConverter.ToInt16(buffer, formatChunkOffset + 24)
            };

            var dataChunkOffset = formatChunkOffset + 8 + BitConverter.ToInt32(buffer, formatChunkOffset + 4);

            Debug.Assert(BitConverter.ToInt32(buffer, dataChunkOffset) == 0x61746164, "We have a data chunk");

            var dataLength = BitConverter.ToInt32(buffer, dataChunkOffset + 4);

            // Potentially we might want to truncate the buffer to remove leading and training silence.
            var chunkStart = dataChunkOffset + 8;
            var chunkEnd = chunkStart + dataLength;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (TrimSpeach)
            {
                while (chunkStart < chunkEnd && buffer[chunkStart + 0] == 0 && buffer[chunkStart + 1] == 0)
                {
                    chunkStart += 2;
                }
                while (chunkStart < chunkEnd && buffer[chunkEnd - 1] == 0 && buffer[chunkEnd - 2] == 0)
                {
                    chunkEnd -= 2;
                }
            }

            player._hGlobal = Marshal.AllocHGlobal(dataLength);
            Marshal.Copy(buffer, chunkStart, player._hGlobal, chunkEnd - chunkStart);

            player._header = new WaveApi.WAVEHDR
            {
                lpData = player._hGlobal,
                dwBufferLength = chunkEnd - chunkStart
            };

            ThreadPool.QueueUserWorkItem((o) => player.Play(volume));

            return player;
        }

        internal static WavePlayer PlaySilence(TimeSpan timeSpan)
        {
            var player = new WavePlayer(-1);
            Task.Delay(timeSpan).ContinueWith((t) => player._semaphoreInner.Release());
            player._semaphoreInner.WaitAsync().ContinueWith((t) => player._semaphoreOuter.Release());
            return player;
        }

        internal void Stop()
        {
            _semaphoreInner.Release();
        }

        internal async Task WaitAsync()
        {
            await _semaphoreOuter.WaitAsync();
        }

        void WaveOutProc(IntPtr hwo, int uMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2)
        {
            switch (uMsg)
            {
                case 0x3BB:
                    //Debug.WriteLine("MM_WOM_OPEN");
                    break;

                case 0x3BC:
                    //Debug.WriteLine("MM_WOM_CLOSE");
                    break;

                case 0x3BD:
                    //Debug.WriteLine("MM_WOM_DONE");
                    _semaphoreInner.Release();
                    break;
            }
        }

        static void Check(Func<int> fn)
        {
            var error = fn();
            if (error != 0)
            {
                throw new WaveException();
            }
        }

        void Play(double volume)
        {
            try
            {
                var hWaveOut = IntPtr.Zero;

                unsafe
                {
                    fixed (WaveApi.WAVEHDR* pHeader = &_header)
                    {
                        _waveOutProc = WaveOutProc;

                        Check(() => WaveApi.waveOutOpen(out hWaveOut, (IntPtr)(_device), ref _format, _waveOutProc, IntPtr.Zero, 0x30000));
                        Check(() => WaveApi.waveOutPrepareHeader(hWaveOut, ref _header, Marshal.SizeOf(_header)));
                        var monoVolume = (int)(0xFFFF * volume) & 0xFFFF;
                        var stereoVolume = (monoVolume << 16) + monoVolume;
                        var result = WaveApi.waveOutSetVolume(hWaveOut, stereoVolume);

                        if (_semaphoreInner.CurrentCount == 0)
                        {
                            Check(() => WaveApi.waveOutWrite(hWaveOut, ref _header, Marshal.SizeOf(_header)));

                            _semaphoreInner.Wait();
                        }

                        Check(() => WaveApi.waveOutReset(hWaveOut));
                        Check(() => WaveApi.waveOutUnprepareHeader(hWaveOut, ref _header, Marshal.SizeOf(_header)));
                    }
                }
                Check(() => WaveApi.waveOutClose(hWaveOut));
            }
            catch (WaveException)
            {

            }

            Marshal.FreeHGlobal(_hGlobal);

            _semaphoreOuter.Release();
        }
    }
}
