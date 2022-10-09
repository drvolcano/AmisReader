using System;
using System.IO;
using System.Security.Cryptography;

namespace MBus
{
    public static class Decoder
    {
        /// <summary>
        /// Decodes the content of the telegram
        /// </summary>
        /// <param name="keyString">The key as hex-string (2 digits per byte, no spaces)</param>
        /// <param name="data">the content of the telegram, without header and footer</param>
        /// <returns></returns>
        public static byte[] Run(string keyString, byte[] data)
        {
            //Generate key (hex-string to bytes)
            byte[] key = new byte[16];
            for (int j = 0; j < 16; j++)
                key[j] = Convert.ToByte(keyString.Substring(j * 2, 2), 16);

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                //Separate data
                var c = reader.ReadByte();
                var a = reader.ReadByte();
                var ci = reader.ReadByte();
                var identification = reader.ReadBytes(4);
                var manufacturer = reader.ReadBytes(2);
                var version = reader.ReadByte();
                var devType = reader.ReadByte();
                var access = reader.ReadByte();
                var status = reader.ReadByte();
                var configuration = reader.ReadBytes(2);
                var encryptedData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));

                using (var iv = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(iv))
                {
                    writer.Write(manufacturer);
                    writer.Write(identification);
                    writer.Write(version);
                    writer.Write(devType);

                    for (int i = 0; i < 8; i++)
                        writer.Write(access);

                    //Decrypt
                    var aes = Aes.Create();
                    aes.Padding = PaddingMode.Zeros;
                    var decryptedData = aes.CreateDecryptor(key, iv.ToArray()).TransformFinalBlock(encryptedData, 0, encryptedData.Length);

                    return decryptedData;
                }
            }
        }
    }
}