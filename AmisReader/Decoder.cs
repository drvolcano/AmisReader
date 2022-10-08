using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;

namespace AmisReader
{
    public static class Decoder
    {
        public static byte[] Run(string keyString, byte[] data)
        {
            int i = 0;
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

            var cryptBytes = data.SubArray(i, data.Length - i);

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
            return result;
        }
    }
}