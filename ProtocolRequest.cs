using System;
using System.Collections.Generic;
using System.Text;

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

        // Get this request's unverified (no checksum) bytes
        protected override List<byte> ToUnverifiedBytes()
        {
            List<byte> bytes = new List<byte>();

            // header
            byte[] headerBytes = BitConverter.GetBytes((UInt16)0x1092);
            header = headerBytes[0].ToString() + " " + headerBytes[1].ToString();
            bytes.AddRange(headerBytes);

            // version
            bytes.Add(1);
            
            length = 9 + payload.Length;

            // length
            bytes.AddRange(BitConverter.GetBytes((uint)(length)));

            // operation
            bytes.Add((byte)(operation == Operation.Encode ? 1 : 2));

            // data
            bytes.AddRange(payload);

            // set checksum but don't include
            checksum = GetCheckSum(bytes);

            return bytes;
        }

        // Get string representation of this request object
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
    }    
}