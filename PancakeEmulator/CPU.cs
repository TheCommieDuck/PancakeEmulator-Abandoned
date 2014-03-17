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
        public ushort SP, PC;

        public ushort HL
        {
            get
            {
                return (ushort)(H << 8 | L);
            }
            set
            {
                H = (byte)(value >> 8);
                L = (byte)value;
            }
        }
        //flags
        public byte ZeroFlag, SubtractionFlag, HalfFlag, CarryFlag;
        //clock cycles
        public uint ClockCycles;
        public int EnableSignal;
        public static int DisableSignal;
        public static bool Stop;

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

        public int EnableInterruptSignal { get; set; }

        public String RegisterDump
        {
            get
            {
                return String.Format("A: {0:X2}, B: {1:X2}, C: {2:X2}, D: {3:X2}, E: {4:X2}, HL: {5:X4}, SP: {6:X4}, CurrentPC: {7:X4}",
                    A, B, C, D, E, HL, SP, PC - 1);
            }
        }

        public String FlagDump
        {
            get
            {
                return String.Format("{0}{1}{2}{3}", ZeroFlag, SubtractionFlag, HalfFlag, CarryFlag);
            }
        }
    }
}
