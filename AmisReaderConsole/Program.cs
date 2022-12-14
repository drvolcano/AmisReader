using MBus;
using System;

namespace AmisReaderConsole
{
    internal class Program
    {
        private static string key = "32D13550827108D59C460D6A9DBAD7BE";
        private static string sampleData1 = "68 5F 5F 68 73 F0 5B 00 00 00 00 2D 4C 01 0E 00 00 50 05 87 F2 8F 02 A8 DE 6E 67 EE A3 29 6A A9 8C E9 52 DE 2E 36 76 2F 29 70 E8 48 15 86 F5 D1 E2 FF C1 D4 AC F9 F4 0B 45 9D A9 71 FC 8C 32 43 0D 0C AE B8 A7 8A 4B 2F 2B 49 96 69 05 F0 44 6F 3E 2E B3 EC C7 45 4F B0 D9 E8 DA 70 18 09 03 48 D9 65 22 EA 16";
        private static string sampleData2 = "68 5F 5F 68 53 F0 5B 00 00 00 00 2D 4C 01 0E 01 00 50 05 E7 7E 04 EB D0 2C 32 C2 53 D6 B5 09 B4 3F 4A B6 83 68 1D 52 45 94 33 03 98 D4 86 D6 D8 E2 53 CC CA 75 62 1E 22 62 C1 AD 4C 18 D7 73 07 74 47 AF 90 19 6D 1E F8 FA 10 9C AF F8 3D C7 66 74 25 D2 DF F7 37 D0 6B 6D 2C F2 7B DB 84 5E 8E 72 8C 3E 97 16";
        private static bool test = false;

        private static void Main(string[] args)
        {
            var reader = new Reader("COM4");
            reader.DataReceived += Reader_DataReceived;
            reader.ChecksumError += Reader_ChecksumError;
            reader.TelegramEndError += Reader_TelegramEndError;

            if (test)
            {
                var text = sampleData1.Replace(" ", "");
                byte[] testData = new byte[text.Length / 2];

                for (int j = 0; j < testData.Length; j++)
                    testData[j] = Convert.ToByte(text.Substring(j * 2, 2), 16);

                reader.Test(testData);
            }
            else
            {
                reader.Start();
            }
            Console.ReadLine();
        }

        private static void Reader_ChecksumError(object sender, byte[] e)
        {
            Console.Clear();
            Console.WriteLine("TelegramEndError");
        }

        private static void Reader_DataReceived(object sender, byte[] e)
        {
            var decoded = Decoder.Run(key, e);
            var results = Analyzer.Run(decoded);

            Console.CursorTop = 0;
            Console.CursorLeft = 0;

            foreach (var a in results)
            {
                if (a.data.Count == 6)
                {
                    var seconds = a.data[0] & 0x3F;
                    var minutes = a.data[1] & 0x3F;
                    var hours = a.data[2] & 0x1F;
                    var day = a.data[3] & 0x1F;
                    var month = a.data[4] & 0x0F;
                    var year = 2000 + (a.data[4] & 0xF0) >> 1 | (a.data[3] & 0xE0) >> 5;

                    var timestamp = new DateTime(year, month, day, hours, minutes, seconds);

                    Console.WriteLine(timestamp.ToShortDateString() + " " + timestamp.ToLongTimeString());
                }
                else
                    Console.WriteLine(a.ToString().PadRight(100));
            }
        }

        private static void Reader_TelegramEndError(object sender, byte[] e)
        {
            Console.Clear();
            Console.WriteLine("ChecksumError");
        }
    }
}