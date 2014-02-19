using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeEmulator
{
    class Processor
    {
        //registers
        public byte A, B, C, D, E, F, H, L;
        //pointery-registers
        public ushort SP, PC, HL;
        //flags
        public byte ZeroFlag, SubtractionFlag, HalfFlag, CarryFlag;
        //clock cycles
        public uint ClockCycles;
        public static int EIsignaled;

        //if A = 0, if a subtraction was performed, if the bottom nibble carried, if the whole thing carried
        public void SetFlags(int zeroFlag, int subtractFlag, int halfCarryFlag, int carryFlag)
        {
            ZeroFlag = (byte)zeroFlag;
            SubtractionFlag = (byte)subtractFlag;
            HalfFlag = (byte)halfCarryFlag;
            CarryFlag = (byte)carryFlag;
        }

        //reset to some instr

        public void Reset()
        {
            A = 0x01;
            B = 0;
            C = 0x13;
            D = 0;
            E = 0xD8;
            H = 0x01;
            L = 0x4D;
            PC = 0x0100;
            SP = 0xFFFE;
            SetFlags(1, 0, 1, 1);
        }
    }
}
