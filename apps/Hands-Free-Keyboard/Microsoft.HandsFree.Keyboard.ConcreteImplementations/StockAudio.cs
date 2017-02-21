using Microsoft.HandsFree.Keyboard.ConcreteImplementations.Properties;
using System.IO;

namespace Microsoft.HandsFree.Keyboard.ConcreteImplementations
{
    static class StockAudio
    {
        internal static readonly byte[] Click = LoadSound(Resources.Click);
        internal static readonly byte[] BeforeSpeaking = LoadSound(Resources.BeforeSpeaking);
        internal static readonly byte[] AfterSpeaking = LoadSound(Resources.AfterSpeaking);
        internal static readonly byte[] RecapCorrection = LoadSound(Resources.RecapCorrection);
        internal static readonly byte[] RecapSimple = LoadSound(Resources.RecapSimple);
        internal static readonly byte[] FillerMusic = LoadSound(Resources.FillerMusic);
        
        internal static readonly byte[] mArgh = LoadSound(Resources.Argh);
        internal static readonly byte[] mLaugh = LoadSound(Resources.Laugh);
        internal static readonly byte[] mCough = LoadSound(Resources.Cough);
        internal static readonly byte[] mUgh = LoadSound(Resources.Ugh);
        internal static readonly byte[] mPfft = LoadSound(Resources.Sarcastic);
        internal static readonly byte[] mOh = LoadSound(Resources.OhPositive);
        internal static readonly byte[] mHmm = LoadSound(Resources.HmmThinking);
        internal static readonly byte[] mSharpBreath = LoadSound(Resources.SharpBreath);

        internal static readonly byte[] fArgh = LoadSound(Resources.fArgh);
        internal static readonly byte[] fLaugh = LoadSound(Resources.fLaugh);
        internal static readonly byte[] fCough = LoadSound(Resources.fCough);
        internal static readonly byte[] fUgh = LoadSound(Resources.fUgh);
        internal static readonly byte[] fPfft = LoadSound(Resources.fSarcastic);
        internal static readonly byte[] fOh = LoadSound(Resources.fOh);
        internal static readonly byte[] fHmm = LoadSound(Resources.fHmmThinking);
        internal static readonly byte[] fSharpBreath = LoadSound(Resources.fSharpBreath);

        static byte[] LoadSound(Stream stream)
        {
            stream.Position = 0;
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);

            return bytes;
        }
    }
}
