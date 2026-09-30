using System;
using System.Security.Cryptography;
using System.Text;

namespace LicenseGenerator
{
    public sealed class LicenseInfo
    {
        public int ProductID { get; set; }
        public int Cout { get; set; }
    }

    // Server/issuer only: do not ship the shared secret in customer software.
    // 5-bit product, 11-bit count, 24-bit nonce, 40-bit truncated HMAC.
    public static class LicenseCodec
    {
        private const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
        // Framework 2.0 RNG has no IDisposable contract. Reuse one provider for
        // the application lifetime and serialize access for thread safety.
        private static readonly RandomNumberGenerator SecureRandom = RandomNumberGenerator.Create();
        private static readonly object RandomLock = new object();
        private static readonly byte[] Domain = Encoding.ASCII.GetBytes("LicenseGenerator/v1:");

        // Generate once, then store securely and reuse. Rotating it invalidates old codes.
        public static byte[] GenerateSecretKey()
        {
            byte[] key = new byte[32];
            FillRandomBytes(key);
            return key;
        }

        public static string Code(LicenseInfo info, byte[] secretKey)
        {
            ValidateKey(secretKey);
            if (info == null) throw new ArgumentNullException("info");
            ValidateFields(info.ProductID, info.Cout);

            byte[] bytes = new byte[10];
            int fields = (info.ProductID << 11) | info.Cout;
            bytes[0] = (byte)(fields >> 8);
            bytes[1] = (byte)fields;
            byte[] nonce = new byte[3];
            FillRandomBytes(nonce);
            Buffer.BlockCopy(nonce, 0, bytes, 2, 3);
            byte[] tag = ComputeTag(bytes, secretKey);
            Buffer.BlockCopy(tag, 0, bytes, 5, 5);

            StringBuilder result = new StringBuilder(19);
            for (int i = 0; i < 16; i++)
            {
                if (i > 0 && i % 4 == 0) result.Append('-');
                int value = 0;
                for (int j = 0; j < 5; j++)
                {
                    int bit = i * 5 + j;
                    value = (value << 1) | ((bytes[bit / 8] >> (7 - bit % 8)) & 1);
                }
                result.Append(Alphabet[value]);
            }
            return result.ToString();
        }

        // Authenticate before returning the decoded properties.
        public static LicenseInfo Decode(string code, byte[] secretKey)
        {
            ValidateKey(secretKey);
            if (code == null) throw new ArgumentNullException("code");
            string compact = code.Trim().ToUpperInvariant();
            if (compact.Length == 19)
            {
                if (compact[4] != '-' || compact[9] != '-' || compact[14] != '-')
                    throw new FormatException("Invalid license format.");
                compact = compact.Replace("-", "");
            }
            if (compact.Length != 16) throw new FormatException("Expected 16 characters.");

            byte[] bytes = new byte[10];
            for (int i = 0; i < compact.Length; i++)
            {
                int value = Alphabet.IndexOf(compact[i]);
                if (value < 0) throw new FormatException("Invalid license character.");
                for (int j = 0; j < 5; j++)
                {
                    int bit = i * 5 + j;
                    bytes[bit / 8] |= (byte)(((value >> (4 - j)) & 1) << (7 - bit % 8));
                }
            }
            byte[] tag = ComputeTag(bytes, secretKey);
            int difference = 0;
            for (int i = 0; i < 5; i++) difference |= bytes[5 + i] ^ tag[i];
            if (difference != 0) throw new CryptographicException("Invalid license.");

            int fields = (bytes[0] << 8) | bytes[1];
            int product = fields >> 11;
            int count = fields & 2047;
            ValidateFields(product, count);
            return new LicenseInfo { ProductID = product, Cout = count };
        }

        // Enforce product binding when authorizing a particular application.
        public static LicenseInfo Decode(string code, byte[] secretKey, int expectedProductID)
        {
            if (expectedProductID < 1 || expectedProductID > 31)
                throw new ArgumentOutOfRangeException("expectedProductID");
            LicenseInfo info = Decode(code, secretKey);
            if (info.ProductID != expectedProductID)
                throw new CryptographicException("License belongs to another product.");
            return info;
        }

        private static void FillRandomBytes(byte[] buffer)
        {
            lock (RandomLock)
                SecureRandom.GetBytes(buffer);
        }

        private static byte[] ComputeTag(byte[] bytes, byte[] key)
        {
            byte[] message = new byte[Domain.Length + 5];
            Buffer.BlockCopy(Domain, 0, message, 0, Domain.Length);
            Buffer.BlockCopy(bytes, 0, message, Domain.Length, 5);
            using (HMACSHA256 hmac = new HMACSHA256(key))
                return hmac.ComputeHash(message);
        }

        private static void ValidateKey(byte[] key)
        {
            if (key == null) throw new ArgumentNullException("secretKey");
            if (key.Length != 32) throw new ArgumentException("Use a random 32-byte secret key.", "secretKey");
        }

        private static void ValidateFields(int product, int count)
        {
            if (product < 1 || product > 31) throw new ArgumentOutOfRangeException("ProductID", "Expected 1..31.");
            if (count < 1 || count > 2047) throw new ArgumentOutOfRangeException("Cout", "Expected 1..2047.");
        }
    }
}
