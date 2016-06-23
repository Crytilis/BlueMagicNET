using System;
using System.Globalization;

namespace BlueMagic.Memory
{
    public class Signature : IDisposable
    {
        public string String { get; private set; }
        public byte[] Bytes { get; private set; }

        public Signature(string signature)
        {
            String = string.Join("", signature.ToUpper().Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
            if (String.Length % 2 != 0)
                throw new ArgumentException("Signature contains an invalid number of characters.");

            bool wildcard = false;
            foreach (char c in String)
            {
                if ((c < '0' || c > '9') &&
                    (c < 'A' || c > 'F') &&
                    (c != '?'))
                    throw new ArgumentException("Signature contains invalid character(s).");

                if (c == '?')
                    wildcard = true;
            }

            if (!wildcard)
            {
                Bytes = new byte[String.Length / 2];
                for (int i = 0, j = 0; j < String.Length; ++i, j += 2)
                    Bytes[i] = byte.Parse(String.Substring(j, 2), NumberStyles.HexNumber);
            }
        }

        ~Signature()
        {
            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
