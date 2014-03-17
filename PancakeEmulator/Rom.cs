using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeEmulator
{
    class RomData
    {
        //0100-0103
        public byte[] CodeExecutionPoint;
        public byte[] Logo;
        public byte[] Name;
        public byte IsColorGB;
        public byte NewLicenseHigh;
        public byte NewLicenseLow;
        public byte IsSuperGB;
        public byte CartridgeType;
        public byte ROMSize;

        public byte RAMSize;
        public byte DestinationCode;
        public byte OldLicense;
        public byte MaskROMNumber; //???
        public byte Complement;
        //public byte Checksum; //ignored
    }

    class Rom
    {
        
        public String Name { get; set; }

        public RomData Header = new RomData();

        public byte[] Data { get; set; }

        private long size;

        public void LoadFromFile(/*String fileName*/)
        {
            using(BinaryReader reader = new BinaryReader(new FileStream("cpu_instrs/01-special.gb", FileMode.Open)))
            {
                size =  reader.BaseStream.Length;
                this.Data = new byte[size];
                //read in the rom data
                for(int i = 0; i < size; ++i)
                    Data[i] = reader.ReadByte();

                //beginning code execution
                Header.CodeExecutionPoint = new byte[4];
                Array.Copy(Data, 0x100, Header.CodeExecutionPoint, 0, 4);

                //104-133 are the nintendo logo
                Header.Logo = new byte[48];
                Array.Copy(Data, 0x104, Header.Logo, 0, 48);

                //134-142 are the game name
                Header.Name = new byte[16];
                Array.Copy(Data, 0x134, Header.Name, 0, 16);

                //0x80 for Color GB, 0x00 otherwise
                Header.IsColorGB = Data[0x143];
                Header.NewLicenseHigh = Data[0x144];
                Header.NewLicenseLow = Data[0x145];
                Header.IsSuperGB = Data[0x146];
                Header.CartridgeType = Data[0x147];
                Header.ROMSize = Data[0x148];
                Header.RAMSize = Data[0x149];

                Header.DestinationCode = Data[0x14A];
                Header.OldLicense = Data[0x14B];
                Header.MaskROMNumber = Data[0x14C];
                Header.Complement = Data[0x14D];
                //checksum is ignored, let's not bother
            }
        }
    }
}
