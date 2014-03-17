using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeEmulator
{
    /* THE BIG TO-DO LIST
     * -Interrupts, halts, stops, all that jazz. Priority: bottom, fix the bugs first
     * Consider setting up semi-unit-testing. At least some kind of 'testbed' with the test roms. //Priority: lower than interrupts, really
     * Graphics - I think this is the lowest one..
     * Add in the missing CB opcodes (I think it's mostly shifts and resetting bits) In progress!
     */

    class Emulator
    {
        public Processor Processor { get; set; }

        public Memory Memory { get; set; }

        public bool IsRunning { get; set; }

        public int NumberInstructions;

        public uint LYReset;

        public static void Main(String[] args)
        {
            Emulator main = new Emulator();
            main.Processor = new Processor();
            main.Memory = new Memory();
            Rom rom = new Rom();
            rom.LoadFromFile();
            main.LoadRom(rom);
            main.Run();
        }

        private void LoadRom(Rom rom)
        {
            Memory.Reset();
            Processor.Reset();
            //if the type is 0 it's a rom only cartridge giving it a different address space, or something.
            for (int i = 0; i < 0x8000; ++i)
                Memory.Data[i] = rom.Data[i];

            //ignore the checking stuff, it's not really needed to be honest
            //nintendo logo check, complement check, checksum (not used regardless)

            //reset the video?
        }

        public void Run()
        {
            //get the instruction and increment the program count
            IsRunning = true;
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            while (IsRunning)
            {
                if (LYReset >= 456)
                {
                    LYReset = 0; //for now..
                    Memory.Data[0xFF44]++;
                    //TODO: interrupt stuff
                    if (Memory.Data[0xFF44] > 153)
                        Memory.Data[0xFF44] = 0; //reset the LY 
                    if (((Memory.Data[0xFF41] /*lcd status*/ & 0x40) > 0) &&  (Memory.Data[0xFF44] == Memory.Data[0xFF45])) //LY and LYC
                        Memory.Data[0xFF41] |= 0x04;   // bit on
                    else 
                        Memory.Data[0xFF41] &= 0xFB;  // bit off

                }

                byte instr = Memory.Data[Processor.PC];
                ushort currentPC = Processor.PC; //then we move the program counter forward for data reading
                //Console.WriteLine("Performing operation {0:X2} at {1:X2}", instr, Processor.PC);
                Processor.PC++;
                //0x0210 is successful
                /*0xC182:
                 * A - C3 B 01 C 00 D D0 E 00 H c1 L 85 Flags 1101
                 * SP DFFB PC C182
                 * C06a - srl b - goes from 0000 FF to 7F 0001
                 */
                if (currentPC == 0x0207)
                    System.Diagnostics.Debugger.Break();
                Decode(instr);

                NumberInstructions++;
            }
        }

        public void Decode(byte opcode)
        {
            //so you read in the opcode at PC, then act on the data /following/ PC (thus the PC+1 everywhere)
            //Opcode decoding table from SharpGB
            //https://code.google.com/p/sharpgb/
            //temp variables
            ushort tempWord;
            //byte tempByte;
            //int halfcarry, carry;
            //ushort address; //a temporary address
            bool err = false;
            uint oldClockCycle = Processor.ClockCycles;
            switch (opcode)
            {
                #region  Immediate Load Instructions 

                // immediate loads
                case 0x3E:  // A <- immediate  
                    ImmediateLoadOp(ref Processor.A);
                    break;
                case 0x06:  // B <- immediate  
                    ImmediateLoadOp(ref Processor.B);
                    break;
                case 0x0E:  // C <- immediate  
                    ImmediateLoadOp(ref Processor.C);
                    break;
                case 0x16:  // D <- immediate  
                    ImmediateLoadOp(ref Processor.D);
                    break;
                case 0x1E:  // E <- immediate  
                    ImmediateLoadOp(ref Processor.E);
                    break;
                case 0x26:  // H <- immediate  
                    ImmediateLoadOp(ref Processor.H);
                    break;
                case 0x2E:  // L <- immediate  
                    ImmediateLoadOp(ref Processor.L);
                    break;
                case 0x01:  // BC <- immediate  
                    Immediate16bitLoadOp(ref Processor.C, ref Processor.B);
                    break;
                case 0x11:  // DE <- immediate  
                    Immediate16bitLoadOp(ref Processor.E, ref Processor.D);
                    break;
                case 0x21:  // HL <- immediate  
                    Immediate16bitLoadOp(ref Processor.L, ref Processor.H);
                    break;
                case 0x31:  // SP <- immediate
                    Immediate16bitLoadOp(ref Processor.SP);
                    break;
                case 0x36:  // (HL) <- immediate  the adress pointed at by HL
                    ImmediateMemoryLocationLoadOp(Processor.HL);
                    break;
#endregion

                #region Memory To Register Transfer Instructions

                // memory to register transfer
                /*case 0xF2:    // A <- (0xFF00 + C)
                    word = (ushort)(0xFF00 + Processor.C);
                    Processor.A = Memory.ReadByte(word);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break; supposedly removed.*/
                case 0x0A:    // A <- (BC)
                    MemoryToRegisterOp(ref Processor.A, (ushort)(Processor.B << 8 | Processor.C));
                    break;
                case 0x1A:    // A <- (DE)
                    MemoryToRegisterOp(ref Processor.A, (ushort)(Processor.D << 8 | Processor.E));
                    break;
                case 0x7E:  // A <- (HL)
                    MemoryToRegisterOp(ref Processor.A, Processor.HL);
                    break;
                case 0x46:  // B <- (HL)
                    MemoryToRegisterOp(ref Processor.B, Processor.HL);
                    break;
                case 0x4E:  // C <- (HL)
                    MemoryToRegisterOp(ref Processor.C, Processor.HL);
                    break;
                case 0x56:  // D <- (HL)
                    MemoryToRegisterOp(ref Processor.D, Processor.HL);
                    break;
                case 0x5E:  // E <- (HL)
                    MemoryToRegisterOp(ref Processor.E, Processor.HL);
                    break;
                case 0x66:  // H <- (HL)
                    MemoryToRegisterOp(ref Processor.H, Processor.HL);
                    break;
                case 0x6E:  // L <- (HL)
                    MemoryToRegisterOp(ref Processor.L, Processor.HL);
                    break;
                case 0x2A:  // A <- (HL), HL++. No flags.
                    MemoryToRegisterOp(ref Processor.A, Processor.HL);
                    Processor.HL++;
                    break;
                case 0xFA:  // A <- (nn immediate)
                    MemoryToRegisterOp(ref Processor.A, Memory.ReadWord(Processor.PC));
                    Processor.PC += 2; //since we used 16bit operand thingy
                    Processor.ClockCycles += 8; //extra cycles for the extra read
                    break;
                case 0xF0:  // A <- (0xFF00+ n immediate)
                    MemoryToRegisterOp(ref Processor.A, (ushort)(0xFF00 + Memory.Data[Processor.PC]));
                    Processor.PC++; //since it is a 2 operand op
                    Processor.ClockCycles += 4; //extra cycles
                    break;
#endregion

                #region Register To Memory Transfer Instructions

                case 0xE2:    // (0xFF00 + C) <- A
                    RegisterToMemoryOp((ushort)(0xFF00 + Processor.C), Processor.A);
                    Processor.ClockCycles += 8;
                    break;
                case 0x02:  // (BC) <- A
                    RegisterToMemoryOp((ushort)(Processor.B << 8 | Processor.C), Processor.A);
                    break;
                case 0x12:  // (DE) <- A
                    RegisterToMemoryOp((ushort)(Processor.D << 8 | Processor.E), Processor.A);
                    break;
                case 0x77:  // (HL) <- A
                    RegisterToMemoryOp(Processor.HL, Processor.A);
                    break;
                case 0x70:  // (HL) <- B
                    RegisterToMemoryOp(Processor.HL, Processor.A);
                    break;
                case 0x71:  // (HL) <- C
                    RegisterToMemoryOp(Processor.HL, Processor.A);
                    break;
                case 0x72:  // (HL) <- D
                    RegisterToMemoryOp(Processor.HL, Processor.A);
                    break;
                case 0x73:  // (HL) <- E
                    RegisterToMemoryOp(Processor.HL, Processor.A);
                    break;
                case 0x74:  // (HL) <- H
                    RegisterToMemoryOp(Processor.HL, Processor.A);
                    break;
                case 0x75:  // (HL) <- L
                    RegisterToMemoryOp(Processor.HL, Processor.A);
                    break;
                case 0xEA:  // (nn) <- A
                    RegisterToMemoryOp(Memory.ReadWord(Processor.PC), Processor.A);
                    Processor.PC += 2; //extra increment for operands
                    Processor.ClockCycles += 8;
                    break;
                case 0xE0:  // (0xFF00+ n immediate) <- A
                    RegisterToMemoryOp((ushort)(0xFF00 + Memory.Data[Processor.PC]), Processor.A);
                    Processor.PC++; //extra increment
                    Processor.ClockCycles += 4;
                    break;
                case 0x32:  // (HL) <- A, HL--    
                    RegisterToMemoryOp(Processor.HL, Processor.A);
                    Processor.HL--;
                    break;
                case 0x22:  // (HL) <- A, HL++    
                    RegisterToMemoryOp(Processor.HL, Processor.A);
                    Processor.HL++;
                    break;
                case 0x08:  // (nn) <- SP No macro for this one
                    Memory.WriteWord(Memory.ReadWord(Processor.PC), Processor.SP);
                    Processor.PC += 2;
                    Processor.ClockCycles += 20;
                    break;
#endregion

                #region Register To Register Transfer Instructions

                case 0x7F:  // A <- A
                    Processor.ClockCycles += 4;
                    break;
                case 0x78:  // A <- B
                    RegisterToRegisterOp(ref Processor.A, Processor.B);
                    break;
                case 0x79:  // A <- C
                    RegisterToRegisterOp(ref Processor.A, Processor.C);
                    break;
                case 0x7A:  // A <- D
                    RegisterToRegisterOp(ref Processor.A, Processor.D);
                    break;
                case 0x7B:  // A <- E
                    RegisterToRegisterOp(ref Processor.A, Processor.E);
                    break;
                case 0x7C:  // A <- H
                    RegisterToRegisterOp(ref Processor.A, Processor.H);
                    break;
                case 0x7D:  // A <- L
                    RegisterToRegisterOp(ref Processor.A, Processor.L);
                    break;
                case 0x47:  // B <- A
                    RegisterToRegisterOp(ref Processor.B, Processor.A);
                    break;
                case 0x40:  // B <- B
                    Processor.ClockCycles += 4;
                    break;
                case 0x41:  // B <- C
                    RegisterToRegisterOp(ref Processor.B, Processor.C);
                    break;
                case 0x42:  // B <- D
                    RegisterToRegisterOp(ref Processor.B, Processor.D);
                    break;
                case 0x43:  // B <- E
                    RegisterToRegisterOp(ref Processor.B, Processor.E);
                    break;
                case 0x44:  // B <- H
                    RegisterToRegisterOp(ref Processor.B, Processor.H);
                    break;
                case 0x45:  // B <- L
                    RegisterToRegisterOp(ref Processor.B, Processor.L);
                    break;
                case 0x4F:  // C <- A
                    RegisterToRegisterOp(ref Processor.C, Processor.A);
                    break;
                case 0x48:  // C <- B
                    RegisterToRegisterOp(ref Processor.C, Processor.B);
                    break;
                case 0x49:  // C <- C
                    Processor.ClockCycles += 4;
                    break;
                case 0x4A:  // C <- D
                    RegisterToRegisterOp(ref Processor.C, Processor.D);
                    break;
                case 0x4B:  // C <- E
                    RegisterToRegisterOp(ref Processor.C, Processor.E);
                    break;
                case 0x4C:  // C <- H
                    RegisterToRegisterOp(ref Processor.C, Processor.H);
                    break;
                case 0x4D:  // C <- L
                    RegisterToRegisterOp(ref Processor.C, Processor.L);
                    break;
                case 0x57:  // D <- A
                    RegisterToRegisterOp(ref Processor.D, Processor.A);
                    break;
                case 0x50:  // D <- B
                    RegisterToRegisterOp(ref Processor.D, Processor.B);
                    break;
                case 0x51:  // D <- C
                    RegisterToRegisterOp(ref Processor.D, Processor.C);
                    break;
                case 0x52:  // D <- D
                    Processor.ClockCycles += 4;
                    break;
                case 0x53:  // D <- E
                    RegisterToRegisterOp(ref Processor.D, Processor.E);
                    break;
                case 0x54:  // D <- H
                    RegisterToRegisterOp(ref Processor.D, Processor.H);
                    break;
                case 0x55:  // D <- L
                    RegisterToRegisterOp(ref Processor.D, Processor.L);
                    break;
                case 0x5F:  // E <- A
                    RegisterToRegisterOp(ref Processor.E, Processor.A);
                    break;
                case 0x58:  // E <- B
                    RegisterToRegisterOp(ref Processor.E, Processor.B);
                    break;
                case 0x59:  // E <- C
                    RegisterToRegisterOp(ref Processor.E, Processor.C);
                    break;
                case 0x5A:  // E <- D
                    RegisterToRegisterOp(ref Processor.E, Processor.D);
                    break;
                case 0x5B:  // E <- E
                    Processor.ClockCycles += 4;
                    break;
                case 0x5C:  // E <- H
                    RegisterToRegisterOp(ref Processor.E, Processor.H);
                    break;
                case 0x5D:  // E <- L
                    RegisterToRegisterOp(ref Processor.E, Processor.L);
                    break;
                case 0x67:  // H <- A
                    RegisterToRegisterOp(ref Processor.H, Processor.A);
                    break;
                case 0x60:  // H <- B
                    RegisterToRegisterOp(ref Processor.H, Processor.B);
                    break;
                case 0x61:  // H <- C
                    RegisterToRegisterOp(ref Processor.H, Processor.C);
                    break;
                case 0x62:  // H <- D
                    RegisterToRegisterOp(ref Processor.H, Processor.D);
                    break;
                case 0x63:  // H <- E
                    RegisterToRegisterOp(ref Processor.H, Processor.E);
                    break;
                case 0x64:  // H <- H
                    Processor.ClockCycles += 4;
                    break;
                case 0x65:  // H <- L
                    RegisterToRegisterOp(ref Processor.H, Processor.L);
                    break;
                case 0x6F:  // L <- A
                    RegisterToRegisterOp(ref Processor.L, Processor.A);
                    break;
                case 0x68:  // L <- B
                    RegisterToRegisterOp(ref Processor.L, Processor.B);
                    break;
                case 0x69:  // L <- C
                    RegisterToRegisterOp(ref Processor.L, Processor.C);
                    break;
                case 0x6A:  // L <- D
                    RegisterToRegisterOp(ref Processor.L, Processor.D);
                    break;
                case 0x6B:  // L <- E
                    RegisterToRegisterOp(ref Processor.L, Processor.E);
                    break;
                case 0x6C:  // L <- H
                    RegisterToRegisterOp(ref Processor.L, Processor.H);
                    break;
                case 0x6D:  // L <- L
                    Processor.ClockCycles += 4;
                    break;

                case 0xF9:  // SP <- HL
                    RegisterToRegister16BitOp(ref Processor.SP, Processor.HL);
                    break;

                case 0xF8: // HL <- SP + signed immediate
                    tempWord = Processor.HL;
                    RegisterToRegister16BitOp(ref Processor.SP, (ushort)(Processor.SP + (sbyte)(Memory.Data[Processor.PC])));
                    Processor.SetFlags(0, 0, ((tempWord & 0xfff) + (Processor.HL & 0xfff) & 0x1000) == 0x1000 ? 1 : 0, 
                        Processor.SP + (sbyte)(Memory.Data[Processor.PC]) > 0xFFFF ? 1 : 0);
                    Processor.PC++; //extra increment for the immediate
                    Processor.ClockCycles += 4; //also more clock cycles
                    break;

                #endregion

                #region Stack Instructions

                case 0xF5:  // PUSH AF
                    PushOp(Processor.F);
                    PushOp(Processor.A);
                    break;
                case 0xC5:  // PUSH BC
                    PushOp(Processor.C);
                    PushOp(Processor.B);
                    break;
                case 0xD5:  // PUSH DE
                    PushOp(Processor.E);
                    PushOp(Processor.D);
                    break;
                case 0xE5:  // PUSH HL
                    PushOp(Processor.L);
                    PushOp(Processor.H);
                    break;

                // POP
                case 0xF1:  // POP AF
                    Processor.A = PopOp();
                    Processor.F = PopOp();
                    Processor.ClockCycles += 12;
                    break;
                case 0xC1:  // POP BC
                    Processor.B = PopOp();
                    Processor.C = PopOp();
                    break;
                case 0xD1:  // POP DE
                    Processor.D = PopOp();
                    Processor.E = PopOp();
                    break;
                case 0xE1:  // POP HL
                    Processor.H = PopOp();
                    Processor.L = PopOp();
                    break;
                #endregion

                #region Add Instructions
                case 0x87: //A += A
                    AddOp(Processor.A);
                    break;
                case 0x80: //A += B
                    AddOp(Processor.B);
                    break;
                case 0x81: //A += C
                    AddOp(Processor.C);
                    break;
                case 0x82: //A += D
                    AddOp(Processor.D);
                    break;
                case 0x83: //A += E
                    AddOp(Processor.E);
                    break;
                case 0x84: //A += H
                    AddOp(Processor.H);
                    break;
                case 0x85: //A += L
                    AddOp(Processor.L);
                    break;
                case 0x86: // A+= (HL)
                    AddOp(Memory.ReadByte(Processor.HL));
                    Processor.ClockCycles += 4; //for the read op
                    break;
                case 0xC6: // A += immediate
                    AddOp(Memory.Data[Processor.PC]);
                    Processor.PC++; //extra increment
                    Processor.ClockCycles += 4; //read op
                    break;

                // ADC
                case 0x8F: //A += A+carry
                    AdcOp(Processor.A);
                    break;
                case 0x88://A+=B+carry
                    AdcOp(Processor.B);
                    break;
                case 0x89://A+=C+carry
                    AdcOp(Processor.C);
                    break;
                case 0x8A://A+=D+carry
                    AdcOp(Processor.D);
                    break;
                case 0x8B://A+=E+carry
                    AdcOp(Processor.E);
                    break;
                case 0x8C://A+=H+carry
                    AdcOp(Processor.H);
                    break;
                case 0x8D://A+=L+carry
                    AdcOp(Processor.L);
                    break;
                case 0x8E://A+=(HL)+carry
                    AdcOp(Memory.ReadByte(Processor.HL));
                    Processor.ClockCycles += 4; //for the read
                    break;
                case 0xCE://A+=immediate+carry
                    AdcOp(Memory.Data[Processor.PC]);
                    Processor.PC++; //for the immediate
                    Processor.ClockCycles += 4; //for the read
                    break;

                // 16-bit arithmetics

                // ADD
                case 0x09: //HL += BC
                    Add16BitOp((ushort)(Processor.B << 8 | Processor.C));
                    break;
                case 0x19: //HL += DE
                    Add16BitOp((ushort)(Processor.D << 8 | Processor.E));
                    Processor.ClockCycles += 8;
                    break;
                case 0x29: //HL += HL
                    Add16BitOp(Processor.HL);
                    break;
                case 0x39: //HL += SP
                    Add16BitOp(Processor.SP);
                    break;
                case 0xE8:  // SP += signed immediate byte                              
                    tempWord = Processor.SP;
                    Processor.SP = (ushort)(Processor.SP + ((sbyte)Memory.Data[Processor.PC]));
                    Processor.SetFlags(0, 0, ((tempWord & 0xfff) + (Processor.HL & 0xfff) & 0x1000) == 0x1000 ? 1 : 0, 
                        Processor.SP + (sbyte)(Memory.Data[Processor.PC]) > 0xFFFF ? 1 : 0);
                    Processor.PC++; //for the immediate
                    Processor.ClockCycles += 16;
                    break;
                #endregion

                #region Sub Instructions
                // SUB
                case 0x97://A-=A
                    SubOp(Processor.A);
                    break;
                case 0x90://A-=B
                    SubOp(Processor.B);
                    break;
                case 0x91://A-=C
                    SubOp(Processor.C);
                    break;
                case 0x92://A-=D
                    SubOp(Processor.D);
                    break;
                case 0x93://A-=E
                    SubOp(Processor.E);
                    break;
                case 0x94: //A -=H
                    SubOp(Processor.H);
                    break;
                case 0x95: //A-=L
                    SubOp(Processor.L);
                    break;
                case 0x96: //A-=(HL)
                    SubOp(Memory.ReadByte(Processor.HL));
                    Processor.ClockCycles += 4; //extra cycles for the read
                    break;
                case 0xD6://A-=(immediate)
                    SubOp(Memory.Data[Processor.PC]);
                    Processor.PC++; //extra for the read
                    Processor.ClockCycles += 4; //extra for the read
                    break;

                // SBC
                case 0x9F: //A-=A+carry
                    SbcOp(Processor.A);
                    break;
                case 0x98: //.etc
                    SbcOp(Processor.B);
                    break;
                case 0x99:
                    SbcOp(Processor.C);
                    break;
                case 0x9A:
                    SbcOp(Processor.D);
                    break;
                case 0x9B:
                    SbcOp(Processor.E);
                    break;
                case 0x9C:
                    SbcOp(Processor.H);
                    break;
                case 0x9D:
                    SbcOp(Processor.L);
                    break;
                case 0x9E:
                    SbcOp(Memory.ReadByte(Processor.HL));
                    Processor.ClockCycles += 4; //extra cycles
                    break;
                // sbc + immediate non-existent?
                #endregion

                #region Inc/Dec Instructions

                // INC
                case 0x3C: //A++
                    Processor.A = IncOp(Processor.A);
                    break;
                case 0x04: //B++
                    Processor.B = IncOp(Processor.B);
                    break;
                case 0x0C: //C++
                    Processor.C = IncOp(Processor.C);
                    break;
                case 0x14: //D++
                    Processor.D = IncOp(Processor.D);
                    break;
                case 0x1C: //E++
                    Processor.E = IncOp(Processor.E);
                    break;
                case 0x24: //H++
                    Processor.H = IncOp(Processor.H);
                    break;
                case 0x2C: //L++
                    Processor.L = IncOp(Processor.L);
                    break;
                case 0x34: //(HL)++
                    Memory.WriteByte(Processor.HL, IncOp(Memory.ReadByte(Processor.HL)));
                    Processor.ClockCycles += 8; //extra cycles
                    break;

                // DEC
                case 0x3D: //A--
                    Processor.A = DecOp(Processor.A);
                    break;
                case 0x05: //B--
                    Processor.B = DecOp(Processor.B);
                    break;
                case 0x0D: //C--
                    Processor.C = DecOp(Processor.C);
                    break;
                case 0x15: //D--
                    Processor.D = DecOp(Processor.D);
                    break;
                case 0x1D: //E--
                    Processor.E = DecOp(Processor.E);
                    break;
                case 0x25: //H--
                    Processor.H = DecOp(Processor.H);
                    break;
                case 0x2D: //L--
                    Processor.L = DecOp(Processor.L);
                    break;
                case 0x35: //(HL)--
                    Memory.WriteByte(Processor.HL, DecOp(Memory.ReadByte(Processor.HL)));
                    Processor.ClockCycles += 8; //extra cycles
                    break;

                    // INC 16bit
                case 0x03:  // BC++
                    tempWord = Inc16BitOp((ushort)(Processor.B << 8 | Processor.C));
                    Processor.B = (byte)(tempWord >> 8);
                    Processor.C = (byte)tempWord;
                    break;
                case 0x13:  // DE++
                    tempWord = Inc16BitOp((ushort)(Processor.D << 8 | Processor.E));
                    Processor.D = (byte)(tempWord >> 8);
                    Processor.E = (byte)tempWord;
                    break;
                case 0x23:  // HL++
                    Processor.HL = Inc16BitOp(Processor.HL);
                    break;
                case 0x33:  // SP++
                    Processor.SP = Inc16BitOp(Processor.SP);
                    break;

                // DEC 16bit
                case 0x0B:  // BC--
                    tempWord = Dec16BitOp((ushort)(Processor.B << 8 | Processor.C));
                    Processor.B = (byte)(tempWord >> 8);
                    Processor.C = (byte)tempWord;
                    break;
                case 0x1B:  // DE--
                    tempWord = Dec16BitOp((ushort)(Processor.D << 8 | Processor.E));
                    Processor.D = (byte)(tempWord >> 8);
                    Processor.E = (byte)tempWord;
                    break;
                case 0x2B:  // HL--
                    Processor.HL = Dec16BitOp(Processor.HL);
                    break;
                case 0x3B:  // SP--
                    Processor.SP = Dec16BitOp(Processor.SP);
                    break;
#endregion

                #region Compare Instructions

                case 0xBF:
                    CompareOp(Processor.A);
                    break;
                case 0xB8:
                    CompareOp(Processor.B);
                    break;
                case 0xB9:
                    CompareOp(Processor.C);
                    break;
                case 0xBA:
                    CompareOp(Processor.D);
                    break;
                case 0xBB:
                    CompareOp(Processor.E);
                    break;
                case 0xBC:
                    CompareOp(Processor.H);
                    break;
                case 0xBD:
                    CompareOp(Processor.L);
                    break;
                case 0xBE:
                    CompareOp(Memory.ReadByte(Processor.HL));
                    Processor.ClockCycles += 4; //extra for the read
                    break;
                case 0xFE:
                    CompareOp(Memory.Data[Processor.PC]);
                    Processor.PC++;//extra for the read
                    Processor.ClockCycles += 4;
                    break;

#endregion
                
                #region  Jump instructions 
                // absolute jumps
                case 0xC3:  // Unconditional + 2B immediate operands
                    JumpFarOp();
                    break;
                case 0xC2:  // Conditional NZ + 2B immediate operands
                    ConditionalJumpFarOp(() => Processor.ZeroFlag == 0);
                    break;
                case 0xCA:  // Conditional Z + 2B immediate operands
                    ConditionalJumpFarOp(() => Processor.ZeroFlag != 0);
                    break;
                case 0xD2:  // Conditional NC + 2B immediate operands
                    ConditionalJumpFarOp(() => Processor.CarryFlag == 0);
                    break;
                case 0xDA:  // Conditional C + 2B immediate operands
                    ConditionalJumpFarOp(() => Processor.CarryFlag != 0);
                    break;
                case 0xE9:  // Unconditional jump to HL
                    Processor.PC = Processor.HL;
                    Processor.ClockCycles += 4;
                    break;

                // relative jumps
                case 0x18:  // Unconditional + relative byte
                    JumpNearOp();
                    break;
                case 0x20:  // Conditional NZ + relative byte
                    ConditionalJumpNearOp(() => Processor.ZeroFlag == 0);
                    break;
                case 0x28:  // Conditional Z + relative byte
                    ConditionalJumpNearOp(() => Processor.ZeroFlag != 0);
                    break;
                case 0x30:  // Conditional NC + relative byte
                    ConditionalJumpNearOp(() => Processor.CarryFlag == 0);
                    break;
                case 0x38:  // Conditional C + relative byte
                    ConditionalJumpNearOp(() => Processor.ZeroFlag != 0);
                    break;
#endregion

                #region Call Instructions

                // calls
                case 0xCD:  // unconditional
                    CallOp();
                    break;
                case 0xC4:  // Conditional NZ
                    ConditionalCallOp(() => Processor.ZeroFlag == 0);
                    break;
                case 0xCC:  // Conditional Z
                    ConditionalCallOp(() => Processor.ZeroFlag != 0);
                    break;
                case 0xD4:  // Conditional NC
                    ConditionalCallOp(() => Processor.CarryFlag == 0);
                    break;
                case 0xDC:  // Conditional C
                    ConditionalCallOp(() => Processor.CarryFlag != 0);
                    break;
#endregion

                #region Reset Instructions
                case 0xC7:
                    ResetOp(0x00);
                    break;
                case 0xCF:
                    ResetOp(0x08);
                    break;
                case 0xD7:
                    ResetOp(0x10);
                    break;
                case 0xDF:
                    ResetOp(0x18);
                    break;
                case 0xE7:
                    ResetOp(0x20);
                    break;
                case 0xEF:
                    ResetOp(0x28);
                    break;
                case 0xF7:
                    ResetOp(0x30);
                    break;
                case 0xFF:
                    ResetOp(0x38);
                    break;
#endregion

                #region Return Instructions

                // returns
                case 0xC9:  // unconditional
                    ReturnOp();
                    break;
                case 0xD9:  // unconditional plus enable interrupts (RETI)
                    ReturnOp();
                    Processor.EnableInterruptSignal = 1;
                    break;
                case 0xC0:  // Conditional NZ
                    ConditionalReturnOp(() => Processor.ZeroFlag == 0);
                    break;
                case 0xC8:  // Conditional Z
                    ConditionalReturnOp(() => Processor.ZeroFlag != 0);
                    Processor.ClockCycles += 8;
                    break;
                case 0xD0:  // Conditional NC
                    ConditionalReturnOp(() => Processor.CarryFlag == 0);
                    break;
                case 0xD8:  // Conditional C
                    ConditionalReturnOp(() => Processor.CarryFlag != 0);
                    break;

                #endregion

                    
                #region  Logical instructions 

                // OR
                case 0xB7:  // A = (A OR A) = A
                    OrOp(ref Processor.A, Processor.A);
                    break;
                case 0xB0:  // A = A OR B
                    OrOp(ref Processor.A, Processor.B);
                    break;
                case 0xB1:  // A = A OR C
                    OrOp(ref Processor.A, Processor.C);
                    break;
                case 0xB2:  // A = A OR D
                    OrOp(ref Processor.A, Processor.D);
                    break;
                case 0xB3:  // A = A OR E
                    OrOp(ref Processor.A, Processor.E);
                    break;
                case 0xB4:  // A = A OR H
                    OrOp(ref Processor.A, Processor.H);
                    break;
                case 0xB5:  // A = A OR L
                    OrOp(ref Processor.A, Processor.L);
                    break;
                case 0xB6:  // A = A OR (HL)
                    OrOp(ref Processor.A, Memory.ReadByte(Processor.HL));
                    Processor.ClockCycles += 4; //extra cycles
                    break;
                case 0xF6:  // A = A OR immediate
                    OrOp(ref Processor.A, Memory.Data[Processor.PC]);
                    Processor.PC++; //extra increment for the immediate
                    Processor.ClockCycles += 4; //extra clock cycles
                    break;

                // XOR
                case 0xAF:  // A = A XOR A = 0 !!!
                    XorOp(ref Processor.A, Processor.A);
                    break;
                case 0xA8:  // A = A XOR B
                    XorOp(ref Processor.A, Processor.B);
                    break;
                case 0xA9:  // A = A XOR C
                    XorOp(ref Processor.A, Processor.C);
                    break;
                case 0xAA:  // A = A XOR D
                    XorOp(ref Processor.A, Processor.D);
                    break;
                case 0xAB:  // A = A XOR E
                    XorOp(ref Processor.A, Processor.E);
                    break;
                case 0xAC:  // A = A XOR H
                    XorOp(ref Processor.A, Processor.H);
                    break;
                case 0xAD:  // A = A XOR L
                    XorOp(ref Processor.A, Processor.L);
                    break;
                case 0xAE:  // A = A XOR (HL)
                    XorOp(ref Processor.A, Memory.ReadByte(Processor.HL));
                    Processor.ClockCycles += 4; //extra cycles
                    break;
                case 0xEE:  // A = A XOR immediate
                    XorOp(ref Processor.A, Memory.Data[Processor.PC]);
                    Processor.PC++; //extra op stuff
                    Processor.ClockCycles += 4;
                    break;

                // AND
                    //TODO: does it really halfcarry?
                case 0xA7:  // A = A AND A
                    AndOp(ref Processor.A, Processor.A);
                    break;
                case 0xA0:  // A = A AND B
                    AndOp(ref Processor.A, Processor.B);
                    break;
                case 0xA1:  // A = A AND C
                    AndOp(ref Processor.A, Processor.C);
                    break;
                case 0xA2:  // A = A AND D
                    AndOp(ref Processor.A, Processor.D);
                    break;
                case 0xA3:  // A = A AND E
                    AndOp(ref Processor.A, Processor.E);
                    break;
                case 0xA4:  // A = A AND H
                    AndOp(ref Processor.A, Processor.H);
                    break;
                case 0xA5:  // A = A AND L
                    AndOp(ref Processor.A, Processor.L);
                    break;
                case 0xA6:  // A = A AND (HL)
                    AndOp(ref Processor.A, Memory.ReadByte(Processor.HL));
                    Processor.ClockCycles += 4;
                    break;
                case 0xE6:  // A = A AND immediate
                    AndOp(ref Processor.A, Memory.Data[Processor.PC]);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;

                #endregion

                #region  Rotate Instructions 

                case 0x07:  // Rotate A left
                    Processor.A = RlcOp(Processor.A);
                    break;
                case 0x17:  // Rotate A left with carry
                    Processor.A = RlOp(Processor.A);
                    break;
                case 0x0F:  // Rotate A right
                    Processor.A = RrcOp(Processor.A);
                    break;
                case 0x1F:  // Rotate A right with carry
                    Processor.A = RrOp(Processor.A); //NOTE: removed PC from rotate right.
                    break;
#endregion

                #region The Huge CB Instruction

                case 0xCB:  // Big Operation! includes rotations, shifts, swaps, set etc.
                    // check the operand to identify real operation
                    Processor.ClockCycles += 4; //needed because there's an extra read op
                    /* Operations under the CB opcode
                     * 0x - RLC and RRC (done x2)
                     * 1x - RL and RR
                     * 2x - SLA and SRA
                     * 3x - swap (done) and SRL
                     * 4x - bit 0 and 1
                     * 5x - bit 2 and 3
                     * 6x - bit 4 and 5
                     * 7x - bit 6 and 7
                     * 8x - res 0 and 1
                     * 9x - res 2 and 3
                     * Ax - res 4 and 5
                     * Bx - res 6 and 7
                     * Cx - set 0 and 1 (done x2)
                     * Dx - set 2 and 3 (done x2)
                     * Ex - set 4 and 5 (done x2)
                     * Fx - set 6 and 7 (done x2)
                     * Swap - done
                     * Setbit - done
                     * rotate - done
                     */
                    switch (Memory.Data[Processor.PC])
                    {
                        // ROTATIONS, 0x
                        case 0x07:  // Rotate A left
                            Processor.A = RlcOp(Processor.A);
                            break;
                        case 0x00:  // Rotate B left
                            Processor.B = RlcOp(Processor.B);
                            break;
                        case 0x01:  // Rotate C left
                            Processor.C = RlcOp(Processor.C);
                            break;
                        case 0x02:  // Rotate D left
                            Processor.D = RlcOp(Processor.D);
                            break;
                        case 0x03:  // Rotate E left
                            Processor.E = RlcOp(Processor.E);
                            break;
                        case 0x04:  // Rotate H left
                            Processor.H = RlcOp(Processor.H);
                            break;
                        case 0x05:  // Rotate L left
                            Processor.L = RlcOp(Processor.L);
                            break;
                        case 0x06:  // Rotate (HL) left
                            Memory.WriteByte(Processor.HL, RlcOp(Memory.ReadByte(Processor.HL)));
                            Processor.ClockCycles += 8; //extra cycles for the read
                            break;
                        case 0x0F:  // Rotate A right
                            Processor.A = RrcOp(Processor.A);
                            break;
                        case 0x08:  // Rotate B right
                            Processor.B = RrcOp(Processor.B);
                            break;
                        case 0x09:  // Rotate C right
                            Processor.C = RrcOp(Processor.C);
                            break;
                        case 0x0A:  // Rotate D right
                            Processor.D = RrcOp(Processor.D);
                            break;
                        case 0x0B:  // Rotate E right
                            Processor.E = RrcOp(Processor.E);
                            break;
                        case 0x0C:  // Rotate H right
                            Processor.H = RrcOp(Processor.H);
                            break;
                        case 0x0D:  // Rotate L right
                            Processor.L = RrcOp(Processor.L);
                            break;
                        case 0x0E:  // Rotate (HL) right
                            Memory.WriteByte(Processor.HL, RrcOp(Memory.ReadByte(Processor.HL)));
                            Processor.ClockCycles += 8; //extra cycles for the read
                            break;
                        //rotations with carry or without carry or whatever, 1x
                        case 0x17:  // Rotate A left
                            Processor.A = RlOp(Processor.A);
                            break;
                        case 0x10:  // Rotate B left
                            Processor.B = RlOp(Processor.B);
                            break;
                        case 0x11:  // Rotate C left
                            Processor.C = RlOp(Processor.C);
                            break;
                        case 0x12:  // Rotate D left
                            Processor.D = RlOp(Processor.D);
                            break;
                        case 0x13:  // Rotate E left
                            Processor.E = RlOp(Processor.E);
                            break;
                        case 0x14:  // Rotate H left
                            Processor.H = RlOp(Processor.H);
                            break;
                        case 0x15:  // Rotate L left
                            Processor.L = RlOp(Processor.L);
                            break;
                        case 0x16:  // Rotate (HL) left
                            Memory.WriteByte(Processor.HL, RlOp(Memory.ReadByte(Processor.HL)));
                            Processor.ClockCycles += 8; //extra cycles for the read
                            break;
                        case 0x1F:  // Rotate A right
                            Processor.A = RrOp(Processor.A);
                            break;
                        case 0x18:  // Rotate B right
                            Processor.B = RrOp(Processor.B);
                            break;
                        case 0x19:  // Rotate C right
                            Processor.C = RrOp(Processor.C);
                            break;
                        case 0x1A:  // Rotate D right
                            Processor.D = RrOp(Processor.D);
                            break;
                        case 0x1B:  // Rotate E right
                            Processor.E = RrOp(Processor.E);
                            break;
                        case 0x1C:  // Rotate H right
                            Processor.H = RrOp(Processor.H);
                            break;
                        case 0x1D:  // Rotate L right
                            Processor.L = RrOp(Processor.L);
                            break;
                        case 0x1E:  // Rotate (HL) right
                            Memory.WriteByte(Processor.HL, RrOp(Memory.ReadByte(Processor.HL)));
                            Processor.ClockCycles += 8; //extra cycles for the read
                            break;
                        //SLA and SRA, 2x
                        //swap and SRL (lolwut), 3x
                        case 0x37:  // SWAP A
                            Processor.A = SwapOp(Processor.A);
                            break;
                        case 0x30:  // SWAP B
                            Processor.B = SwapOp(Processor.B);
                            break;
                        case 0x31:  // SWAP C
                            Processor.C = SwapOp(Processor.C);
                            break;
                        case 0x32:  // SWAP D
                            Processor.D = SwapOp(Processor.D);
                            break;
                        case 0x33:  // SWAP E
                            Processor.E = SwapOp(Processor.E);
                            break;
                        case 0x34:  // SWAP H
                            Processor.H = SwapOp(Processor.H);
                            break;
                        case 0x35:  // SWAP L
                            Processor.L = SwapOp(Processor.L);
                            break;
                        case 0x36:  // SWAP (HL)
                            Memory.WriteByte(Processor.HL, SwapOp(Memory.ReadByte(Processor.HL)));
                            Processor.ClockCycles += 8; //extra cycles for the read+write
                            break;
                        case 0x3F://srl a
                            Processor.A = SrlOp(Processor.A);
                            break;
                        case 0x38://srl b
                            Processor.B = SrlOp(Processor.B);
                            break;
                        case 0x39://srl c
                            Processor.C = SrlOp(Processor.C);
                            break;
                        case 0x3A://srl d
                            Processor.D = SrlOp(Processor.D);
                            break;
                        case 0x3B://srl e
                            Processor.E = SrlOp(Processor.E);
                            break;
                        case 0x3C://srl h
                            Processor.H = SrlOp(Processor.H);
                            break;
                        case 0x3D://srl l
                            Processor.L = SrlOp(Processor.L);
                            break;
                        case 0x3E://srl (HL)
                            Memory.WriteByte(Processor.HL, SrlOp(Memory.ReadByte(Processor.HL)));
                            Processor.ClockCycles += 8; //extra cycles for the read+write
                            break;


                        //Testbit 0-1, 4x
                        //testbit 2-3, 5x
                        //testbit 4-5, 6x
                        //testbit 6-7, 7x
                        //reset 0-1, 8x
                        //reset 2-3, 9x
                        //reset 4-5, Ax
                        //reset 6-7, Bx
                        #region Setbit
                        //set 0-1, Cx
                        case 0xC7:  // Set 0, A
                            Processor.A = SetBitOp(0, Processor.A);
                            break;
                        case 0xC0:  // Set 0, B
                            Processor.B = SetBitOp(0, Processor.B);
                            break;
                        case 0xC1:  // Set 0, C
                            Processor.C = SetBitOp(0, Processor.C);
                            break;
                        case 0xC2:  // Set 0, D
                            Processor.D = SetBitOp(0, Processor.D);
                            break;
                        case 0xC3:  // Set 0, E
                            Processor.E = SetBitOp(0, Processor.E);
                            break;
                        case 0xC4:  // Set 0, H
                            Processor.H = SetBitOp(0, Processor.H);
                            break;
                        case 0xC5:  // Set 0, L
                            Processor.L = SetBitOp(0, Processor.L);
                            break;
                        case 0xC6:  // Set 0, (HL)
                            Memory.WriteByte(Processor.HL, SetBitOp(0, Memory.ReadByte(Processor.HL)));
                            Processor.ClockCycles += 8;
                            break;                        
                        case 0xCF:  // Set 1, A
                            Processor.A = SetBitOp(1, Processor.A);
                            break;
                        case 0xC8:  // Set 1, B
                            Processor.B = SetBitOp(1, Processor.B);
                            break;
                        case 0xC9:  // Set 1, C
                            Processor.C = SetBitOp(1, Processor.C);
                            break;
                        case 0xCA:  // Set 1, D
                            Processor.D = SetBitOp(1, Processor.D);
                            break;
                        case 0xCB:  // Set 1, E
                            Processor.E = SetBitOp(1, Processor.E);
                            break;
                        case 0xCC:  // Set 1, H
                            Processor.H = SetBitOp(1, Processor.H);
                            break;
                        case 0xCD:  // Set 1, L
                            Processor.L = SetBitOp(1, Processor.L);
                            break;
                        case 0xCE:  // Set 1, (HL)
                            Memory.WriteByte(Processor.HL, SetBitOp(1, Memory.ReadByte(Processor.HL)));
                            Processor.ClockCycles += 8;
                            break;

                        //set 2-3, Dx
                        case 0xD7:  // Set 2, A
                            Processor.A = SetBitOp(2, Processor.A);
                            break;
                        case 0xD0:  // Set 2, B
                            Processor.B = SetBitOp(2, Processor.B);
                            break;
                        case 0xD1:  // Set 2, C
                            Processor.C = SetBitOp(2, Processor.C);
                            break;
                        case 0xD2:  // Set 2, D
                            Processor.D = SetBitOp(2, Processor.D);
                            break;
                        case 0xD3:  // Set 2, E
                            Processor.E = SetBitOp(2, Processor.E);
                            break;
                        case 0xD4:  // Set 2, H
                            Processor.H = SetBitOp(2, Processor.H);
                            break;
                        case 0xD5:  // Set 2, L
                            Processor.L = SetBitOp(2, Processor.L);
                            break;
                        case 0xD6:  // Set 2, (HL)
                            Memory.WriteByte(Processor.HL, SetBitOp(2, Memory.ReadByte(Processor.HL)));
                            Processor.ClockCycles += 8;
                            break;
                        case 0xDF:  // Set 3, A
                            Processor.A = SetBitOp(3, Processor.A);
                            break;
                        case 0xD8:  // Set 3, B
                            Processor.B = SetBitOp(3, Processor.B);
                            break;
                        case 0xD9:  // Set 3, C
                            Processor.C = SetBitOp(3, Processor.C);
                            break;
                        case 0xDA:  // Set 3, D
                            Processor.D = SetBitOp(3, Processor.D);
                            break;
                        case 0xDB:  // Set 3, E
                            Processor.E = SetBitOp(3, Processor.E);
                            break;
                        case 0xDC:  // Set 3, H
                            Processor.H = SetBitOp(3, Processor.H);
                            break;
                        case 0xDD:  // Set 3, L
                            Processor.L = SetBitOp(3, Processor.L);
                            break;
                        case 0xDE:  // Set 3, (HL)
                            Memory.WriteByte(Processor.HL, SetBitOp(3, Memory.ReadByte(Processor.HL)));
                            Processor.ClockCycles += 8;
                            break;

                        //set 4-5, Ex
                        case 0xE7:  // Set 4, A
                            Processor.A = SetBitOp(4, Processor.A);
                            break;
                        case 0xE0:  // Set 4, B
                            Processor.B = SetBitOp(4, Processor.B);
                            break;
                        case 0xE1:  // Set 4, C
                            Processor.C = SetBitOp(4, Processor.C);
                            break;
                        case 0xE2:  // Set 4, D
                            Processor.D = SetBitOp(4, Processor.D);
                            break;
                        case 0xE3:  // Set 4, E
                            Processor.E = SetBitOp(4, Processor.E);
                            break;
                        case 0xE4:  // Set 4, H
                            Processor.H = SetBitOp(4, Processor.H);
                            break;
                        case 0xE5:  // Set 4, L
                            Processor.L = SetBitOp(4, Processor.L);
                            break;
                        case 0xE6:  // Set 4, (HL)
                            Memory.WriteByte(Processor.HL, SetBitOp(4, Memory.ReadByte(Processor.HL)));
                            Processor.ClockCycles += 8;
                            break;
                        case 0xEF:  // Set 5, A
                            Processor.A = SetBitOp(5, Processor.A);
                            break;
                        case 0xE8:  // Set 5, B
                            Processor.B = SetBitOp(5, Processor.B);
                            break;
                        case 0xE9:  // Set 5, C
                            Processor.C = SetBitOp(5, Processor.C);
                            break;
                        case 0xEA:  // Set 5, D
                            Processor.D = SetBitOp(5, Processor.D);
                            break;
                        case 0xEB:  // Set 5, E
                            Processor.E = SetBitOp(5, Processor.E);
                            break;
                        case 0xEC:  // Set 5, H
                            Processor.H = SetBitOp(5, Processor.H);
                            break;
                        case 0xED:  // Set 5, L
                            Processor.L = SetBitOp(5, Processor.L);
                            break;
                        case 0xEE:  // Set 5, (HL)
                            Memory.WriteByte(Processor.HL, SetBitOp(5, Memory.ReadByte(Processor.HL)));
                            Processor.ClockCycles += 8;
                            break;

                        //set 6-7, Fx
                        case 0xF7:  // Set 6, A
                            Processor.A = SetBitOp(6, Processor.A);
                            break;
                        case 0xF0:  // Set 6, B
                            Processor.B = SetBitOp(6, Processor.B);
                            break;
                        case 0xF1:  // Set 6, C
                            Processor.C = SetBitOp(6, Processor.C);
                            break;
                        case 0xF2:  // Set 6, D
                            Processor.D = SetBitOp(6, Processor.D);
                            break;
                        case 0xF3:  // Set 6, E
                            Processor.E = SetBitOp(6, Processor.E);
                            break;
                        case 0xF4:  // Set 6, H
                            Processor.H = SetBitOp(6, Processor.H);
                            break;
                        case 0xF5:  // Set 6, L
                            Processor.L = SetBitOp(6, Processor.L);
                            break;
                        case 0xF6:  // Set 6, (HL)
                            Memory.WriteByte(Processor.HL, SetBitOp(6, Memory.ReadByte(Processor.HL)));
                            Processor.ClockCycles += 8;
                            break;                                                
                        case 0xFF:  // Set 7, A
                            Processor.A = SetBitOp(7, Processor.A);
                            break;
                        case 0xF8:  // Set 7, B
                            Processor.B = SetBitOp(7, Processor.B);
                            break;
                        case 0xF9:  // Set 7, C
                            Processor.C = SetBitOp(7, Processor.C);
                            break;
                        case 0xFA:  // Set 7, D
                            Processor.D = SetBitOp(7, Processor.D);
                            break;
                        case 0xFB:  // Set 7, E
                            Processor.E = SetBitOp(7, Processor.E);
                            break;
                        case 0xFC:  // Set 7, H
                            Processor.H = SetBitOp(7, Processor.H);
                            break;
                        case 0xFD:  // Set 7, L
                            Processor.L = SetBitOp(7, Processor.L);
                            break;
                        case 0xFE:  // Set 7, (HL)
                            Memory.WriteByte(Processor.HL, SetBitOp(7, Memory.ReadByte(Processor.HL)));
                            Processor.ClockCycles += 8;
                            break;
                        #endregion

                        default: //hopefully we never hit here..because it's a full 256 opcode instr o_0
                            err = true;
                            Console.WriteLine("{0:X2}", Memory.Data[Processor.PC]);
                            Processor.ClockCycles += 0;
                            break;
                    }
                    Processor.PC++;
                    break;
                #endregion

                #region Interrupt and Other Various Instructions
                case 0x2F:  // Complement A
                    Processor.A = (byte)~Processor.A;
                    Processor.SetFlags(Processor.ZeroFlag, 1, 1, Processor.CarryFlag);
                    Processor.ClockCycles += 4;
                    break;

                case 0x3F:  // Complement Carry TODO: check up the flags to set on these
                    Processor.SetFlags(Processor.ZeroFlag, 0, 0, Processor.CarryFlag ^ 1);
                    Processor.ClockCycles += 4;
                    break;

                case 0x37:  // Set Carry
                    Processor.SetFlags(Processor.ZeroFlag, 0, 0, 1);
                    Processor.ClockCycles += 4;
                    break;

                case 0x00:  // NOP
                    Processor.ClockCycles += 4; //we don't really need to move out NOP to a macro op; it's nop..
                    break;

                /*case 0x76:  // HALT TODO with interrupts
                    if (Processor.IME) Processor.CPUHalt = true;
                    else Processor.SkipPCCounting = true;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;*/

                case 0x10:  // STOP
                    if (Memory.Data[Processor.PC] != 0)   // check if next operand is 0 (has to be!)
                    {
                        err = true;
                        Processor.ClockCycles += 0;
                        break;
                    }
                    else
                    {
                        Processor.Stop = true;
                        Processor.ClockCycles += 4;
                    }
                    break;

                case 0xF3:  // Disable Interrupts (DI)
                    Processor.DisableSignal = 1;  // Interrupts Mode will change after next opcode, so signal it
                    Processor.ClockCycles += 4;
                    break;

                case 0xFB:  // Enable Interrupts (EI)
                    Processor.EnableSignal = 1;  // same here, will take effect AFTER THE NEXT instruction
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                #endregion

                // In case OPcode isn't implemented or sth. went wrong, halt emulation
                default:
                    err = true;
                    Processor.ClockCycles += 0;
                    break;
            }
            if (err)
                System.Diagnostics.Debugger.Break();
            //TODO: move this when I do the drawing
            LYReset += Processor.ClockCycles - oldClockCycle;
        }

        #region Bitwise Macro Operations
        /// <summary>
        /// Ands the register and the data and assigns this to the register (R &= d). 4 clock cycles. Will set flags. Will not increment PC.
        /// </summary>
        /// <param name="register">The register to store the result in.</param>
        /// <param name="data">The data to be ANDd.</param>
        public void AndOp(ref byte register, byte data)
        {
            register &= data;
            Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 1, 0);
            Processor.ClockCycles += 4;
        }

        /// <summary>
        /// Xors the register and the data and assigns this to the register (R ^= d). 4 clock cycles. Will set flags. Will not increment PC.
        /// </summary>
        /// <param name="register">The register to store the result in.</param>
        /// <param name="data">The data to be XORd.</param>
        private void XorOp(ref byte register, byte data)
        {
            register ^= data;
            Processor.SetFlags((register == 0) ? 1 : 0, 0, 0, 0);
            Processor.ClockCycles += 4;
        }

        /// <summary>
        /// Ors the register and the data and assigns this to the register (R |= d). 4 clock cycles. Will set flags. Will not increment PC.
        /// </summary>
        /// <param name="register">The register to store the result in.</param>
        /// <param name="data">The data to be ORd.</param>
        public void OrOp(ref byte register, byte data)
        {
            register |= data;
            Processor.SetFlags((register == 0) ? 1 : 0, 0, 0, 0);
            Processor.ClockCycles += 4;
        }

#endregion

        #region Load Macro Operations

        /// <summary>
        /// Load an immediate value (PC) into a register. 8 clock cycles. Will increment PC.
        /// </summary>
        /// <param name="register">The register to load a value into.</param>
        public void ImmediateLoadOp(ref byte register)
        {
            register = Memory.Data[Processor.PC];
            Processor.PC++;
            Processor.ClockCycles += 8;
        }


        /// <summary>
        /// Load an immediate value (PC - PC+1) into 2 registers. 12 clock cycles. Will increment PC by 2.
        /// i.e. to load a value into BC, call with (C, B) not (B, C)!
        /// </summary>
        /// <param name="register1">The register to load the first byte into.</param>
        /// <param name="register2">The register to load the second byte into.</param>
        public void Immediate16bitLoadOp(ref byte register1, ref byte register2)
        {
            register1 = Memory.Data[Processor.PC];
            register2 = Memory.Data[Processor.PC + 1];
            Processor.PC += 2;
            Processor.ClockCycles += 12;
        }

        /// <summary>
        /// Load an immediate value (PC - PC+1) into a (16 bit) register. 12 clock cycles. Will increment PC by 2.
        /// </summary>
        /// <param name="register">The register to load the word into.</param>
        public void Immediate16bitLoadOp(ref ushort register)
        {
            register = Memory.ReadWord(Processor.PC);
            Processor.PC += 2;
            Processor.ClockCycles += 12;
        }

        /// <summary>
        /// Load an immediate value (PC) into a memory location. 12 clock cycles. Will increment PC.
        /// </summary>
        /// <param name="address">The memory location to load the data into.</param>
        public void ImmediateMemoryLocationLoadOp(ushort address)
        {
            Memory.WriteByte(address, Memory.Data[Processor.PC]);
            Processor.PC++;
            Processor.ClockCycles += 12;
        }
        #endregion

        #region Transfer Macro Operations

        /// <summary>
        /// Load a value from memory into a register. 8 clock cycles. Will NOT increment PC.
        /// </summary>
        /// <param name="register">The register to load the data into.</param>
        /// <param name="address">The memory location to read the data from.</param>
        public void MemoryToRegisterOp(ref byte register, ushort address)
        {
            register = Memory.ReadByte(address);
            Processor.ClockCycles += 8;
        }

        /// <summary>
        /// Load a value from register into memory. 8 clock cycles. Will NOT increment PC.
        /// </summary>
        /// <param name="data">The register to load the data into.</param>
        /// <param name="address">The memory location to read the data from.</param>
        public void RegisterToMemoryOp(ushort address, byte data)
        {
            Memory.WriteByte(address, Processor.A);
            Processor.ClockCycles += 8;
        }

        /// <summary>
        /// Copy a value from (16 bit) register2 to register1. 8 clock cycles. Will NOT increment PC.
        /// </summary>
        /// <param name="register1">The register to copy the data INTO.</param>
        /// <param name="register2">The register to copy the data FROM.</param>
        public void RegisterToRegister16BitOp(ref ushort register1, ushort register2)
        {
            register1 = register2;
            Processor.ClockCycles += 8;
        }

        /// <summary>
        /// Copy a value from register2 to register1. 4 clock cycles. Will NOT increment PC.
        /// </summary>
        /// <param name="register1">The register to copy the data INTO.</param>
        /// <param name="register2">The register to copy the data FROM.</param>
        private void RegisterToRegisterOp(ref byte register1, byte register2)
        {
            register1 = register2;
            Processor.ClockCycles += 4;
        }

#endregion

        #region Stack Macro Operations

        /// <summary>
        /// Pushes a value onto the stack. 8 clock cycles. Will NOT increment PC.
        /// </summary>
        /// <param name="value">The value to be pushed onto the stack.</param>
        public void PushOp(byte value)
        {
            //decrement the stack pointer then add in the value. The SP points to the top, not the free spot!
            Processor.SP--;
            Memory.Data[Processor.SP] = value;
        }

        /// <summary>
        /// Pushes a 16bit value onto the stack. 16 clock cycles. Will NOT increment PC.
        /// </summary>
        /// <param name="value">The value to be pushed onto the stack.</param>
        public void Push16BitOp(ushort value)
        {
            PushOp((byte)value); //push the low end
            PushOp((byte)(value >> 8)); //push the high end
        }

        /// <summary>
        /// Pops a value from the stack. 6 clock cycles. Will NOT increment PC.
        /// </summary>
        public byte PopOp()
        {
            byte ret = Memory.Data[Processor.SP];
            Processor.SP++;
            Processor.ClockCycles += 6;
            return ret;
        }

        /// <summary>
        /// Pops a 16 bit value from the stack (i.e. pops 2 8-bit ops off the stack and merges)
        /// </summary>
        /// <returns>the 16-bit value.</returns>
        private ushort Pop16BitOp()
        {
            return (ushort)((PopOp() << 8) + PopOp());
        }

        #endregion

        #region Arithmetic Macro Operations

        /// <summary>
        /// Add a value to register A. 4 clock cycles. Will NOT increment PC. Sets flags.
        /// </summary>
        /// <param name="value">The byte to add to A.</param>
        public void AddOp(byte value)
        {
            byte a = Processor.A;
            Processor.A += value;
            //first two flags are obvious. halfcarry is set if bit 3 carries. carry set if the addition is smaller than the input
            Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, (((a & 0xf) + (Processor.A & 0xf)) & 0x10) == 0x10 ? 1 : 0,
                Processor.A < value ? 1 : 0);
            Processor.ClockCycles += 4;
        }

        /// <summary>
        /// Add a value to register A, including the carry flag. 4 clock cycles. Will NOT increment PC. Sets flags.
        /// </summary>
        /// <param name="value">The byte to add to A.</param>
        public void AdcOp(byte value)
        {
            byte a = Processor.A;
            Processor.A += (byte)(value + Processor.CarryFlag);
            Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, (((a & 0xf) + (Processor.A & 0xf)) & 0x10) == 0x10 ? 1 : 0,
                Processor.A < a + Processor.CarryFlag ? 1 : 0);
            Processor.ClockCycles += 4;
        }

        /// <summary>
        /// Add a 16 bit value to register HL. 8 clock cycles. Will NOT increment PC. Sets flags.
        /// </summary>
        /// <param name="value">The byte to add to HL.</param>
        public void Add16BitOp(ushort value) //adds to HL
        {
            ushort a = Processor.HL;
            Processor.HL += value;
            Processor.SetFlags((Processor.HL == 0) ? 1 : 0, 0, (((a & 0xfff) + (value & 0xfff)) & 0x10) == 0x10 ? 1 : 0, (Processor.HL < a ? 1 : 0));
            Processor.ClockCycles += 8;
        }

        /// <summary>
        /// Subtracts a value from register A including carry. 8 clock cycles. Will NOT increment PC. Sets flags.
        /// </summary>
        /// <param name="value">The value to subtract from A.</param>
        public void SbcOp(byte value)
        {
            byte a = Processor.A;
            Processor.A -= (byte)(value+Processor.CarryFlag);
            //first two flags are obvious. halfcarry is set if bit 3 carries. carry set if the addition is smaller than the input
            Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, (((a & 0x10) - (Processor.A & 0x10)) & 0xf) == 0xf ? 1 : 0,
                Processor.A > a ? 1 : 0);
        }

        /// <summary>
        /// Subtracts a value from register A. 4 clock cycles. Will NOT increment PC. Sets flags.
        /// </summary>
        /// <param name="value">The value to subtract from A.</param>
        public void SubOp(byte value)
        {
            byte a = Processor.A;
            Processor.A -= value;
            //first two flags are obvious. halfcarry is set if bit 3 carries. carry set if the addition is smaller than the input
            Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, (((a & 0x10) - (Processor.A & 0x10)) & 0xf) == 0xf ? 1 : 0,
                Processor.A > a ? 1 : 0);
        }

        #endregion

        #region Inc/Dec Macro Operations

        /// <summary>
        /// Decrements a value and returns it. 4 clock cycles. Sets flags. Will not increment PC.
        /// </summary>
        /// <param name="value">The value to decrement.</param>
        /// <returns>Decremented value.</returns>
        public byte DecOp(byte value)
        {
            byte a = value;
            a--;
            Processor.SetFlags((a == 0) ? 1 : 0, 1, (((value & 0x10) - (a & 0x10)) & 0x10) == 0x10 ? 1 : 0, Processor.CarryFlag);
            Processor.ClockCycles += 4;
            return a;
        }

        /// <summary>
        /// Increments a value and returns it. 8 clock cycles. Will not increment PC.
        /// </summary>
        /// <param name="value">The value to increment.</param>
        /// <returns>incremented value.</returns>
        public ushort Inc16BitOp(ushort value)
        {
            ushort a = value;
            a++;
            Processor.ClockCycles += 8;
            return a;
        }

        /// <summary>
        /// Decrements a value and returns it. 8 clock cycles. Will not increment PC.
        /// </summary>
        /// <param name="value">The value to decrement.</param>
        /// <returns>Decremented value.</returns>
        public ushort Dec16BitOp(ushort value)
        {
            ushort a = value;
            a--;
            Processor.ClockCycles += 8;
            return a;
        }

        /// <summary>
        /// Increments a value and returns it. 4 clock cycles. Sets flags. Will not increment PC.
        /// </summary>
        /// <param name="value">The value to increment.</param>
        /// <returns>incremented value.</returns>
        public byte IncOp(byte value)
        {
            byte a = value;
            a++;
            Processor.SetFlags((a == 0) ? 1 : 0, 0, (((a & 0xf) + (value & 0xf)) & 0x10) == 0x10 ? 1 : 0, Processor.CarryFlag);
            Processor.ClockCycles += 4;
            return a;
        }

        #endregion

        /// <summary>
        /// Set the bit specified to the 1. 4 clock cycles. Will not increment PC.
        /// </summary>
        /// <param name="position">the 0-based position of the bit to set.</param>
        /// <param name="val">the value to set the bit in.</param>
        /// <returns></returns>
        private byte SetBitOp(int position, byte val) 
        {
            Processor.ClockCycles += 4;
            return (byte)(val | (0x01 << position));
        }

        /// <summary>
        /// Swaps the upper and lower nibbles of a byte. 4 clock cycles. Will not increment PC. Sets flags.
        /// </summary>
        /// <param name="p">The byte to swap.</param>
        /// <returns>The swapped byte.</returns>
        public byte SwapOp(byte p)
        {
            Processor.ClockCycles += 4; //TODO: check it is 4 for a swap
            byte result = (byte)((p << 4) | (p >> 4));
            Processor.SetFlags(result == 0 ? 1 : 0, 0, 0, 0);
            return result;
        }

        #region Rotate Macro Operations - 3/4 not done!

        /// <summary>
        /// Rotates a byte to the right. 4 clock cycles. Will not increment PC. The lost bit is replaced by the carry bit. Sets flags.
        /// </summary>
        /// <param name="p">the data to be rotated.</param>
        /// <returns>The rotated data.</returns>
        public byte RrOp(byte p)
        {
            byte bit0 = (byte)(p & 0x1); //grab the bottom bit
            byte rotated = (byte)(p >> 1); //move the bits
            rotated |= (byte)(Processor.CarryFlag << 7); //add the carry flag in
            Processor.SetFlags((rotated == 0 ? 1 : 0), 0, 0, bit0);
            Processor.ClockCycles += 4; //TODO: check how much of an op rotate is
            return rotated;
        }

        /// <summary>
        /// Rotates a byte to the right. 4 clock cycles. Will not increment PC. bit0->bit7. Sets flags.
        /// </summary>
        /// <param name="p">the data to be rotated.</param>
        /// <returns>The rotated data.</returns>
        public byte RrcOp(byte p)
        {
            byte bit0 = (byte)(p & 0x1); //grab the bottom bit
            byte rotated = (byte)(p >> 1); //move the bits
            rotated |= (byte)(bit0 << 7); //add the carry flag in
            Processor.SetFlags((rotated == 0 ? 1 : 0), 0, 0, bit0);
            Processor.ClockCycles += 4; //TODO: check how much of an op rotate is
            return rotated;
        }

        /// <summary>
        /// Rotates a byte to the left. 4 clock cycles. Will not increment PC. carry flag->bit0. Sets flags.
        /// </summary>
        /// <param name="p">the data to be rotated.</param>
        /// <returns>The rotated data.</returns>
        public byte RlOp(byte p)
        {
            byte bit7 = (byte)((p & 0x80) >> 7); //grab the top bit
            byte rotated = (byte)(p << 1); //move the bits
            rotated |= Processor.CarryFlag; //add the carry flag in
            Processor.SetFlags((rotated == 0 ? 1 : 0), 0, 0, bit7);
            Processor.ClockCycles += 4; //TODO: check how much of an op rotate is
            return rotated;
        }

        /// <summary>
        /// Rotates a byte to the left. 4 clock cycles. Will not increment PC. bit7->bit0. Sets flags.
        /// </summary>
        /// <param name="p">the data to be rotated.</param>
        /// <returns>The rotated data.</returns>
        public byte RlcOp(byte p)
        {
            byte bit7 = (byte)((p & 0x80) >> 7); //grab the top bit
            byte rotated = (byte)(p << 1); //move the bits
            rotated |= bit7; //add the carry flag in
            Processor.SetFlags((rotated == 0 ? 1 : 0), 0, 0, bit7);
            Processor.ClockCycles += 4; //TODO: check how much of an op rotate is
            return rotated;
        }

        /// <summary>
        /// Shift the value right and store the lost bit in carry, setting MSB to 0. 4 clock cycles. Will not increment PC. Sets flags.
        /// </summary>
        /// <param name="val">the value to shift</param>
        /// <returns>the shifted value</returns>
        public byte SrlOp(byte val)
        {
            byte bit0 = (byte)(val & 0x1);
            byte shifted = (byte)(val >> 1);
            Processor.ClockCycles += 4;
            Processor.SetFlags(shifted == 0 ? 1 : 0, 0, 0, bit0);
            return shifted;
        }

        #endregion

        #region Jump Macro Operations
        /// <summary>
        /// Jump to an absolute, 16 bit (immediate) address. Changes PC. 12 clock cycles.
        /// </summary>
        public void JumpFarOp() //jump to a 16bit address
        {
            Processor.PC = Memory.ReadWord(Processor.PC);
            Processor.ClockCycles += 12;
        }

        /// <summary>
        /// Jump to an absolute, 16 bit (immediate) address if a condition is met. Changes PC. 12 clock cycles.
        /// </summary>
        public void ConditionalJumpFarOp(Func<bool> condition)
        {
            if (condition())
                JumpFarOp();
            else
                Processor.PC += 2; //they'll always have a 16bit operand
            Processor.ClockCycles += 12;
        }

        /// <summary>
        /// Jump to an relative, 8 bit (immediate signed) address + the current PC. Changes PC but will increment (fixed bug). 8 clock cycles.
        /// </summary>
        public void JumpNearOp()
        {
            Processor.PC = (ushort)(Processor.PC + (sbyte)Memory.ReadByte(Processor.PC));
            Processor.PC++;
            Processor.ClockCycles += 8;
        }

        /// <summary>
        /// Jump to an relative, 8 bit (immediate signed) address + the current PC if condition is met. Changes PC and increments if nojump. 8 clock cycles.
        /// </summary>
        public void ConditionalJumpNearOp(Func<bool> condition)
        {
            if (condition())
                JumpNearOp();
            else
                Processor.PC++; //only a single increment since it's an 8bit operand
            Processor.ClockCycles += 8;
        }
        #endregion

        #region Call Macro Operations

        /// <summary>
        /// Stores the current PC to the stack and moves to the immediate value. Will move PC. 12 clock cycles.
        /// </summary>
        public void CallOp() //call 
        {
            ushort nextInstr = (ushort)(Processor.PC + 2); //PC-1 is the current instruction (i.e. what called call), then PC and PC+1 are the memory address of the call
            Push16BitOp(nextInstr);
            Processor.PC = Memory.ReadWord(Processor.PC);
            Processor.ClockCycles += 12;
        }

        /// <summary>
        /// Stores the current PC to the stack and moves to the immediate value if condition. Will move PC. 12 clock cycles.
        /// </summary>
        public void ConditionalCallOp(Func<bool> condition) //call 
        {
            if(condition())
            {
                ushort nextInstr = (ushort)(Processor.PC + 2); //PC-1 is the current instruction (i.e. what called call), then PC and PC+1 are the memory address of the call
                Push16BitOp(nextInstr);
                Processor.PC = Memory.ReadWord(Processor.PC);
            }
            else
                Processor.PC += 2;
            Processor.ClockCycles += 12;
        }
        #endregion

        #region Return Macro Operations

        /// <summary>
        /// Returns from a subroutine. Will change PC. 8 clock cycles.
        /// </summary>
        public void ReturnOp()
        {
            Processor.PC = Pop16BitOp();
            Processor.ClockCycles += 8;
        }

        /// <summary>
        /// Returns from a subroutine if condition. Will change PC. 8 clock cycles.
        /// </summary>
        public void ConditionalReturnOp(Func<bool> condition)
        {
            if(condition())
                Processor.PC = Pop16BitOp();
            Processor.ClockCycles += 8;
        }
        #endregion



        #region Other Macro Operations (reset, compare)

        /// <summary>
        /// Reset the PC to some address. 32 clock cycles. Will move PC.
        /// </summary>
        /// <param name="to">The address to reset to.</param>
        public void ResetOp(ushort to)
        {
            ushort oldPC = (ushort)(Processor.PC - 1); //we want to store the old PC, not the current one - i.e. the executing opcode location
            Push16BitOp(oldPC);
            Processor.PC = to;
            Processor.ClockCycles += 32;
        }

        /// <summary>
        /// Compares register A and sets flags. 4 clock cycles. Will not increment PC. Sets flags.
        /// </summary>
        /// <param name="value">the value to compare to.</param>
        public void CompareOp(byte value)
        {
            Processor.SetFlags((Processor.A == value ? 1 : 0), 1, (((Processor.A - value) & 0x10) == 0) ? 1 : 0, Processor.A < value ? 1 : 0);
            Processor.ClockCycles += 4;
        }

        #endregion
    }
}
