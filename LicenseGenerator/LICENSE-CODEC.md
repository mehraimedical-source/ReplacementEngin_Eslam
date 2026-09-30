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

## WinForms generator

The form provides ProductID/Cout numeric inputs, Code, an editable license field,
Copy, Decode, and separate read-only decoded ProductID/Cout fields. Decode
inspects the product encoded in the license independently of the generation
inputs. Invalid or edited input clears previous decoded results.

On first launch the form creates a random issuer key. Later launches reuse it
from %LOCALAPPDATA%/LicenseGenerator/issuer-key.dat. The key file is protected
with Windows DPAPI CurrentUser; no secret is displayed or committed. Save Key
exports a protected backup; Load Key imports and persists it, asking before
switching to a different key. Backups should be restored on the same Windows
computer/account, not treated as portable keys. Back up the key before deleting
the application data or resetting the Windows account. If an existing key file
cannot be read, generation stays disabled rather than silently rotating it.

This form is an issuer/admin tool. Do not distribute it or its issuer key with
customer products.

Static checks verified event-handler wiring, control initialization and parent
assignment, codec calls, clearing of decode results, and the System.Security
assembly reference required by DPAPI. Windows UI execution, DPAPI round trips,
and a C# build remain unverified in this environment.

Manual Windows checks:
1. Build and launch, generate ProductID=3/Cout=250, then Decode and verify both fields.
2. Copy the code, close and reopen, paste and Decode to check persisted key reuse.
3. Edit one character and Decode; results must clear and an error must appear.
4. Save Key and Load Key under the same Windows account; previous codes stay valid.
5. Check boundaries ProductID=1/31 and Cout=1/2047 and inspect at normal/high DPI.
