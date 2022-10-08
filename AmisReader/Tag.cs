using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmisReader
{
    public class Tag
    {
        public List<byte> data = new List<byte>();
        public List<byte> difs = new List<byte>();
        public List<byte> vifs = new List<byte>();

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