using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;

namespace MBus
{
    public class Reader
    {
        /*
            Telegram:
            0x68
            length(data)
            length(data)
            0x68
            data
            ...
            data
            checksum
            0x16
            */

        private bool stop = false;

        public Reader(string portName)
        {
            PortName = portName;
        }

        public event EventHandler<byte[]> ChecksumError;

        public event EventHandler<byte[]> DataReceived;

        public event EventHandler<byte[]> TelegramEndError;

        public byte ByteAck => 0xe5;
        public byte ByteEnd => 0x16;
        public byte ByteHeader => 0x68;
        public int LenghtHeader => 4;
        public int LengtFooter => 2;
        public int LengthData { get; private set; } = 0;
        public string PortName { get; set; }
        public int TelegramLength => LenghtHeader + LengthData + LengtFooter;

        public void Start()
        {
            Start(null);
        }

        public void Stop()
        {
            stop = true;
        }

        public void Test(byte[] data)
        {
            Start(new MemoryStream(data));
        }

        private void Start(MemoryStream testStream)
        {
            while (stop) ;

            Task.Run(() =>
            {
                var serialPort = new SerialPort()
                {
                    PortName = PortName,
                    BaudRate = 9600,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Parity = Parity.Even,
                    NewLine = "\r\n"
                };

                if (testStream == null)
                    serialPort.Open();

                bool reading = false;
                List<byte> buffer = new List<byte>();
                byte data = 0;
                bool hasData = false;

                while (!stop)
                {
                    //Check if there is either real data on the port or test data in the stream
                    hasData = false;

                    if (testStream == null && serialPort.BytesToRead > 0)
                    {
                        data = (byte)serialPort.ReadByte();
                        hasData = true;
                    }
                    else if (testStream != null && testStream.Position < testStream.Length)
                    {
                        data = (byte)testStream.ReadByte();
                        hasData = true;
                    }

                    if (hasData)
                    {
                        if (!reading && data == ByteHeader)
                        {
                            buffer.Clear();
                            LengthData = 0;
                            reading = true;
                        }

                        if (reading)
                        {
                            buffer.Add(data);

                            //Analyze header
                            if (buffer.Count == 4)
                            {
                                //Check if content is valid
                                if (buffer[0] == ByteHeader
                                 && buffer[1] == buffer[2]
                                 && buffer[3] == ByteHeader)
                                    LengthData = buffer[1];
                                else
                                    reading = false;
                            }
                        }

                        //Wait until analyzed telegram length is reached
                        if (buffer.Count == TelegramLength)
                        {
                            //Last read byte must be 0x16
                            if (reading && data != ByteEnd)
                                TelegramEndError?.Invoke(this, buffer.ToArray());
                            else
                            {
                                var content = buffer.ToArray().SubArray(LenghtHeader, LengthData);

                                reading = false;
                                var checksum = 0;

                                foreach (var b in content)
                                    checksum += b;

                                if ((checksum & 0xFF) != buffer[buffer.Count - 2])
                                    ChecksumError?.Invoke(this, buffer.ToArray());
                                else
                                    DataReceived?.Invoke(this, content);
                            }

                            reading = false;
                        }

                        //Whenever 0x16 is received and not within telegram length, send an ack to the meter.
                        //This is to acknowledge an already read telegram or to start communicaion afther the meter sent the every-minute-signal.
                        if (!reading && data == ByteEnd)
                            serialPort.Write(new byte[] { ByteAck }, 0, 1);
                    }
                }
                serialPort.Close();
                stop = false;
            });
        }
    }
}