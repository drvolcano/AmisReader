using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;

namespace AmisSimulatorConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var PortName = "COM4";

            var serialPort = new SerialPort()
            {
                PortName = PortName,
                BaudRate = 9600,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.Even,
                NewLine = "\r\n"
            };

            serialPort.Open();

            while (true)
            {
                Console.ReadLine();

                serialPort.Write(new byte[] { 0x10, 0x40, 0xF0, 0x30, 0x16 }, 0, 5);
            }
        }
    }
}