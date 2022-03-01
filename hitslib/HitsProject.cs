using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hitslib
{
    enum OKYMode
    {
        DISABLE,
        NEXT_CLIP,
        PLAY,
        RANDOM_CLIP,
        RESET,
    }
    enum IOMode
    {
        DISABLE,
        BUSY_OUT,
        BLINK1_5HZ,
    }
    public class PlaylistItem
    {
        public string fileName { get; set; }
        public AudioQuality audioQuality { get; set; }
        public int sampleSize { get; set; }
    }
    public class HitsProject
    {
        public int MegaHitsFileVersion { get; set; }
        public string ProjectName { get; set; }
        public string ProjectNumber { get; set; }
        public string Description { get; set; }
        public int FlashSize { get; set; }
        public int PowerOnPlay { get; set; }
        public int PowerOnLoop { get; set; }
        public int AntiNoiseDebounce { get; set; }
        public int SampleRate { get; set; }
        public int OKY1Config { get; set; }
        public int OKY2Config { get; set; }
        public int IOConfig { get; set; }
        public List<PlaylistItem> PlayList { get; set; }
        public override string ToString()
        {
            return GetType().GetProperties()
                .Select(info => (info.Name, Value: info.GetValue(this, null) ?? "(null)"))
                .Aggregate(
                    new StringBuilder(),
                    (sb, pair) => sb.AppendLine($"{pair.Name}: {pair.Value}"),
                    sb => sb.ToString());
        }
    }
}
