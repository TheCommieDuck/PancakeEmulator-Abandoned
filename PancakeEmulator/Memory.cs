using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PancakeEmulator
{
    class Memory
    {
        public byte[] Data;
        //read 8 bit byte
        public byte rb(ushort pc)
        {
            return 0;
        }

        internal static void WriteByte(ushort p1, byte p2)
        {
            throw new NotImplementedException();
        }

        internal static byte ReadByte(ushort word)
        {
            throw new NotImplementedException();
        }

        public static ushort GetWord(ushort p)
        {
            return (ushort)(Data[p + 1] << 8 | Data[p]);
        }
    }
}
