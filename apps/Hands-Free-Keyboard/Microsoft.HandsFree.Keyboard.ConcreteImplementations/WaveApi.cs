using System;
using System.Runtime.InteropServices;

namespace Microsoft.HandsFree.Keyboard.ConcreteImplementations
{
    static class WaveApi
    {
        const string winmm = "winmm";

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
        public struct WAVEOUTCAPS
        {
            public short wMid;
            public short wPid;
            public int vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szPname;
            public int dwFormats;
            public short wChannels;
            public short wReserved1;
            public int dwSupport;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WAVEFORMATEX
        {
            public short wFormatTag;
            public short nChannels;
            public int nSamplesPerSec;
            public int nAvgBytesPerSec;
            public short nBlockAlign;
            public short wBitsPerSample;
            public short cbSize;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WAVEHDR
        {
            public IntPtr lpData;
            public int dwBufferLength;
            public int dwBytesRecorded;
            public IntPtr dwUser;
            public int dwFlags;
            public int dwLoops;
            public IntPtr lpNext;
            public int reserved;
        }

        public delegate void waveOutProc(IntPtr hwo, int uMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);

        [DllImport(winmm, SetLastError = true)]
        public static extern int waveOutGetNumDevs();

        [DllImport(winmm, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int waveOutGetDevCaps(IntPtr uDeviceID, out WAVEOUTCAPS pwoc, int cbwoc);

        [DllImport(winmm, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int waveOutOpen(out IntPtr phwo, IntPtr uDeviceID, ref WAVEFORMATEX pwfx, waveOutProc dwCallback, IntPtr dwCallbackInstance, int fdwFlags);

        [DllImport(winmm, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int waveOutPrepareHeader(IntPtr hwo, ref WAVEHDR pwh, int dwVolume);

        [DllImport(winmm, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int waveOutSetVolume(IntPtr hwo, int cbwh);

        [DllImport(winmm, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int waveOutWrite(IntPtr hwo, ref WAVEHDR pwh, int cbwh);

        [DllImport(winmm, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int waveOutUnprepareHeader(IntPtr hwo, ref WAVEHDR pwh, int cbwh);

        [DllImport(winmm, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int waveOutReset(IntPtr hwo);

        [DllImport(winmm, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int waveOutClose(IntPtr hwo);
    }
}
