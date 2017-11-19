using System.Collections.Generic;

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
            List<byte> bytes = ToUnverifiedBytes();
            byte checksum = GetCheckSum(bytes);
            bytes.Add(checksum);

            return bytes;
        }       

        // Get checksum for the passed bytes
        public static byte GetCheckSum(List<byte> bytes)
        {
            byte cs = 0;
            int i = 0;

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

    // Enumeration representing various operation commands
    public enum Operation
    {
        Encode,
        Decode,
        Update,
        Clear
    }
}