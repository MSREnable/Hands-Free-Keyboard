namespace Microsoft.HandsFree.ArcReactor
{
    public class NeoPixelColor
    {
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        public static NeoPixelColor FromRGB888(uint color) => new NeoPixelColor
        {
            Red   = (byte) ((color & 0x00FF0000) >> 16),
            Green = (byte) ((color & 0x0000FF00) >> 8),
            Blue  = (byte)  (color & 0x000000FF)
        };

        public uint ToRGB888()
        {
            return ((uint)Red << 16) | ((uint)Green << 8) | Blue;
        }

        public static NeoPixelColor FromRGB565(ushort color) => new NeoPixelColor
        {
            Red   = (byte) ((color & 0xF800) >> 11),
            Green = (byte) ((color & 0x07F0) >> 5),
            Blue  = (byte)  (color & 0x003F)
        };

        public ushort ToRGB565()
        {
            return (ushort)(((Red / 8) << 11) | ((Green / 4) << 5) | (Blue / 8));
        }
    }
}
