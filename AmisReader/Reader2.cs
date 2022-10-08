using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace AmisReader
{
    /// <summary>
    /// Alternative Version, kann aber bei NetzOoe keine Daten lesen (ERR0007)
    /// </summary>
    internal class Reader2
    {
        public void Do()
        {
            var serialPort = new SerialPort()
            {
                PortName = "COM4",
                BaudRate = 300,
                DataBits = 7,
                StopBits = StopBits.One,
                Parity = Parity.Even,
                NewLine = "\r\n"
            };

            Console.WriteLine(serialPort.PortName + " with " + serialPort.BaudRate + " baud " + serialPort.DataBits + " " + serialPort.StopBits + " " + serialPort.Parity);
            serialPort.Open();

            var send = "";
            var receive = "";

            send = "#";
            Console.WriteLine("send: " + send);
            serialPort.WriteLine(send);

            send = "/?!";
            Console.WriteLine("send: " + send);
            serialPort.WriteLine(send);

            receive = serialPort.ReadLine();

            int start = receive.IndexOf("/");

            if (start != 0)
                Console.WriteLine("truncated: " + receive.Substring(0, start));

            receive = receive.Substring(start);
            Console.WriteLine("receive: " + receive);

            var vid = receive.Substring(1, 3);
            var baudId = receive.Substring(4, 1);
            var id = receive.Substring(5, receive.Length - 5);

            var baud = 0;

            switch (int.Parse(baudId))
            {
                case 0: baud = 300; break;
                case 1: baud = 600; break;
                case 2: baud = 1200; break;
                case 3: baud = 2400; break;
                case 4: baud = 4800; break;
                case 5: baud = 9600; break;
                case 6: baud = 19200; break;
                case 9: baud = 115200; break;
            }

            Console.WriteLine("vid = " + vid);
            Console.WriteLine("baudId = " + baudId);
            Console.WriteLine("baud = " + baud);
            Console.WriteLine("id = " + id);

            var V = "0";
            var Z = baudId;
            var Y = "0";

            send = (char)6 + V + Z + Y;
            Console.WriteLine("send: " + send);
            serialPort.WriteLine(send);

            Task.Delay(200).Wait();

            serialPort.BaudRate = baud;
            Console.WriteLine(serialPort.PortName + " with " + serialPort.BaudRate + " baud " + serialPort.DataBits + " " + serialPort.StopBits + " " + serialPort.Parity);

            Console.WriteLine("receive: ");

            while (true)
                if (serialPort.BytesToRead > 0)
                {
                    var b = serialPort.ReadByte();
                    Console.WriteLine("0x" + b.ToString("X2") + " " + (char)b + " ");
                }

            Console.ReadLine();
        }
    }
}