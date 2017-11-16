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
        string payload = "";
        byte checksum = 0;
        Operation operation = Operation.Encode;

        public ProtocolResponse(string header, int status, int length, string payload, byte checksum, Operation operation)
        {
            this.header = header;
            this.status = status;
            this.length = length;
            this.payload = payload;
            this.checksum = checksum;
            this.operation = operation;
        }

        public static ProtocolResponse Receive(NetworkStream stream)
        {
            byte[] headers = new byte[7];
            stream.Read(headers, 0, headers.Length);            

            string header = headers[0] + " " + headers[1];
            int status = headers[2];
            var length = headers[3] + 16 * headers[4] + 256 * headers[5] + 4096 * headers[6];
            
            byte[] bytes = new byte[length - 7];

            stream.Read(bytes, 0, length);

            byte checksum = GetCheckSum(ToByteArray().ToList());
            //var payloadBytes = bytes.subArray(n - 1 ???)
            //var payload = ??? bytesToAscii(payloadBytes) ???

            ProtocolResponse response = new ProtocolResponse(header, status, length, payload, checksum);

            if (GetCheckSum(ToByteArray().ToList()) != checksum)
            {
                //error
            }

            return response;
        }

        public override string ToString()
        {
            return "Header: " + header + "\n"
                + "Status: " + status + "\n"
                + "Length: " + length + "\n"
                + "Data: " + payload + "\n"
                + "Checksum: " + checksum;
        }

        public byte GetCheckSum(List<byte> bytes)
        {
            byte cs = 0;

            for (var i = 0; i < bytes.Count; i++)
            {
                if (((1 << (i % 8)) & bytes[i]) > 0)
                {
                    cs++;
                }
            }

            return cs;
        }

        public byte[] ToByteArray()
        {
            var packet = new List<byte>();
            byte[] headerBytes = BitConverter.GetBytes((UInt16)0x1092);
            header = headerBytes[0].ToString() + " " + headerBytes[1].ToString();
            packet.AddRange(BitConverter.GetBytes((UInt16)0x1092));
            packet.Add(1);
            var data = Encoding.ASCII.GetBytes(this.payload);
            length = 9 + data.Length;
            packet.AddRange(BitConverter.GetBytes((UInt32)(length)));
            packet.Add((byte)(operation == Operation.Encode ? 1 : 2));
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
