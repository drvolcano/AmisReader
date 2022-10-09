using System;
using System.Collections.Generic;

namespace MBus
{
    public class Tag
    {  /*
        Length in Bit 	Code 	Meaning            Code 	Meaning
        0 	            0000 	No data 	       1000 	Selection for Readout
        8 	            0001 	8 Bit Integer 	   1001 	2 digit BCD
        16 	            0010 	16 Bit Integer 	   1010 	4 digit BCD
        24 	            0011 	24 Bit Integer 	   1011 	6 digit BCD
        32 	            0100 	32 Bit Integer 	   1100 	8 digit BCD
        32 / N 	        0101 	32 Bit Real 	   1101 	variable length
        48 	            0110 	48 Bit Integer 	   1110 	12 digit BCD
        64 	            0111 	64 Bit Integer     1111 	Special Functions
        */

        public List<byte> data = new List<byte>();

        public List<byte> difs = new List<byte>();

        public List<byte> vifs = new List<byte>();

        public enum TagType : byte
        {
            /// <summary>
            /// No data
            /// </summary>
            NoData = 0,

            /// <summary>
            /// 8 Bit Integer
            /// </summary>
            Int8 = 1,

            /// <summary>
            /// 16 Bit Integer
            /// </summary>
            Int16 = 2,

            /// <summary>
            /// 24 Bit Integer
            /// </summary>
            Int24 = 3,

            /// <summary>
            /// 32 Bit Integer
            /// </summary>
            Int32 = 4,

            /// <summary>
            /// 32 Bit Real
            /// </summary>
            Real32 = 5,

            /// <summary>
            /// 48 Bit Integer
            /// </summary>
            Int48 = 6,

            /// <summary>
            /// 64 Bit Integer
            /// </summary>
            Int64 = 7,

            /// <summary>
            /// Selection for Readout
            /// </summary>
            SelectioForReadout = 8,

            /// <summary>
            /// 2 digit BCD
            /// </summary>
            Bcd2 = 9,

            /// <summary>
            /// 4 digit BCD
            /// </summary>
            Bcd4 = 10,

            /// <summary>
            /// 6 digit BCD
            /// </summary>
            Bcd6 = 11,

            /// <summary>
            /// 8 digit BCD
            /// </summary>
            Bcd8 = 12,

            /// <summary>
            /// variable length
            /// </summary>
            VariableLength = 13,

            /// <summary>
            /// 12 digit BCD
            /// </summary>
            Bcd12 = 14,

            /// <summary>
            /// Special Functions
            /// </summary>
            SpecialFunctions = 15
        }

        public byte Coding => (byte)(difs[0] & 0xF);

        public int DataLength
        {
            get
            {
                switch (Type)
                {
                    case TagType.NoData: return 0;
                    case TagType.Int8: return 1;
                    case TagType.Int16: return 2;
                    case TagType.Int24: return 3;
                    case TagType.Int32: return 4;
                    case TagType.Real32: return 4;
                    case TagType.Int48: return 6;
                    case TagType.Int64: return 8;

                    case TagType.SelectioForReadout: return 0;
                    case TagType.Bcd2: return 1;
                    case TagType.Bcd4: return 2;
                    case TagType.Bcd6: return 3;
                    case TagType.Bcd8: return 4;
                    case TagType.VariableLength: return -1;
                    case TagType.Bcd12: return 6;
                    case TagType.SpecialFunctions: return 8;
                    default: return -1;
                }
            }
        }

        public bool ExtensionBit => (difs[0] & 0x80) > 0;

        public bool FunctionField => (difs[0] & 0x20) > 0;

        public bool StorageNumbeLsb => (difs[0] & 0x40) > 0;

        public TagType Type => (TagType)(difs[0] & 0xF);

        public override string ToString()
        {
            var text = "";

            foreach (var dif in difs)
                text += dif.ToString("X2");

            text += " ";

            foreach (var vif in vifs)
                text += vif.ToString("X2");

            text += " " + Type.ToString() + " = ";

            switch (Type)
            {
                case TagType.Int16: text += BitConverter.ToUInt16(data.ToArray(), 0).ToString(); break;
                case TagType.Int32: text += BitConverter.ToUInt32(data.ToArray(), 0).ToString(); break;
                case TagType.Int64: text += BitConverter.ToUInt64(data.ToArray(), 0).ToString(); break;

                default:
                    //Todo: alle in sinnvollte Daten umwandeln
                    foreach (var d in data)
                        text += d.ToString("X2");
                    break;
            }

            return text;
        }
    }
}