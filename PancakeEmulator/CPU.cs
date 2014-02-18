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

        //if A = 0, if a subtraction was performed, if the bottom nibble carried, if the whole thing carried
        public void SetFlags(int zeroFlag, int subtractFlag, int halfCarryFlag, int carryFlag)
        {
            throw new NotImplementedException();
        }

        //reset to some instr
    }
}
