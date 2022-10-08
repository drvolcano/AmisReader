using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmisReader
{
    public class Tag
    {
        /*
        Length in Bit 	Code 	Meaning 	Code 	Meaning
        0 	0000 	No data 	1000 	Selection for Readout
        8 	0001 	8 Bit Integer 	1001 	2 digit BCD
        16 	0010 	16 Bit Integer 	1010 	4 digit BCD
        24 	0011 	24 Bit Integer 	1011 	6 digit BCD
        32 	0100 	32 Bit Integer 	1100 	8 digit BCD
        32 / N 	0101 	32 Bit Real 	1101 	variable length
        48 	0110 	48 Bit Integer 	1110 	12 digit BCD
        64 	0111 	64 Bit Integer 	1111 	Special Functions
        */
        public List<byte> data = new List<byte>();
        public List<byte> difs = new List<byte>();
        public List<byte> vifs = new List<byte>();

        public byte Coding => (byte)(difs[0] & 0xF);
        public bool ExtensionBit => (difs[0] & 0x80) > 0;
        public bool FunctionField => (difs[0] & 0x20) > 0;
        public bool StorageNumbeLsb => (difs[0] & 0x40) > 0;

        public override string ToString()
        {
            var text = "";

            foreach (var dif in difs)
                text += dif.ToString("X2");

            text += " ";

            foreach (var vif in vifs)
                text += vif.ToString("X2");

            text += " = ";

            if (data.Count == 4)
                text += BitConverter.ToInt32(data.ToArray(), 0).ToString();
            else
                foreach (var d in data)
                    text += d.ToString("X2");

            return text;
        }
    }
}