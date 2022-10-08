using System;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace AmisReader
{
    public static class Decoder
    {
        public static List<int> Run(string keyString, byte[] data)
        {
            //Convert hex-string into bytes
            byte[] keyBytes = new byte[keyString.Length / 2];
            for (int i = 0; i < keyBytes.Length; i++)
                keyBytes[i] = Convert.ToByte(keyString.Substring(i * 2, 2), 16);

            var header = data.SubArray(0, 4);
            var cryptBytes = data.SubArray(19, 80);

            //Verschlüsselter Teil
            // string cryptString = decrypt.Replace(" ", "").Substring(19 * 2, 80 * 2);

            //Wiird aus Daten vor verschlüsseltem Teil zusammengesezt
            string ivString =
                "2D4C00000000010E";

            for (int i = 0; i < 8; i++)
                ivString += data[15].ToString("X2");

            //Wird von NetzOoe vergeben

            byte[] ivBytes = new byte[ivString.Length / 2];
            //  byte[] cryptBytes = new byte[cryptString.Length / 2];

            for (int i = 0; i < ivBytes.Length; i++)
                ivBytes[i] = Convert.ToByte(ivString.Substring(i * 2, 2), 16);

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