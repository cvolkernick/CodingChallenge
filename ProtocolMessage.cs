using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingChallengeV2Client
{
    abstract class ProtocolMessage
    {
        public byte GetCheckSum()
        {
            return ProtocolMessage.GetCheckSum(this.ToUnverifiedBytes());
        }

        public List<byte> ToVerifiedBytes()
        {
            var byteArray = ToUnverifiedBytes();
            var checksum = GetCheckSum(byteArray);
            byteArray.Add(checksum);

            return byteArray;
        }

        protected abstract List<byte> ToUnverifiedBytes();

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
}