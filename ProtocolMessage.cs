using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingChallengeV2Client
{
    abstract class ProtocolMessage
    {
        // Get checksum for this message
        public byte GetCheckSum()
        {
            return GetCheckSum(ToUnverifiedBytes());
        }

        // Get message bytes without verified checksum
        protected abstract List<byte> ToUnverifiedBytes();

        // Get message bytes with verified checksum
        public List<byte> ToVerifiedBytes()
        {
            var byteArray = ToUnverifiedBytes();
            var checksum = GetCheckSum(byteArray);
            byteArray.Add(checksum);

            return byteArray;
        }       

        // Get checksum for the passed bytes
        public static byte GetCheckSum(List<byte> bytes)
        {
            byte cs = 0;
            var i = 0;

            foreach (byte b in bytes)
            {
                if (((1 << (i % 8)) & b) > 0)
                {
                    cs++;
                }

                i++;
            }

            return cs;
        }
    }

    public enum Operation
    {
        Encode,
        Decode
    }
}