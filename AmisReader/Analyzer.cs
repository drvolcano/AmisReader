using System;
using System.Collections.Generic;
using System.IO;

namespace MBus
{
    public static class Analyzer
    {
        public static List<Tag> Run(byte[] data)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(data));
            var h1 = reader.ReadByte();
            var h2 = reader.ReadByte();

            var results = new List<Tag>();

            do
            {
                var tag = new Tag();

                while (true)
                {
                    var dif = reader.ReadByte();
                    tag.difs.Add(dif);

                    if ((dif & 0x80) == 0)
                        break;
                }

                while (true)
                {
                    var vif = reader.ReadByte();
                    tag.vifs.Add(vif);

                    if ((vif & 0x80) == 0)
                        break;
                }

                tag.data.AddRange(reader.ReadBytes(tag.DataLength));

                results.Add(tag);
            } while (reader.BaseStream.Position < reader.BaseStream.Length - 2);

            return results;

            //erstes Byte ist das Dif, wenn 8x (höchstes Bit), folgt ein DIFE byte
            //Ansonsten kodiert der 2. Teil die Länge, 4= 4 , 6= 6. Theoretisch gibt es noch sonerzahlen für String,etc.
        }

        public static List<int> RunFixed(byte[] data)
        {
            var b066D = data.SubArray(4, 6);//Datum M-Bus Format
            var b0403 = BitConverter.ToInt32(data.SubArray(12, 4), 0);//1.8.0 Wirk Bezug Wh
            var b04833C = BitConverter.ToInt32(data.SubArray(19, 4), 0);//2.8.0 Wirk Einspeis Wh
            var b8410FB8273 = BitConverter.ToInt32(data.SubArray(28, 4), 0);//3.8.1 Blind + varh
            var b8410FB82F33C = BitConverter.ToInt32(data.SubArray(38, 4), 0);//4.8.1 Blind - varh
            var b042B = BitConverter.ToInt32(data.SubArray(44, 4), 0);//1.7.0 //Wirkleistung Bezug W
            var b04AB3C = BitConverter.ToInt32(data.SubArray(51, 4), 0);//2.7.0 //Wirkleistung Einspeis W
            var b04FB14 = BitConverter.ToInt32(data.SubArray(58, 4), 0);//3.7.0 Blindleistung + W
            var b04FB943C = BitConverter.ToInt32(data.SubArray(66, 4), 0);//4.7.0 Blindleistung - W
            var b0483FF04 = BitConverter.ToInt32(data.SubArray(74, 4), 0);//1.128.0 Inkassozählwerk Wh

            return new List<int>() {
            b0403 ,
            b04833C ,
            b8410FB8273 ,
            b8410FB82F33C ,
            b042B ,
            b04AB3C ,
            b04FB14 ,
            b04FB943C ,
            b0483FF04 ,
            };
        }
    }
}