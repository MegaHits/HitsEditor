using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hitslib
{
    interface IMegaHits
    {
        void audioToRaw(string inPath, string outPath, AudioQuality quality, bool trimAudio = false);
        void rawToHits(string inPath, string outPath, bool loop, bool emptySpaces, AudioQuality quality);
        void audioToHits(string inPath, string outPath, AudioQuality quality, bool trimAudio = false);
    }
}
