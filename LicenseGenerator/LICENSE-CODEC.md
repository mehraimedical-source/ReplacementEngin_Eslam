# Compact license codec

`LicenseCodec.cs` targets the existing .NET Framework 2.0 project. `LicenseInfo`
has exactly the requested `ProductID` and `Cout` properties (the spelling is intentional).

```csharp
// Provision once. Store this key in protected issuer/server storage and load it thereafter.
byte[] secretKey = LicenseCodec.GenerateSecretKey();
LicenseInfo info = new LicenseInfo { ProductID = 3, Cout = 250 };
string code = LicenseCodec.Code(info, secretKey);
LicenseInfo decoded = LicenseCodec.Decode(code, secretKey, 3);
// decoded.ProductID == 3, decoded.Cout == 250
```

`Code` emits 16 Crockford Base32 characters in `XXXX-XXXX-XXXX-XXXX` format.
`Decode` accepts that format or 16 compact characters, case insensitively, with
optional surrounding whitespace. It rejects invalid characters, bad formatting,
invalid fields, wrong keys, and invalid authentication tags. The overload with
`expectedProductID` also rejects a license for another product.

## Limits and security

- ProductID: 1..31; Cout: 1..2047. Values outside these ranges are rejected.
- Layout: product 5 bits, count 11 bits, random nonce 24 bits, HMAC-SHA256 tag 40 bits.
- This is authenticated encoding, not encryption. Product and count can be read
  by anyone who knows the format. Randomness does not conceal these fields.
- Each independent tag guess has probability 1/2^40 of succeeding. The tag is
  deliberately short to fit 16 characters; this is not strong offline licensing.
- Keep the random 32-byte shared secret on the issuer/server. Shipping it in a
  desktop client allows an attacker who extracts it to generate valid codes.
  Call Decode on a trusted server and rate-limit attempts if using this format
  for activation. Strong offline validation requires asymmetric signatures and
  a longer license or a signed license file. Even 25..32 Base32 characters are
  not enough for a conventional secure public-key signature plus this payload.
- The nonce does not guarantee uniqueness. Licenses are reusable bearer codes;
  there is no device binding, expiry, revocation, or activation counter here.
  Cout is a signed entitlement value, not an enforced consumption counter.
- Persist the key securely. Generating a new key each launch invalidates old
  codes. No production key is embedded in the source or committed to GitHub.

## Tests

`LicenseCodec.Tests.cs` is a standalone console harness, intentionally excluded
from the WinForms project. On a compatible Windows developer environment:

```bat
csc /out:LicenseCodec.Tests.exe LicenseCodec.cs LicenseCodec.Tests.cs
LicenseCodec.Tests.exe
```

It covers a known vector, boundary round trips, lowercase and compact formats,
wrong secret, wrong product, malformed inputs, out-of-range fields, and every
single-character mutation of the known vector.

Known public test key: bytes 0..31; product 3; count 250; nonce 0x123456.
Expected code: `33X1-4D2P-BAAH-64FM`. Never use this public key in production.

Validation performed in the creation environment: an independent Python model
checked all 63,457 allowed product/count combinations and the known vector.
The C# harness and the WinForms build were not run: no C#/.NET compiler was available.
