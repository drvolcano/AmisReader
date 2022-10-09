﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using System.IO;

namespace AmisReader
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
            this.PortName = portName;
        }

        public event EventHandler<byte[]> DataReceived;

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

        public void Start(MemoryStream sampleStream)
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

                if (sampleStream == null)
                    serialPort.Open();

                bool reading = false;
                List<byte> buffer = new List<byte>();
                byte data = 0;
                bool hasData = false;

                while (!stop)
                {
                    if (sampleStream == null && serialPort.BytesToRead > 0)
                    {
                        data = (byte)serialPort.ReadByte();
                        hasData = true;
                    }
                    else if (sampleStream != null && sampleStream.Position < sampleStream.Length)
                    {
                        data = (byte)sampleStream.ReadByte();
                        hasData = true;
                    }
                    else
                        hasData = false;

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

                        if (reading && data == ByteEnd && buffer.Count == TelegramLength)
                        {
                            //TOOD: Checksum

                            reading = false;
                            DataReceived?.Invoke(this, buffer.ToArray().SubArray(LenghtHeader, LengthData));
                        }

                        if (!reading && data == ByteEnd)
                            serialPort.Write(new byte[] { ByteAck }, 0, 1);
                    }
                }
                serialPort.Close();
                stop = false;
            });
        }

        public void Stop()
        {
            stop = true;
        }
    }
}