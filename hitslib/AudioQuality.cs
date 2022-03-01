using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace hitslib
{
    public enum AudioQuality
    {
        Efficient,
        Balance,
        Standard,
        HQ,
        XQ,
        K22
    }
    public enum AudioSampleRate
    {
        Default,
        Hz4000,
        Hz5000,
        Hz6000,
        Hz8000,
        Hz12000,
        Hz22050
    }
    public static class AudioQualityUtil {
        public static AudioQuality GetAudioQualityFromFile(string inPath) {
            byte[] file = File.ReadAllBytes(inPath);
            if (file[0]!=0xFE)
            {
                throw new Exception("Not a MegaHits single song ROM file");
            }
            switch (file[1])
            {
                case 0xFF:
                    return AudioQuality.Standard;
                case 0xFE:
                    return AudioQuality.HQ;
                case 0xFC:
                    return AudioQuality.XQ;
                case 0xFB:
                    return AudioQuality.K22;
                case 0xF3:
                    return AudioQuality.Efficient;
                case 0xF2:
                    return AudioQuality.Balance;
                default:
                    throw new Exception("Not a MegaHits single song ROM file");
            }
        }
        public static int GetSampleRateInInt(AudioQuality audioQuality) {
            switch (audioQuality)
            {
                case AudioQuality.Efficient:
                    return 4000;
                case AudioQuality.Balance:
                    return 5000;
                case AudioQuality.Standard:
                    return 6000;
                case AudioQuality.HQ:
                    return 8000;
                case AudioQuality.XQ:
                    return 12000;
                case AudioQuality.K22:
                    return 22050;
                default:
                    return 0;
            }
        }
    }
}
