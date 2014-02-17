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

        internal void WriteByte(ushort address, byte data)
        {
            throw new NotImplementedException();
        }

        public byte ReadByte(ushort p)
        {
            return Data[p];
        }

        public ushort ReadWord(ushort p)
        {
            return (ushort)(Data[p + 1] << 8 | Data[p]);
        }

        public void WriteWord(ushort address, ushort data)
        {
            WriteByte(address, (byte)(data & 0x00FF));
            WriteByte((ushort)(address + 1), (byte)(data >> 8));
        }
    }
}
