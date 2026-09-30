using System;
using System.Security.Cryptography;
using LicenseGenerator;

// Standalone console test: csc /out:LicenseCodec.Tests.exe LicenseCodec.cs LicenseCodec.Tests.cs
internal static class LicenseCodecTests
{
    private static void Main()
    {
        byte[] key = new byte[32]; // Public test key only; never use in production.
        for (int i = 0; i < key.Length; i++) key[i] = (byte)i;
        string vector = "33X1-4D2P-BAAH-64FM";
        LicenseInfo decoded = LicenseCodec.Decode(vector, key, 3);
        if (decoded.Cout != 250) throw new Exception("Known vector failed.");
        int[] counts = { 1, 250, 2047 };
        for (int product = 1; product <= 31; product++)
            foreach (int count in counts)
            {
                string code = LicenseCodec.Code(new LicenseInfo { ProductID = product, Cout = count }, key);
                decoded = LicenseCodec.Decode(code.ToLowerInvariant(), key, product);
                if (decoded.ProductID != product || decoded.Cout != count || code.Length != 19)
                    throw new Exception("Round trip failed.");
                decoded = LicenseCodec.Decode(code.Replace("-", ""), key);
                if (decoded.Cout != count) throw new Exception("Compact input failed.");
            }
        byte[] wrong = (byte[])key.Clone(); wrong[0] ^= 1;
        MustReject(delegate { LicenseCodec.Decode(vector, wrong); });
        MustReject(delegate { LicenseCodec.Decode(vector, key, 2); });
        MustReject(delegate { LicenseCodec.Decode("123", key); });
        MustReject(delegate { LicenseCodec.Decode("IIII-IIII-IIII-IIII", key); });
        MustReject(delegate { LicenseCodec.Code(new LicenseInfo { ProductID = 0, Cout = 1 }, key); });
        MustReject(delegate { LicenseCodec.Code(new LicenseInfo { ProductID = 1, Cout = 2048 }, key); });
        const string alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
        for (int i = 0; i < vector.Length; i++)
        {
            if (vector[i] == '-') continue;
            foreach (char replacement in alphabet)
            {
                if (replacement == vector[i]) continue;
                char[] chars = vector.ToCharArray(); chars[i] = replacement;
                string altered = new string(chars);
                MustReject(delegate { LicenseCodec.Decode(altered, key); });
            }
        }
        Console.WriteLine("All license codec tests passed.");
    }

    private delegate void TestAction();
    private static void MustReject(TestAction action)
    {
        try { action(); }
        catch (ArgumentException) { return; }
        catch (FormatException) { return; }
        catch (CryptographicException) { return; }
        throw new Exception("Invalid input was accepted.");
    }
}
