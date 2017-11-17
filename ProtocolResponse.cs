using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
        public byte[] payload;
        byte checksum = 0;

        public ProtocolResponse(string header, int status, int length, byte[] payload, byte checksum)
        {
            this.header = header;
            Status = status;
            this.length = length;
            this.payload = payload;
            this.checksum = checksum;
        }

        public static ProtocolResponse Receive(NetworkStream stream)
        {
            // Get only the response headers (without payload & checksum)
            byte[] headers = new byte[7];
            stream.Read(headers, 0, headers.Length);            

            string header = headers[0] + " " + headers[1];
            int status = headers[2];

            // Get little endian length value
            var length = headers[3] + 16 * headers[4] + 256 * headers[5] + 4096 * headers[6];

            // Get the payload & checksum bytes
            byte[] bytes = new byte[length - 7];

            stream.Read(bytes, 0, length - 7);

            var payloadBytes = new byte[bytes.Length - 1];
            Buffer.BlockCopy(bytes, 0, payloadBytes, 0, bytes.Length - 1);
            var payload = payloadBytes;
            var checksum = bytes[bytes.Length - 1];

            ProtocolResponse response = new ProtocolResponse(header, status, length, payload, checksum);
            Console.WriteLine(response.ToString());

            return response;
        }

        public override string ToString()
        {
            return "~~~~~~~~~~~~~~~~~~~~" + "\n"
                + "RESPONSE" + "\n"
                + "~~~~~~~~~~~~~~~~~~~~" + "\n"
                + "Header: " + header + "\n"
                + "Status: " + Status + "\n"
                + "Length: " + length + "\n"
                + "Data: " + Encoding.ASCII.GetString(payload) + "\n"
                + "Checksum: " + checksum;
        }

        protected override List<byte> ToUnverifiedBytes()
        {
            var packet = new List<byte>();
            
            packet.AddRange(BitConverter.GetBytes((UInt16)0x0978));
            packet.Add(Convert.ToByte(Status));

            var data = payload;
            length = 9 + data.Length;
            packet.AddRange(BitConverter.GetBytes((UInt32)(length)));
            packet.AddRange(data);

            return packet;
        }
    }
}