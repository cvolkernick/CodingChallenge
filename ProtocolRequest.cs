using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingChallengeV2Client
{
    class ProtocolRequest : ProtocolMessage
    {
        string header = "";
        int length = 0;
        byte[] payload;
        Operation operation = Operation.Encode;
        byte checksum = 0;

        public ProtocolRequest(byte[] payload, Operation operation)
        {            
            this.payload = payload;
            this.operation = operation;
        }

        public override string ToString()
        {
            return "~~~~~~~~~~~~~~~~~~~~" + "\n"
                + "REQUEST" + "\n"
                + "~~~~~~~~~~~~~~~~~~~~" + "\n"
                + "Header: " + header + "\n"
                + "Version: 1" + "\n"
                + "Length: " + length + "\n"
                + "Operation: " + operation + "\n"
                + "Data: " + Encoding.ASCII.GetString(payload) + "\n"
                + "Checksum: " + checksum;                
        }

        // Get this request's unverified (no checksum) bytes
        protected override List<byte> ToUnverifiedBytes()
        {
            var bytes = new List<byte>();

            // header
            byte[] headerBytes = BitConverter.GetBytes((UInt16)0x1092);
            header = headerBytes[0].ToString() + " " + headerBytes[1].ToString();
            bytes.AddRange(headerBytes);

            // version
            bytes.Add(1);
            
            length = 9 + payload.Length;

            // length
            bytes.AddRange(BitConverter.GetBytes((UInt32)(length)));

            // operation
            bytes.Add((byte)(operation == Operation.Encode ? 1 : 2));

            // data
            bytes.AddRange(payload);

            return bytes;
        }
    }    
}