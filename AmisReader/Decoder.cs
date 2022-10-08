using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;

namespace AmisReader
{
    public static class Decoder
    {
        public static List<int> Run(string keyString, byte[] data)
        {
            int i = 0;

            //-----------------------------------
            var h1 = data[i++];//0x68
            var l1 = data[i++];//Nutzdatenlänge
            var l2 = data[i++];//Nutzdatenlänge Wiederholung
            var h2 = data[i++];//0x68
            //-----------------------------------
            var c = data[i++];
            var a = data[i++];
            var ci = data[i++];

            var identification = new byte[]
            {
                data [i++],
                data[i++],
                data[i++],
                data[i++]
            };

            var manufacturer = new byte[]
            {
                    data[i++],
                    data[i++],
            };

            var version = data[i++];
            var devType = data[i++];
            var access = data[i++];
            var status = data[i++];

            var configuration = new byte[]
            {
                    data[i++],
                    data[i++],
            };

            //-----------------------------------
            //4 bytes header
            //15 bytes pre data
            //(len- 15) bytes data
            //2 bytes footer

            var cryptBytes = data.SubArray(19, l1 - 15);

            //-----------------------------------

            var ivBytes = new byte[]
            {
                manufacturer[0],
                manufacturer[1],
                identification[0],
                identification[1],
                identification[2],
                identification[3],
                version,
                devType,
                access,
                access,
                access,
                access,
                access,
                access,
                access,
                access,
            };

            //Convert hex-string into bytes
            byte[] keyBytes = new byte[16];
            for (int j = 0; j < 16; j++)
                keyBytes[j] = Convert.ToByte(keyString.Substring(j * 2, 2), 16);

            var aes = Aes.Create();
            aes.Padding = PaddingMode.Zeros;
            var result = aes.CreateDecryptor(keyBytes, ivBytes).TransformFinalBlock(cryptBytes, 0, cryptBytes.Length);

            //erstes Byte ist das Dif, wenn 8x (höchstes Bit), folgt ein DIFE byte
            //Ansonsten kodiert der 2. Teil die Länge, 4= 4 , 6= 6. Theoretisch gibt es noch sonerzahlen für String,etc.

            var b066D = result.SubArray(4, 6);//Datum M-Bus Format
            var b0403 = BitConverter.ToInt32(result.SubArray(12, 4), 0);//1.8.0 Wirk Bezug Wh
            var b04833C = BitConverter.ToInt32(result.SubArray(19, 4), 0);//2.8.0 Wirk Einspeis Wh
            var b8410FB8273 = BitConverter.ToInt32(result.SubArray(28, 4), 0);//3.8.1 Blind + varh
            var b8410FB82F33C = BitConverter.ToInt32(result.SubArray(38, 4), 0);//4.8.1 Blind - varh
            var b042B = BitConverter.ToInt32(result.SubArray(44, 4), 0);//1.7.0 //Wirkleistung Bezug W
            var b04AB3C = BitConverter.ToInt32(result.SubArray(51, 4), 0);//2.7.0 //Wirkleistung Einspeis W
            var b04FB14 = BitConverter.ToInt32(result.SubArray(58, 4), 0);//3.7.0 Blindleistung + W
            var b04FB943C = BitConverter.ToInt32(result.SubArray(66, 4), 0);//4.7.0 Blindleistung - W
            var b0483FF04 = BitConverter.ToInt32(result.SubArray(74, 4), 0);//1.128.0 Inkassozählwerk Wh

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