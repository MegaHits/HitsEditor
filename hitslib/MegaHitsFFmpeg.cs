using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace hitslib
{
    public class MegaHitsFFmpeg:IMegaHits
    {
        private List<byte> audioData = new List<byte>();
        private byte[] magicWord =       { 0xFE, 0xFF };
        private byte[] magicWordHQ =     { 0xFE, 0xFE };
        private byte[] magicWordXQ =     { 0xFE, 0xFC };
        private byte[] magicWordK22 =    { 0xFE, 0xFB };
        private byte[] magicWordEC =     { 0xFE, 0xF3 };
        private byte[] magicWordBL =     { 0xFE, 0xF2 };

        private static byte[] generateEmptyBytes(int size)
        {
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < size; i++)
            {
                bytes.Add(0xFF);
            }
            return bytes.ToArray();
        }
        public void audioToRaw(string inPath, string outPath, AudioQuality quality, bool trimAudio = false)
        {
            if (!File.Exists(inPath))
            {
                throw new FileNotFoundException(inPath + ": No such file or directory.");
            }
            AudioSampleRate sampleRate = AudioSampleRate.Default;

            // Saves the frame located on the 15th second of the video.
            switch (quality)
            {
                case AudioQuality.Standard:
                    sampleRate = AudioSampleRate.Hz6000;
                    break;
                case AudioQuality.HQ:
                    sampleRate = AudioSampleRate.Hz8000;
                    break;
                case AudioQuality.XQ:
                    sampleRate = AudioSampleRate.Hz12000;
                    break;
                case AudioQuality.K22:
                    sampleRate = AudioSampleRate.Hz22050;
                    break;
                case AudioQuality.Efficient:
                    sampleRate = AudioSampleRate.Hz4000;
                    break;
                case AudioQuality.Balance:
                    sampleRate = AudioSampleRate.Hz5000;
                    break;
                default:
                    throw new System.ArgumentException("Invalid audio quality");
            }
            using (Process process = new Process())
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    process.StartInfo.FileName = "ffmpeg";
                }
                else {
                    process.StartInfo.FileName = "./bin/ffmpeg.exe";
                }
                
                if (trimAudio)
                {
                    process.StartInfo.Arguments = $"-i \"{inPath}\" -f u8 -acodec pcm_u8 -ac 1 -af \"silenceremove = start_periods = 1:start_duration = 1:start_threshold = -60dB: detection = peak,aformat = dblp,areverse,silenceremove = start_periods = 1:start_duration = 1:start_threshold = -60dB: detection = peak,aformat = dblp,areverse\" -ar {sampleRate.ToString().Replace("Hz", "")} \"{outPath}\"";
                }
                else
                {
                    process.StartInfo.Arguments = $"-i \"{inPath}\" -f u8 -acodec pcm_u8 -ac 1 -ar {sampleRate.ToString().Replace("Hz", "")} \"{outPath}\"";
                }
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                //process.BeginOutputReadLine();
                process.WaitForExit();
            }

        }
        public void rawToHits(string inPath, string outPath, bool loop, bool emptySpaces, AudioQuality quality)
        {
            if (!File.Exists(inPath))
            {
                throw new FileNotFoundException(inPath + ": No such file or directory.");
            }
            audioData.Clear();
            audioData.AddRange(File.ReadAllBytes(inPath));
            byte[] intBytes = BitConverter.GetBytes(audioData.Count);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);
            byte[] songSizeHex = intBytes;
            if (emptySpaces)
            {
                audioData.InsertRange(0, generateEmptyBytes(16377));
            }
            if (loop)
            {
                audioData.Insert(0, 0x80);
            }
            else
            {
                audioData.Insert(0, 0);
            }
            audioData.InsertRange(0, songSizeHex);
            for (int i = songSizeHex.Length; i < 4; i++)
            {
                audioData.Insert(0, 0);
            }

            switch (quality)
            {
                case AudioQuality.Standard:
                    audioData.InsertRange(0, magicWord);
                    break;
                case AudioQuality.HQ:
                    audioData.InsertRange(0, magicWordHQ);
                    break;
                case AudioQuality.XQ:
                    audioData.InsertRange(0, magicWordXQ);
                    break;
                case AudioQuality.K22:
                    audioData.InsertRange(0, magicWordK22);
                    break;
                case AudioQuality.Efficient:
                    audioData.InsertRange(0, magicWordEC);
                    break;
                case AudioQuality.Balance:
                    audioData.InsertRange(0, magicWordBL);
                    break;
                default:
                    throw new System.ArgumentException("Invalid audio quality");
            }
            File.WriteAllBytes($"{outPath}", audioData.ToArray());
        }
        public void audioToHits(string inPath, string outPath, AudioQuality quality, bool trimAudio = false)
        {
            if (!File.Exists(inPath))
            {
                throw new FileNotFoundException(inPath + ": No such file or directory.");
            }
            string rawPath = Path.GetFileNameWithoutExtension(inPath) + ".raw";
            audioToRaw(inPath, rawPath, quality, trimAudio);
            rawToHits(rawPath, outPath, false, true, quality);
            if (File.Exists(rawPath))
            {
                File.Delete(rawPath);
            }
        }
    }
}
