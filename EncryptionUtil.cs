using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;
using PgpCore;

public static class EncryptionUtil
{

    /// <summary>
    /// Uses PgpCore and raw BouncyCastle to encrypt data in chunks using TripleDes.
    /// </summary>
    /// <param name="keys">Public key using PgpCore class.</param>
    /// <param name="inputStream">Data to encrypt.</param>
    /// <param name="outputStream">Encryption output.</param>
    /// <param name="bufferSize">Size of buffers to use for encryption.</param>
    /// <returns></returns>
    public static async Task EncryptAsync(EncryptionKeys keys, Stream inputStream, Stream outputStream, int bufferSize = 4096)
    {
        var pk = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.TripleDes, true, new SecureRandom());
        foreach (var publicKeyRing in keys.PublicKeyRings)
        {
            PgpPublicKey publicKey = publicKeyRing.PreferredEncryptionKey ?? publicKeyRing.DefaultEncryptionKey;
            pk.AddMethod(publicKey);
        }

        using var armoredStream = new ArmoredOutputStream(outputStream); // Friendlier file output.

        using (var @out = pk.Open(armoredStream, new byte[bufferSize]))
        {
            var lData = new PgpLiteralDataGenerator(false);
            using (var pOut = lData.Open(@out, PgpLiteralData.Binary, PgpLiteralData.Console, DateTime.Now, new byte[bufferSize]))
            {
                await inputStream.CopyToAsync(pOut);
            }
        }
    }
}