using System;
using System.Collections.Generic;
using System.IO;

namespace hitslib
{
    public class HitsCompiler
    {
        private static byte[] generateEmptyBytes(int size)
        {
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < size; i++)
            {
                bytes.Add(0xFF);
            }
            return bytes.ToArray();
        }
        public List<Byte> compile(HitsProject project,string workingDir) {
            List<Byte> compiled = new List<byte>();
            int clips = 0;
            int offset = 0;
            byte option1 = 0;
            foreach (PlaylistItem item in project.PlayList) {
                clips++;
            }
            option1 += (byte)((project.PowerOnPlay==1) ? 0b00000001 : 0);
            option1 += (byte)((project.PowerOnLoop==1) ? 0b00000010 : 0);
            option1 += (byte)((project.AntiNoiseDebounce==1) ? 0b00000100 : 0);
            compiled.Add(0xFE);//0x00
            compiled.Add(0xEF);//0x01
            compiled.AddRange(generateEmptyBytes(6));//0x02 to 0x07
            compiled.Add(option1);//0x08 option byte
            compiled.Add((byte) clips);//0x09 clips count
            compiled.Add((byte)project.OKY1Config);//0x0A OKY1 Mode
            compiled.Add((byte)project.OKY2Config);//0x0B OKY2 Mode
            compiled.Add((byte)project.IOConfig);//0x0C IO Mode
            compiled.AddRange(generateEmptyBytes(3));//0x0D to 0x0F
            foreach (PlaylistItem item in project.PlayList)
            {
                compiled.Add((byte)(offset >> 16));
                compiled.Add((byte)(offset >> 8));
                compiled.Add((byte)offset);
                compiled.Add((byte)(item.sampleSize >> 16));
                compiled.Add((byte)(item.sampleSize >> 8));
                compiled.Add((byte)item.sampleSize);
                switch (item.audioQuality)
                {
                    case AudioQuality.Efficient:
                        compiled.Add(0xF3);
                        break;
                    case AudioQuality.Balance:
                        compiled.Add(0xF2);
                        break;
                    case AudioQuality.Standard:
                        compiled.Add(0xFF);
                        break;
                    default:
                        break;
                }
                compiled.Add(0xFF);
                offset += item.sampleSize;
            }
            compiled.AddRange(generateEmptyBytes(0x4000-compiled.Count));
            foreach (PlaylistItem item in project.PlayList)
            {
                Byte[] temp = File.ReadAllBytes(workingDir + "/" + item.fileName);
                compiled.AddRange(temp[0x4000..temp.Length]);
            }
            return compiled;
        }
    }
}
