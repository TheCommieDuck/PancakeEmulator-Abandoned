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

        internal static void SetFlags(int p1, int p2, int p3, int p4)
        {
            throw new NotImplementedException();
        }

        //reset to some instr
    }
}
