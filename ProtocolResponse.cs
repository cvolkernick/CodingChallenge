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
        int status = 0;
        int length = 0;
        public byte[] payload;
        byte checksum = 0;
        //Operation operation = Operation.Encode;

        public ProtocolResponse(string header, int status, int length, byte[] payload, byte checksum)
        {
            this.header = header;
            this.status = status;
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
            //byte checksum = GetCheckSum(ToByteArray().ToList());
            //var payloadBytes = bytes.ToList()..subArray(bytes.Length - 1);
            var payload = payloadBytes;
            var checksum = bytes[bytes.Length - 1];

            ProtocolResponse response = new ProtocolResponse(header, status, length, payload, checksum);
            Console.WriteLine(response.ToString());
            //response.ToUnverifiedBytes

            //if (GetCheckSum(ToByteArray().ToList()) != checksum)
            //{
            //error
            //}

            return response;
        }

        public override string ToString()
        {
            string output = "Header: " + header + "\n"
                + "Status: " + status + "\n"
                + "Length: " + length + "\n"
                + "Data: " + payload + "\n"
                + "Checksum: " + checksum;

            //output = output.Replace(@"\", @"-");

            return output;
        }

        public byte[] ToByteArray()
        {
            var packet = new List<byte>();
            byte[] headerBytes = BitConverter.GetBytes((UInt16)0x1092);
            header = headerBytes[0].ToString() + " " + headerBytes[1].ToString();
            packet.AddRange(BitConverter.GetBytes((UInt16)0x1092));
            packet.Add(1);
            var data = payload;
            length = 9 + data.Length;
            packet.AddRange(BitConverter.GetBytes((UInt32)(length)));
            //packet.Add((byte)(operation == Operation.Encode ? 1 : 2));
            packet.AddRange(data);
            checksum = GetCheckSum(packet);
            packet.Add(checksum);

            return packet.ToArray();
        }

        protected override List<byte> ToUnverifiedBytes()
        {
            throw new NotImplementedException();
        }
    }
}
