using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PancakeEmulator
{
    class Memory
    {
        public static byte[] Logo =
        {
        0xCE, 0xED, 0x66, 0x66, 0xCC, 0x0D, 0x00, 0x0B, 0x03, 0x73, 0x00,
        0x83, 0x00, 0x0C, 0x00, 0x0D, 0x00, 0x08, 0x11, 0x1F, 0x88, 0x89,
        0x00, 0x0E, 0xDC, 0xCC, 0x6E, 0xE6, 0xDD, 0xDD, 0xD9, 0x99, 0xBB,
        0xBB, 0x67, 0x63, 0x6E, 0x0E, 0xEC, 0xCC, 0xDD, 0xDC, 0x99, 0x9F,
        0xBB, 0xB9, 0x33, 0x3E
        };

        public byte[] Data;

        public Memory()
        {
            //2^16
            Data = new byte[65536];
        }

        internal void WriteByte(ushort address, byte data)
        {
            //if it writes to ram, we need to mirror it
            //borrowed from sharpgb
            if ((address >= 0xE000) && (address <= 0xF300))
                this.Data[0xC000 + (address - 0xE000)] = data;
            else if ((address >= 0xC000) && (address <= 0xD300))
                this.Data[0xE000 + (address - 0xC000)] = data;
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

        public void Reset()
        {
            //set the memory values
            Data[0xFF05] = 0; //TIMA
            Data[0xFF06] = 0; //TMA
            Data[0xFF07] = 0; //TAC
            Data[0xFF10] = 0x80; //NR10
            Data[0xFF11] = 0xBF; //NR11
            Data[0xFF12] = 0xF3; //NR12
            Data[0xFF14] = 0xBF; //NR14
            Data[0xFF16] = 0x3F; //NR21
            Data[0xFF17] = 0; //NR22
            Data[0xFF19] = 0xBF; //NR24
            Data[0xFF1A] = 0x7F; //NR30
            Data[0xFF1B] = 0xFF; //NR31
            Data[0xFF1C] = 0x9F; //NR32
            Data[0xFF1E] = 0xBF; //NR33
            Data[0xFF20] = 0xFF; //NR41
            Data[0xFF21] = 0; //NR42
            Data[0xFF22] = 0; //NR43
            Data[0xFF23] = 0xBF; //NR30?
            Data[0xFF24] = 0x77; //NR50
            Data[0xFF25] = 0xF3; //NR51
            Data[0xFF26] = 0xF1; //NR52 (F0 for super gameboy)

            Data[0xFF40] = 0x91; //LCDC
            Data[0xFF42] = 0; //SCY
            Data[0xFF43] = 0; //SCX
            Data[0xFF45] = 0; //LYC
            Data[0xFF47] = 0xFC; //BGP
            Data[0xFF48] = 0xFF; //OBP0
            Data[0xFF49] = 0xFF; //OBP1
            Data[0xFF4A] = 0; //WY
            Data[0xFF4B] = 0; //WX
            Data[0xFFFF] = 0; //IE
        }
    }
}
