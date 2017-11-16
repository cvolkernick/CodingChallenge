﻿using System;
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
        string payload = "";
        Operation operation = Operation.Encode;
        byte checksum = 0;

        public ProtocolRequest(string payload, Operation operation)
        {            
            this.payload = payload;
            this.operation = operation;
        }

        public override string ToString()
        {
            return "Header: " + header + "\n"
                + "Version: 1" + "\n"
                + "Length: " + length + "\n"
                + "Operation: " + operation + "\n"
                + "Data: " + payload + "\n"
                + "Checksum: " + checksum;
                
        }

        // Form the request
        protected override List<byte> ToUnverifiedBytes()
        {
            var packet = new List<byte>();

            // header
            byte[] headerBytes = BitConverter.GetBytes((UInt16)0x1092);
            header = headerBytes[0].ToString() + " " + headerBytes[1].ToString();
            packet.AddRange(headerBytes);

            // version
            packet.Add(1);
                        
            var data = Encoding.ASCII.GetBytes(this.payload);
            length = 9 + data.Length;

            // length
            packet.AddRange(BitConverter.GetBytes((UInt32)(length)));

            // operation
            packet.Add((byte)(operation == Operation.Encode ? 1 : 2));

            // data
            packet.AddRange(data);

            // checksum
            checksum = GetCheckSum(packet);
            //packet.Add(checksum);

            return packet;
        }
    }

    public enum Operation
    {
        Encode,
        Decode
    }
}
