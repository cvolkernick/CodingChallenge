using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace CodingChallengeV2Client
{
    class ProtocolResponse : ProtocolMessage
    {
        string header = "";
        public int _status;

        public int Status
        {
            get { return _status;  }
            private set { _status = value; }
        }

        int length = 0;
        public byte[] data;
        byte checksum = 0;

        public ProtocolResponse(string header, int status, int length, byte[] data, byte checksum)
        {
            this.header = header;
            Status = status;
            this.length = length;
            this.data = data;
            this.checksum = checksum;
        }

        // Read a response from the given stream
        public static ProtocolResponse Receive(NetworkStream stream)
        {
            // Get only the response headers (without data & checksum)
            byte[] headers = new byte[7];
            stream.Read(headers, 0, headers.Length);            

            string header = headers[0] + " " + headers[1];
            int status = headers[2];

            // Get little endian length value
            int length = headers[3] + 16 * headers[4] + 256 * headers[5] + 4096 * headers[6];

            // Get the data & checksum bytes
            byte[] headerBytes = new byte[length - 7];
            stream.Read(headerBytes, 0, length - 7);
            byte[] dataBytes = new byte[headerBytes.Length - 1];
            Buffer.BlockCopy(headerBytes, 0, dataBytes, 0, headerBytes.Length - 1);
            byte[] data = dataBytes;
            byte checksum = headerBytes[headerBytes.Length - 1];

            // Create the response object
            ProtocolResponse response = new ProtocolResponse(header, status, length, data, checksum);
            Console.WriteLine(response.ToString());

            return response;
        }        

        // Get this response's unverified (no checksum) bytes
        protected override List<byte> ToUnverifiedBytes()
        {
            List<byte> bytes = new List<byte>();
            
            // header
            bytes.AddRange(BitConverter.GetBytes((UInt16)0x0978));

            // status
            bytes.Add(Convert.ToByte(Status));

            // length
            length = 9 + data.Length;
            bytes.AddRange(BitConverter.GetBytes((uint)(length)));

            // data
            bytes.AddRange(data);

            return bytes;
        }

        // Get string representation of this response object
        public override string ToString()
        {
            return "~~~~~~~~~~~~~~~~~~~~" + "\n"
                + "RESPONSE" + "\n"
                + "~~~~~~~~~~~~~~~~~~~~" + "\n"
                + "Header: " + header + "\n"
                + "Status: " + Status + "\n"
                + "Length: " + length + "\n"
                + "Data: " + Encoding.ASCII.GetString(data) + "\n"
                + "Checksum: " + checksum;
        }
    }
}