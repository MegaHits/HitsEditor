using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hitslib
{
    public class MegaHitsNAudio : IMegaHits
    {
        private bool isInited = false;
        private WaveOut _waveOut = new WaveOut();
        public void audioToHits(string inPath, string outPath, AudioQuality quality, bool trimAudio = false)
        {
            throw new NotImplementedException();
        }

        public void audioToRaw(string inPath, string outPath, AudioQuality quality, bool trimAudio = false)
        {
            throw new NotImplementedException();
        }

        public byte[] getRAWArrayFromAudio(string inPath) {
            throw new NotImplementedException();
        }

        public void stopAudio() {
            if (!isInited)
            {
                return;
            }
            isInited = false;
            _waveOut.Stop();
            _waveOut.Dispose();
        }
        public void playAudio(Byte[] audioStream, AudioQuality quality) {
            if (isInited)
            {
                stopAudio();
            }
            isInited = true;
            RawSourceWaveStream waveStream = new RawSourceWaveStream(new MemoryStream(audioStream[0x4000..audioStream.Length]),new WaveFormat(AudioQualityUtil.GetSampleRateInInt(quality),8,1));
            //WdlResamplingSampleProvider resampler = new WdlResamplingSampleProvider(waveStream.ToSampleProvider(), 5000);
            //RawSourceWaveStream pcmstream = new RawSourceWaveStream(provider.ToSampleProvider(), new WaveFormat());
            _waveOut.Init(waveStream.ToSampleProvider());
            //provider.Read(pcmStream, 0, resampler.)
            //_waveOut.Init(provider);
            _waveOut.Play();
        }

        public void rawToHits(string inPath, string outPath, bool loop, bool emptySpaces, AudioQuality quality)
        {
            throw new NotImplementedException();
        }
    }
}
