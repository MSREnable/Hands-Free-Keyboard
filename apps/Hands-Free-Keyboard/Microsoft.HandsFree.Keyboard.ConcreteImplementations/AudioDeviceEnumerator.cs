using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.HandsFree.Keyboard.ConcreteImplementations
{
    class AudioDeviceEnumerator
    {
        public static IEnumerable<string> GetDevices()
        {
            var count = WaveApi.waveOutGetNumDevs();
            for (var index = 0; index < count; index++)
            {
                var caps = new WaveApi.WAVEOUTCAPS();
                WaveApi.waveOutGetDevCaps((IntPtr)index, out caps, Marshal.SizeOf(caps));
                yield return caps.szPname;
            }
        }

        public static int GetDeviceIndex(string device)
        {
            var count = WaveApi.waveOutGetNumDevs();

            var index = 0;
            var found = false;
            while (index < count && !found)
            {
                var caps = new WaveApi.WAVEOUTCAPS();
                WaveApi.waveOutGetDevCaps((IntPtr)index, out caps, Marshal.SizeOf(caps));

                if (caps.szPname == device)
                {
                    found = true;
                }
                else
                {
                    index++;
                }
            }

            return found ? index : -1;
        }
    }
}
