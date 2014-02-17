using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeEmulator
{
    class Emulator
    {
        public Processor Processor { get; set; }

        public Memory Memory { get; set; }

        public static void Main(String[] args)
        {
            Emulator main = new Emulator();
            main.Processor = new Processor();
            main.Memory = new Memory();
            main.Run();
        }

        public void Run()
        {
            //get the instruction and increment the program count
            byte instr = Memory.Data[Processor.PC];
            Processor.PC++;
            Decode(instr);
        }

        public void Decode(byte opcode)
        {
            //so you read in the opcode at PC, then act on the data /following/ PC (thus the PC+1 everywhere)
            //Opcode decoding table from SharpGB
            //https://code.google.com/p/sharpgb/
            //temp variables
            ushort tempWord;
            byte tempByte;
            switch (opcode)
            {
                #region   /* Load instructions */
                // immediate loads
                case 0x3E:  // A <- immediate  
                    Processor.A = Memory.Data[Processor.PC];
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x06:  // B <- immediate  
                    Processor.B = Memory.Data[Processor.PC];
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x0E:  // C <- immediate  
                    Processor.C = Memory.Data[Processor.PC];
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x16:  // D <- immediate  
                    Processor.D = Memory.Data[Processor.PC];
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x1E:  // E <- immediate  
                    Processor.E = Memory.Data[Processor.PC];
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x26:  // H <- immediate  
                    Processor.H = Memory.Data[Processor.PC];
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x2E:  // L <- immediate  
                    Processor.L = Memory.Data[Processor.PC];
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x01:  // BC <- immediate  
                    Processor.C = Memory.Data[Processor.PC];
                    Processor.PC++;
                    Processor.B = Memory.Data[Processor.PC];
                    Processor.PC++;
                    Processor.ClockCycles += 12;
                    break;
                case 0x11:  // DE <- immediate  
                    Processor.E = Memory.Data[Processor.PC];
                    Processor.PC++;
                    Processor.D = Memory.Data[Processor.PC];
                    Processor.PC++;
                    Processor.ClockCycles += 12;
                    break;
                case 0x21:  // HL <- immediate  
                    Processor.L = Memory.Data[Processor.PC];
                    Processor.PC++;
                    Processor.H = Memory.Data[Processor.PC];
                    Processor.PC++;
                    Processor.ClockCycles += 12;
                    break;
                case 0x31:  // SP <- immediate
                    Processor.SP = Memory.GetWord(Processor.PC);
                    Processor.PC += 2;
                    Processor.ClockCycles += 12;
                    break;
                case 0x36:  // (HL) <- immediate  the adress pointed at by HL
                    Memory.WriteByte(Processor.HL, Memory.Data[Processor.PC]);
                    Processor.PC++;
                    Processor.ClockCycles += 12;
                    break;

                // memory to register transfer
                /*case 0xF2:    // A <- (0xFF00 + C)
                    word = (ushort)(0xFF00 + Processor.C);
                    Processor.A = Memory.ReadByte(word);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break; supposedly removed.*/
                case 0x0A:    // A <- (BC)
                    tempWord = (ushort)(Processor.B << 8 | Processor.C);
                    Processor.A = Memory.ReadByte(tempWord);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x1A:    // A <- (DE)
                    tempWord = (ushort)(Processor.D << 8 | Processor.E);
                    Processor.A = Memory.ReadByte(tempWord);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x7E:  // A <- (HL)
                    Processor.A = Memory.ReadByte(Processor.HL);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x46:  // B <- (HL)
                    Processor.B = Memory.ReadByte(Processor.HL);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x4E:  // C <- (HL)
                    Processor.C = Memory.ReadByte(Processor.HL);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x56:  // D <- (HL)
                    Processor.D = Memory.ReadByte(Processor.HL);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x5E:  // E <- (HL)
                    Processor.E = Memory.ReadByte(Processor.HL);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x66:  // H <- (HL)
                    Processor.H = Memory.ReadByte(Processor.HL);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x6E:  // L <- (HL)
                    Processor.L = Memory.ReadByte(Processor.HL);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x2A:  // A <- (HL), HL++      /* FLAGS??? supposedly not needed*/
                    Processor.A = Memory.ReadByte(Processor.HL);
                    Processor.HL++;
                    Processor.H = (byte)(Processor.HL >> 8);
                    Processor.L = (byte)Processor.HL;
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0xFA:  // A <- (nn immediate)
                    Processor.A = Memory.ReadByte(Memory.GetWord(Processor.PC));
                    Processor.PC += 2;
                    Processor.ClockCycles += 16;
                    break;
                case 0xF0:  // A <- (0xFF00+ n immediate)
                    tempWord = (ushort)(0xFF00 + Memory.Data[Processor.PC]);
                    Processor.A = Memory.ReadByte(tempWord);
                    Processor.PC++;
                    Processor.ClockCycles += 12;
                    break;
                //TODO: here onwards
                // register to memory transfer
                case 0xE2:    // (0xFF00 + C) <- A
                    word = (ushort)(0xFF00 + Processor.C);
                    Memory.WriteByte(word, Processor.A);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x02:  // (BC) <- A
                    word = (ushort)(Processor.B << 8 | Processor.C);
                    Memory.WriteByte(word, Processor.A);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x12:  // (DE) <- A
                    word = (ushort)(Processor.D << 8 | Processor.E);
                    Memory.WriteByte(word, Processor.A);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x77:  // (HL) <- A
                    Memory.WriteByte(Processor.HL, Processor.A);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x70:  // (HL) <- B
                    Memory.WriteByte(Processor.HL, Processor.B);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x71:  // (HL) <- C
                    Memory.WriteByte(Processor.HL, Processor.C);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x72:  // (HL) <- D
                    Memory.WriteByte(Processor.HL, Processor.D);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x73:  // (HL) <- E
                    Memory.WriteByte(Processor.HL, Processor.E);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x74:  // (HL) <- H
                    Memory.WriteByte(Processor.HL, Processor.H);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x75:  // (HL) <- L
                    Memory.WriteByte(Processor.HL, Processor.L);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0xEA:  // (nn) <- A
                    word = (ushort)(Memory.Data[Processor.PC + 2] << 8 | Memory.Data[Processor.PC + 1]);
                    Memory.WriteByte(word, Processor.A);
                    Processor.PC += 3;
                    Processor.ClockCycles += 16;
                    break;
                case 0xE0:  // (0xFF00+ n immediate) <- A
                    word = (ushort)(0xFF00 + Memory.Data[Processor.PC + 1]);
                    Memory.WriteByte(word, Processor.A);
                    Processor.PC += 2;
                    Processor.ClockCycles += 12;
                    break;
                case 0x32:  // (HL) <- A, HL--    
                    Memory.WriteByte(Processor.HL, Processor.A);
                    Processor.HL--;
                    Processor.H = (byte)(Processor.HL >> 8);
                    Processor.L = (byte)Processor.HL;
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x22:  // (HL) <- A, HL++    
                    Memory.WriteByte(Processor.HL, Processor.A);
                    Processor.HL++;
                    Processor.H = (byte)(Processor.HL >> 8);
                    Processor.L = (byte)Processor.HL;
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x08:  // (nn) <- SP
                    word = (ushort)(Memory.Data[Processor.PC + 2] << 8 | Memory.Data[Processor.PC + 1]);
                    Memory.WriteByte(word, (byte)Processor.SP);
                    Memory.WriteByte((ushort)(word + 1), (byte)(Processor.SP >> 8));
                    Processor.PC += 3;
                    Processor.ClockCycles += 20;
                    break;

                // register to register transfer
                case 0x7F:  // A <- A
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x78:  // A <- B
                    Processor.A = Processor.B;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x79:  // A <- C
                    Processor.A = Processor.C;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x7A:  // A <- D
                    Processor.A = Processor.D;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x7B:  // A <- E
                    Processor.A = Processor.E;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x7C:  // A <- H
                    Processor.A = Processor.H;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x7D:  // A <- L
                    Processor.A = Processor.L;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x47:  // B <- A
                    Processor.B = Processor.A;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x40:  // B <- B
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x41:  // B <- C
                    Processor.B = Processor.C;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x42:  // B <- D
                    Processor.B = Processor.D;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x43:  // B <- E
                    Processor.B = Processor.E;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x44:  // B <- H
                    Processor.B = Processor.H;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x45:  // B <- L
                    Processor.B = Processor.L;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x4F:  // C <- A
                    Processor.C = Processor.A;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x48:  // C <- B
                    Processor.C = Processor.B;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x49:  // C <- C
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x4A:  // C <- D
                    Processor.C = Processor.D;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x4B:  // C <- E
                    Processor.C = Processor.E;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x4C:  // C <- H
                    Processor.C = Processor.H;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x4D:  // C <- L
                    Processor.C = Processor.L;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x57:  // D <- A
                    Processor.D = Processor.A;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x50:  // D <- B
                    Processor.D = Processor.B;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x51:  // D <- C
                    Processor.D = Processor.C;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x52:  // D <- D
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x53:  // D <- E
                    Processor.D = Processor.E;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x54:  // D <- H
                    Processor.D = Processor.H;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x55:  // D <- L
                    Processor.D = Processor.L;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x5F:  // E <- A
                    Processor.E = Processor.A;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x58:  // E <- B
                    Processor.E = Processor.B;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x59:  // E <- C
                    Processor.E = Processor.C;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x5A:  // E <- D
                    Processor.E = Processor.D;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x5B:  // E <- E
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x5C:  // E <- H
                    Processor.E = Processor.H;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x5D:  // E <- L
                    Processor.E = Processor.L;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x67:  // H <- A
                    Processor.H = Processor.A;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x60:  // H <- B
                    Processor.H = Processor.B;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x61:  // H <- C
                    Processor.H = Processor.C;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x62:  // H <- D
                    Processor.H = Processor.D;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x63:  // H <- E
                    Processor.H = Processor.E;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x64:  // H <- H
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x65:  // H <- L
                    Processor.H = Processor.L;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x6F:  // L <- A
                    Processor.L = Processor.A;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x68:  // L <- B
                    Processor.L = Processor.B;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x69:  // L <- C
                    Processor.L = Processor.C;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x6A:  // L <- D
                    Processor.L = Processor.D;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x6B:  // L <- E
                    Processor.L = Processor.E;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x6C:  // L <- H
                    Processor.L = Processor.H;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x6D:  // L <- L
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;

                case 0xF9:  // SP <- HL
                    Processor.SP = Processor.HL;
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;

                case 0xF8: // HL <- SP + signed immediate
                    word = Processor.HL;
                    Processor.HL = (ushort)(Processor.SP + (sbyte)(Memory.Data[Processor.PC + 1]));
                    Processor.SetFlags(0, 0, (word & 0x800) - (Processor.HL & 0x800), (word & 0x8000) - (Processor.HL & 0x8000));
                    Processor.PC += 2;
                    Processor.ClockCycles += 12;
                    break;


                // STACK OPS
                // PUSH
                case 0xF5:  // PUSH AF
                    PushOp(Processor.F);
                    PushOp(Processor.A);
                    Processor.PC++;
                    Processor.ClockCycles += 16;
                    break;
                case 0xC5:  // PUSH BC
                    PushOp(Processor.C);
                    PushOp(Processor.B);
                    Processor.PC++;
                    Processor.ClockCycles += 16;
                    break;
                case 0xD5:  // PUSH DE
                    PushOp(Processor.E);
                    PushOp(Processor.D);
                    Processor.PC++;
                    Processor.ClockCycles += 16;
                    break;
                case 0xE5:  // PUSH HL
                    PushOp(Processor.L);
                    PushOp(Processor.H);
                    Processor.PC++;
                    Processor.ClockCycles += 16;
                    break;

                // POP
                case 0xF1:  // POP AF
                    Processor.A = op_pop();
                    Processor.F = op_pop();
                    Processor.PC++;
                    Processor.ClockCycles += 12;
                    break;
                case 0xC1:  // POP BC
                    Processor.B = op_pop();
                    Processor.C = op_pop();
                    Processor.PC++;
                    Processor.ClockCycles += 12;
                    break;
                case 0xD1:  // POP DE
                    Processor.D = op_pop();
                    Processor.E = op_pop();
                    Processor.PC++;
                    Processor.ClockCycles += 12;
                    break;
                case 0xE1:  // POP HL
                    Processor.H = op_pop();
                    Processor.L = op_pop();
                    Processor.PC++;
                    Processor.ClockCycles += 12;
                    break;


                #endregion

                #region /* Arithmetic instructions */

                // 8-bit arithmetics

                // ADD
                case 0x87:
                    op_add(Processor.A);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x80:
                    op_add(Processor.B);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x81:
                    op_add(Processor.C);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x82:
                    op_add(Processor.D);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x83:
                    op_add(Processor.E);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x84:
                    op_add(Processor.H);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x85:
                    op_add(Processor.L);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x86:
                    op_add(Memory.ReadByte(Processor.HL));
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0xC6:
                    op_add(Memory.Data[Processor.PC + 1]);
                    Processor.PC += 2;
                    Processor.ClockCycles += 8;
                    break;

                // ADC
                case 0x8F:
                    op_adc(Processor.A);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x88:
                    op_adc(Processor.B);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x89:
                    op_adc(Processor.C);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x8A:
                    op_adc(Processor.D);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x8B:
                    op_adc(Processor.E);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x8C:
                    op_adc(Processor.H);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x8D:
                    op_adc(Processor.L);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x8E:
                    op_adc(Memory.ReadByte(Processor.HL));
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0xCE:
                    op_add(Memory.Data[Processor.PC + 1]);
                    Processor.PC += 2;
                    Processor.ClockCycles += 8;
                    break;

                // SUB
                case 0x97:
                    op_sub(Processor.A);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x90:
                    op_sub(Processor.B);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x91:
                    op_sub(Processor.C);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x92:
                    op_sub(Processor.D);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x93:
                    op_sub(Processor.E);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x94:
                    op_sub(Processor.H);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x95:
                    op_sub(Processor.L);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x96:
                    op_sub(Memory.ReadByte(Processor.HL));
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0xD6:
                    op_sub(Memory.Data[Processor.PC + 1]);
                    Processor.PC += 2;
                    Processor.ClockCycles += 8;
                    break;

                // SBC
                case 0x9F:
                    op_sbc(Processor.A);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x98:
                    op_sbc(Processor.B);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x99:
                    op_sbc(Processor.C);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x9A:
                    op_sbc(Processor.D);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x9B:
                    op_sbc(Processor.E);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x9C:
                    op_sbc(Processor.H);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x9D:
                    op_sbc(Processor.L);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x9E:
                    op_sbc(Memory.ReadByte(Processor.HL));
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                // sbc + immediate non-existent?


                // INC
                case 0x3C:
                    Processor.A = op_inc(Processor.A);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x04:
                    Processor.B = op_inc(Processor.B);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x0C:
                    Processor.C = op_inc(Processor.C);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x14:
                    Processor.D = op_inc(Processor.D);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x1C:
                    Processor.E = op_inc(Processor.E);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x24:
                    Processor.H = op_inc(Processor.H);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x2C:
                    Processor.L = op_inc(Processor.L);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x34:
                    value = Memory.ReadByte(Processor.HL);
                    Memory.WriteByte(Processor.HL, op_inc(value));
                    Processor.PC++;
                    Processor.ClockCycles += 12;
                    break;

                // DEC
                case 0x3D:
                    Processor.A = op_dec(Processor.A);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x05:
                    Processor.B = op_dec(Processor.B);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x0D:
                    Processor.C = op_dec(Processor.C);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x15:
                    Processor.D = op_dec(Processor.D);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x1D:
                    Processor.E = op_dec(Processor.E);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x25:
                    Processor.H = op_dec(Processor.H);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x2D:
                    Processor.L = op_dec(Processor.L);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x35:
                    value = Memory.ReadByte(Processor.HL);
                    Memory.WriteByte(Processor.HL, op_dec(value));
                    Processor.PC++;
                    Processor.ClockCycles += 12;
                    break;

                // CMP
                case 0xBF:
                    op_cmp(Processor.A);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xB8:
                    op_cmp(Processor.B);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xB9:
                    op_cmp(Processor.C);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xBA:
                    op_cmp(Processor.D);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xBB:
                    op_cmp(Processor.E);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xBC:
                    op_cmp(Processor.H);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xBD:
                    op_cmp(Processor.L);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xBE:
                    op_cmp(Memory.ReadByte(Processor.HL));
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0xFE:
                    op_cmp(Memory.Data[Processor.PC + 1]);
                    Processor.PC += 2;
                    Processor.ClockCycles += 8;
                    break;

                // 16-bit arithmetics

                // ADD
                case 0x09:
                    op_add16((ushort)(Processor.B << 8 | Processor.C));
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x19:
                    op_add16((ushort)(Processor.D << 8 | Processor.E));
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x29:
                    op_add16(Processor.HL);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x39:
                    op_add16(Processor.SP);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0xE8:  // SP += signed immediate byte                              
                    word = Processor.SP;
                    Processor.SP = (ushort)(Processor.SP + ((sbyte)Memory.Data[Processor.PC + 1]));
                    Processor.SetFlags(0, 0, (word & 0x800) - (Processor.SP & 0x800), (word & 0x8000) - (Processor.SP & 0x8000));
                    Processor.PC += 2;
                    Processor.ClockCycles += 16;
                    break;

                // INC
                case 0x03:  // BC++
                    word = (ushort)(Processor.B << 8 | Processor.C);
                    word++;
                    Processor.B = (byte)(word >> 8);
                    Processor.C = (byte)word;
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x13:  // DE++
                    word = (ushort)(Processor.D << 8 | Processor.E);
                    word++;
                    Processor.D = (byte)(word >> 8);
                    Processor.E = (byte)word;
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x23:  // HL++
                    Processor.HL++;
                    Processor.H = (byte)(Processor.HL >> 8);
                    Processor.L = (byte)Processor.HL;
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x33:  // SP++
                    Processor.SP++;
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;

                // DEC
                case 0x0B:  // BC--
                    word = (ushort)(Processor.B << 8 | Processor.C);
                    word--;
                    Processor.B = (byte)(word >> 8);
                    Processor.C = (byte)word;
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x1B:  // DE--
                    word = (ushort)(Processor.D << 8 | Processor.E);
                    word--;
                    Processor.D = (byte)(word >> 8);
                    Processor.E = (byte)word;
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x2B:  // HL--
                    Processor.HL--;
                    Processor.H = (byte)(Processor.HL >> 8);
                    Processor.L = (byte)Processor.HL;
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0x3B:  // SP--
                    Processor.SP--;
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;

                #endregion

                #region /* Jump instructions */
                // absolute jumps
                case 0xC3:  // Unconditional + 2B immediate operands
                    op_jmpfar();
                    Processor.ClockCycles += 12;
                    break;

                case 0xC2:  // Conditional NZ + 2B immediate operands
                    if (Processor.ZeroFlag == 0) op_jmpfar();
                    else Processor.PC += 3;
                    Processor.ClockCycles += 12;
                    break;
                case 0xCA:  // Conditional Z + 2B immediate operands
                    if (Processor.ZeroFlag != 0) op_jmpfar();
                    else Processor.PC += 3;
                    Processor.ClockCycles += 12;
                    break;
                case 0xD2:  // Conditional NC + 2B immediate operands
                    if (Processor.CarryFlag == 0) op_jmpfar();
                    else Processor.PC += 3;
                    Processor.ClockCycles += 12;
                    break;
                case 0xDA:  // Conditional C + 2B immediate operands
                    if (Processor.CarryFlag != 0) op_jmpfar();
                    else Processor.PC += 3;
                    Processor.ClockCycles += 12;
                    break;
                case 0xE9:  // Unconditional jump to HL
                    Processor.PC = Processor.HL;
                    Processor.ClockCycles += 4;
                    break;

                // relative jumps
                case 0x18:  // Unconditional + relative byte
                    op_jmpnear();
                    Processor.ClockCycles += 8;
                    break;
                case 0x20:  // Conditional NZ + relative byte
                    if (Processor.ZeroFlag == 0) op_jmpnear();
                    else Processor.PC += 2;
                    Processor.ClockCycles += 8;
                    break;
                case 0x28:  // Conditional Z + relative byte
                    if (Processor.ZeroFlag != 0) op_jmpnear();
                    else Processor.PC += 2;
                    Processor.ClockCycles += 8;
                    break;
                case 0x30:  // Conditional NC + relative byte
                    if (Processor.CarryFlag == 0) op_jmpnear();
                    else Processor.PC += 2;
                    Processor.ClockCycles += 8;
                    break;
                case 0x38:  // Conditional C + relative byte
                    if (Processor.CarryFlag != 0) op_jmpnear();
                    else Processor.PC += 2;
                    Processor.ClockCycles += 8;
                    break;

                // calls
                case 0xCD:  // unconditional
                    op_call();
                    Processor.ClockCycles += 12;
                    break;
                case 0xC4:  // Conditional NZ
                    if (Processor.ZeroFlag == 0) op_call();
                    else Processor.PC += 3;
                    Processor.ClockCycles += 12;
                    break;
                case 0xCC:  // Conditional Z
                    if (Processor.ZeroFlag != 0) op_call();
                    else Processor.PC += 3;
                    Processor.ClockCycles += 12;
                    break;
                case 0xD4:  // Conditional NC
                    if (Processor.CarryFlag == 0) op_call();
                    else Processor.PC += 3;
                    Processor.ClockCycles += 12;
                    break;
                case 0xDC:  // Conditional C
                    if (Processor.CarryFlag != 0) op_call();
                    else Processor.PC += 3;
                    Processor.ClockCycles += 12;
                    break;

                // resets
                case 0xC7:
                    ResetOp(0x00);
                    Processor.ClockCycles += 32;
                    break;
                case 0xCF:
                    ResetOp(0x08);
                    Processor.ClockCycles += 32;
                    break;
                case 0xD7:
                    ResetOp(0x10);
                    Processor.ClockCycles += 32;
                    break;
                case 0xDF:
                    ResetOp(0x18);
                    Processor.ClockCycles += 32;
                    break;
                case 0xE7:
                    ResetOp(0x20);
                    Processor.ClockCycles += 32;
                    break;
                case 0xEF:
                    ResetOp(0x28);
                    Processor.ClockCycles += 32;
                    break;
                case 0xF7:
                    ResetOp(0x30);
                    Processor.ClockCycles += 32;
                    break;
                case 0xFF:
                    ResetOp(0x38);
                    Processor.ClockCycles += 32;
                    break;

                // returns
                case 0xC9:  // unconditional
                    op_return();
                    Processor.ClockCycles += 8;
                    break;
                case 0xD9:  // unconditional plus enable interrupts (RETI)
                    op_return();
                    Processor.EIsignaled = 1;
                    Processor.ClockCycles += 8;
                    break;
                case 0xC0:  // Conditional NZ
                    if (Processor.ZeroFlag == 0) op_return();
                    else Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0xC8:  // Conditional Z
                    if (Processor.ZeroFlag != 0) op_return();
                    else Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0xD0:  // Conditional NC
                    if (Processor.CarryFlag == 0) op_return();
                    else Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0xD8:  // Conditional C
                    if (Processor.CarryFlag != 0) op_return();
                    else Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;


                #endregion

                #region /* Logical instructions */

                // OR
                case 0xB7:  // A = A OR A = A !!!
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 0, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xB0:  // A = A OR B
                    Processor.A |= Processor.B;
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 0, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xB1:  // A = A OR C
                    Processor.A |= Processor.C;
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 0, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xB2:  // A = A OR D
                    Processor.A |= Processor.D;
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 0, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xB3:  // A = A OR E
                    Processor.A |= Processor.E;
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 0, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xB4:  // A = A OR H
                    Processor.A |= Processor.H;
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 0, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xB5:  // A = A OR L
                    Processor.A |= Processor.L;
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 0, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xB6:  // A = A OR (HL)
                    Processor.A |= Memory.ReadByte(Processor.HL);
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 0, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0xF6:  // A = A OR immediate
                    Processor.A |= Memory.Data[Processor.PC + 1];
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 0, 0);
                    Processor.PC += 2;
                    Processor.ClockCycles += 8;
                    break;
                // XOR
                case 0xAF:  // A = A XOR A = 0 !!!
                    Processor.A = 0;
                    Processor.SetFlags(1, 0, 0, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xA8:  // A = A XOR B
                    Processor.A ^= Processor.B;
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 0, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xA9:  // A = A XOR C
                    Processor.A ^= Processor.C;
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 0, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xAA:  // A = A XOR D
                    Processor.A ^= Processor.D;
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 0, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xAB:  // A = A XOR E
                    Processor.A ^= Processor.E;
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 0, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xAC:  // A = A XOR H
                    Processor.A ^= Processor.H;
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 0, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xAD:  // A = A XOR L
                    Processor.A ^= Processor.L;
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 0, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xAE:  // A = A XOR (HL)
                    Processor.A ^= Memory.ReadByte(Processor.HL);
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 0, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0xEE:  // A = A XOR immediate
                    Processor.A ^= Memory.Data[Processor.PC + 1];
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 0, 0);
                    Processor.PC += 2;
                    Processor.ClockCycles += 8;
                    break;
                // AND
                case 0xA7:  // A = A AND A
                    Processor.A &= Processor.A;
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 1, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xA0:  // A = A AND B
                    Processor.A &= Processor.B;
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 1, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xA1:  // A = A AND C
                    Processor.A &= Processor.C;
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 1, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xA2:  // A = A AND D
                    Processor.A &= Processor.D;
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 1, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xA3:  // A = A AND E
                    Processor.A &= Processor.E;
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 1, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xA4:  // A = A AND H
                    Processor.A &= Processor.H;
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 1, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xA5:  // A = A AND L
                    Processor.A &= Processor.L;
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 1, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0xA6:  // A = A AND (HL)
                    Processor.A &= Memory.ReadByte(Processor.HL);
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 1, 0);
                    Processor.PC++;
                    Processor.ClockCycles += 8;
                    break;
                case 0xE6:  // A = A AND immediate
                    Processor.A &= Memory.Data[Processor.PC + 1];
                    Processor.SetFlags((Processor.A == 0) ? 1 : 0, 0, 1, 0);
                    Processor.PC += 2;
                    Processor.ClockCycles += 8;
                    break;

                #endregion

                #region /* Miscellaneous instructions */

                case 0x07:  // Rotate A left
                    Processor.A = op_rlc(Processor.A);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x17:  // Rotate A left with carry
                    Processor.A = op_rl(Processor.A);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x0F:  // Rotate A right
                    Processor.A = op_rrc(Processor.A);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;
                case 0x1F:  // Rotate A right with carry
                    Processor.A = op_rr(Processor.A);
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;

                #region CB
                case 0xCB:  // Big Operation! includes rotations, shifts, swaps, set etc.
                    // check the operand to identify real operation
                    switch (Memory.Data[Processor.PC + 1])
                    {
                        // SWAPS
                        case 0x37:  // SWAP A
                            Processor.A = op_swap(Processor.A);
                            Processor.ClockCycles += 8;
                            break;
                        case 0x30:  // SWAP B
                            Processor.B = op_swap(Processor.B);
                            Processor.ClockCycles += 8;
                            break;
                        case 0x31:  // SWAP C
                            Processor.C = op_swap(Processor.C);
                            Processor.ClockCycles += 8;
                            break;
                        case 0x32:  // SWAP D
                            Processor.D = op_swap(Processor.D);
                            Processor.ClockCycles += 8;
                            break;
                        case 0x33:  // SWAP E
                            Processor.E = op_swap(Processor.E);
                            Processor.ClockCycles += 8;
                            break;
                        case 0x34:  // SWAP H
                            Processor.H = op_swap(Processor.H);
                            Processor.ClockCycles += 8;
                            break;
                        case 0x35:  // SWAP L
                            Processor.L = op_swap(Processor.L);
                            Processor.ClockCycles += 8;
                            break;
                        case 0x36:  // SWAP (HL)
                            value = op_swap(Memory.ReadByte(Processor.HL));
                            Memory.WriteByte(Processor.HL, value);
                            Processor.ClockCycles += 16;
                            break;
                        // ROTATIONS
                        case 0x07:  // Rotate A left
                            Processor.A = op_rlc(Processor.A);
                            Processor.ClockCycles += 8;
                            break;
                        case 0x00:  // Rotate B left
                            Processor.B = op_rlc(Processor.B);
                            Processor.ClockCycles += 8;
                            break;
                        case 0x01:  // Rotate C left
                            Processor.C = op_rlc(Processor.C);
                            Processor.ClockCycles += 8;
                            break;
                        case 0x02:  // Rotate D left
                            Processor.D = op_rlc(Processor.D);
                            Processor.ClockCycles += 8;
                            break;
                        case 0x03:  // Rotate E left
                            Processor.E = op_rlc(Processor.E);
                            Processor.ClockCycles += 8;
                            break;
                        case 0x04:  // Rotate H left
                            Processor.H = op_rlc(Processor.H);
                            Processor.ClockCycles += 8;
                            break;
                        case 0x05:  // Rotate L left
                            Processor.L = op_rlc(Processor.L);
                            Processor.ClockCycles += 8;
                            break;
                        case 0x06:  // Rotate (HL) left
                            value = op_rlc(Memory.ReadByte(Processor.HL));
                            Memory.WriteByte(Processor.HL, value);
                            Processor.ClockCycles += 16;
                            break;

                        // SETS
                        case 0xC7:  // Set 0, A
                            Processor.A = op_setbit(0, Processor.A);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xCF:  // Set 1, A
                            Processor.A = op_setbit(1, Processor.A);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xD7:  // Set 2, A
                            Processor.A = op_setbit(2, Processor.A);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xDF:  // Set 3, A
                            Processor.A = op_setbit(3, Processor.A);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xE7:  // Set 4, A
                            Processor.A = op_setbit(4, Processor.A);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xEF:  // Set 5, A
                            Processor.A = op_setbit(5, Processor.A);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xF7:  // Set 6, A
                            Processor.A = op_setbit(6, Processor.A);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xFF:  // Set 7, A
                            Processor.A = op_setbit(7, Processor.A);
                            Processor.ClockCycles += 8;
                            break;

                        case 0xC0:  // Set 0, B
                            Processor.B = op_setbit(0, Processor.B);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xC8:  // Set 1, B
                            Processor.B = op_setbit(1, Processor.B);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xD0:  // Set 2, B
                            Processor.B = op_setbit(2, Processor.B);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xD8:  // Set 3, B
                            Processor.B = op_setbit(3, Processor.B);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xE0:  // Set 4, B
                            Processor.B = op_setbit(4, Processor.B);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xE8:  // Set 5, B
                            Processor.B = op_setbit(5, Processor.B);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xF0:  // Set 6, B
                            Processor.B = op_setbit(6, Processor.B);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xF8:  // Set 7, B
                            Processor.B = op_setbit(7, Processor.B);
                            Processor.ClockCycles += 8;
                            break;

                        case 0xC1:  // Set 0, C
                            Processor.C = op_setbit(0, Processor.C);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xC9:  // Set 1, C
                            Processor.C = op_setbit(1, Processor.C);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xD1:  // Set 2, C
                            Processor.C = op_setbit(2, Processor.C);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xD9:  // Set 3, C
                            Processor.C = op_setbit(3, Processor.C);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xE1:  // Set 4, C
                            Processor.C = op_setbit(4, Processor.C);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xE9:  // Set 5, C
                            Processor.C = op_setbit(5, Processor.C);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xF1:  // Set 6, C
                            Processor.C = op_setbit(6, Processor.C);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xF9:  // Set 7, C
                            Processor.C = op_setbit(7, Processor.C);
                            Processor.ClockCycles += 8;
                            break;

                        case 0xC2:  // Set 0, D
                            Processor.D = op_setbit(0, Processor.D);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xCA:  // Set 1, D
                            Processor.D = op_setbit(1, Processor.D);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xD2:  // Set 2, D
                            Processor.D = op_setbit(2, Processor.D);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xDA:  // Set 3, D
                            Processor.D = op_setbit(3, Processor.D);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xE2:  // Set 4, D
                            Processor.D = op_setbit(4, Processor.D);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xEA:  // Set 5, D
                            Processor.D = op_setbit(5, Processor.D);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xF2:  // Set 6, D
                            Processor.D = op_setbit(6, Processor.D);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xFA:  // Set 7, D
                            Processor.D = op_setbit(7, Processor.D);
                            Processor.ClockCycles += 8;
                            break;

                        case 0xC3:  // Set 0, E
                            Processor.E = op_setbit(0, Processor.E);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xCB:  // Set 1, E
                            Processor.E = op_setbit(1, Processor.E);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xD3:  // Set 2, E
                            Processor.E = op_setbit(2, Processor.E);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xDB:  // Set 3, E
                            Processor.E = op_setbit(3, Processor.E);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xE3:  // Set 4, E
                            Processor.E = op_setbit(4, Processor.E);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xEB:  // Set 5, E
                            Processor.E = op_setbit(5, Processor.E);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xF3:  // Set 6, E
                            Processor.E = op_setbit(6, Processor.E);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xFB:  // Set 7, E
                            Processor.E = op_setbit(7, Processor.E);
                            Processor.ClockCycles += 8;
                            break;

                        case 0xC4:  // Set 0, H
                            Processor.H = op_setbit(0, Processor.H);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xCC:  // Set 1, H
                            Processor.H = op_setbit(1, Processor.H);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xD4:  // Set 2, H
                            Processor.H = op_setbit(2, Processor.H);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xDC:  // Set 3, H
                            Processor.H = op_setbit(3, Processor.H);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xE4:  // Set 4, H
                            Processor.H = op_setbit(4, Processor.H);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xEC:  // Set 5, H
                            Processor.H = op_setbit(5, Processor.H);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xF4:  // Set 6, H
                            Processor.H = op_setbit(6, Processor.H);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xFC:  // Set 7, H
                            Processor.H = op_setbit(7, Processor.H);
                            Processor.ClockCycles += 8;
                            break;

                        case 0xC5:  // Set 0, L
                            Processor.L = op_setbit(0, Processor.L);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xCD:  // Set 1, L
                            Processor.L = op_setbit(1, Processor.L);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xD5:  // Set 2, L
                            Processor.L = op_setbit(2, Processor.L);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xDD:  // Set 3, L
                            Processor.L = op_setbit(3, Processor.L);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xE5:  // Set 4, L
                            Processor.L = op_setbit(4, Processor.L);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xED:  // Set 5, L
                            Processor.L = op_setbit(5, Processor.L);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xF5:  // Set 6, L
                            Processor.L = op_setbit(6, Processor.L);
                            Processor.ClockCycles += 8;
                            break;
                        case 0xFD:  // Set 7, L
                            Processor.L = op_setbit(7, Processor.L);
                            Processor.ClockCycles += 8;
                            break;

                        case 0xC6:  // Set 0, (HL)
                            value = op_setbit(0, Memory.ReadByte(Processor.HL));
                            Memory.WriteByte(Processor.HL, value);
                            Processor.ClockCycles += 16;
                            break;
                        case 0xCE:  // Set 1, (HL)
                            value = op_setbit(1, Memory.ReadByte(Processor.HL));
                            Memory.WriteByte(Processor.HL, value);
                            Processor.ClockCycles += 16;
                            break;
                        case 0xD6:  // Set 2, (HL)
                            value = op_setbit(2, Memory.ReadByte(Processor.HL));
                            Memory.WriteByte(Processor.HL, value);
                            Processor.ClockCycles += 16;
                            break;
                        case 0xDE:  // Set 3, (HL)
                            value = op_setbit(3, Memory.ReadByte(Processor.HL));
                            Memory.WriteByte(Processor.HL, value);
                            Processor.ClockCycles += 16;
                            break;
                        case 0xE6:  // Set 4, (HL)
                            value = op_setbit(4, Memory.ReadByte(Processor.HL));
                            Memory.WriteByte(Processor.HL, value);
                            Processor.ClockCycles += 16;
                            break;
                        case 0xEE:  // Set 5, (HL)
                            value = op_setbit(5, Memory.ReadByte(Processor.HL));
                            Memory.WriteByte(Processor.HL, value);
                            Processor.ClockCycles += 16;
                            break;
                        case 0xF6:  // Set 6, (HL)
                            value = op_setbit(6, Memory.ReadByte(Processor.HL));
                            Memory.WriteByte(Processor.HL, value);
                            Processor.ClockCycles += 16;
                            break;
                        case 0xFE:  // Set 7, (HL)
                            value = op_setbit(7, Memory.ReadByte(Processor.HL));
                            Memory.WriteByte(Processor.HL, value);
                            Processor.ClockCycles += 16;
                            break;



                        // RESETS

                        // BIT


                        default:
                            UnknownOperand = true;
                            Processor.ClockCycles += 0;
                            Processor.PC -= 2;
                            break;
                    }
                    Processor.PC += 2;
                    break;
                #endregion

                case 0x2F:  // Complement A
                    Processor.A = (byte)~Processor.A;
                    Processor.SubtractFlag = 1;
                    Processor.HalfCarryFlag = 1;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;

                case 0x3F:  // Complement Carry
                    Processor.CarryFlag ^= 1;
                    Processor.SubtractFlag = 0;
                    Processor.HalfCarryFlag = 0;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;

                case 0x37:  // Set Carry
                    Processor.CarryFlag = 1;
                    Processor.SubtractFlag = 0;
                    Processor.HalfCarryFlag = 0;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;

                case 0x00:  // NOP
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;

                case 0x76:  // HALT
                    if (Processor.IME) Processor.CPUHalt = true;
                    else Processor.SkipPCCounting = true;
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;

                case 0x10:  // STOP
                    Processor.PC++;
                    if (Memory.Data[Processor.PC] != 0)   // check if next operand is 0 (has to be!)
                    {
                        UnknownOperand = true;
                        Processor.ClockCycles += 0;
                        break;
                    }
                    else
                    {
                        Processor.CPUStop = true;
                        Processor.ClockCycles += 4;
                    }
                    break;

                case 0xF3:  // Disable Interrupts (DI)
                    Processor.DIsignaled = 1;  // Interrupts Mode will change after next opcode, so signal it
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;

                case 0xFB:  // Enable Interrupts (EI)
                    Processor.EIsignaled = 1;  // same here, will take effect AFTER THE NEXT instruction
                    Processor.PC++;
                    Processor.ClockCycles += 4;
                    break;

                #endregion

                // In case OPcode isn't implemented or sth. went wrong, halt emulation
                default:
                    UnknownOPcode = true;
                    Processor.ClockCycles += 0;
                    break;
            }
        }

        //return from a subroutine
        public void ReturnOp()
        {
            Processor.PC = Pop16BitOp();
        }

        //reset the PC to some address
        public byte ResetOp(ushort to)
        {
            ushort oldPC = PC;
            PushOp(oldPC);
            Processor.PC = to;
        }

        //so push this value to the current stack addr, then drop the stack pointer
        public void PushOp(byte val)
        {
            Memory.Data[Processor.SP] = val;
            Processor.SP--;
        }

        public byte PopOp()
        {
            Processor.SP++;
            return Memory.Data[Processor.SP];
        }
    }
}
