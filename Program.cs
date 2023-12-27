using System.Diagnostics;
using PgpCore;

var process = Process.GetCurrentProcess();
long lastMemoryUsage = process.PrivateMemorySize64;
Console.WriteLine($"Starting memory: {(double)process.PrivateMemorySize64 / 1024 / 1024} MB");

void ReportMemoryUsage()
{
    process.Refresh();
    Console.WriteLine($"Memory usage: {(double)process.PrivateMemorySize64 / 1024 / 1024} MB");
    Console.WriteLine($"\tDelta: {(double)(process.PrivateMemorySize64 - lastMemoryUsage) / 1024 / 1024} MB\n");
    lastMemoryUsage = process.PrivateMemorySize64;
}

// Generate keys...
Console.WriteLine("Generating keys...");

var publicKeyFileName = "public.asc";
var privateKeyFileName = "private.asc";
var password = "password";
var encrypteFileName = "encryptionStream.txt";
var decryptedFileName = "decryptionStream.txt";
var numberOfRandomLines = 1000000;

using (var pgpGenerate = new PGP())
{
    pgpGenerate.GenerateKey(new FileInfo(publicKeyFileName), new FileInfo(privateKeyFileName), "brian.mouton@fisglobal.com", password);
}

ReportMemoryUsage();

//Encryption in chunks...
Console.WriteLine("Encrypting random data...");

EncryptionKeys keys;

using (var publicKeyStream = new FileStream(publicKeyFileName, FileMode.Open))
    keys = new EncryptionKeys(publicKeyStream);

// using (var storageStream = new FileStream(encrypteFileName, FileMode.Create)) // Imagine this is an output or custom stream to write to an S3 bucket.
// {
//     using (var pgpEncrypt = new PGP(keys))
//     {
//         pgpEncrypt.EncryptStream(new RandomDataStream(numberOfRandomLines), storageStream);
//     }

//     ReportMemoryUsage();
// }

using (var storageStream = new FileStream(encrypteFileName, FileMode.Create)) // Imagine this is an output or custom stream to write to an S3 bucket.
{
    await EncryptionUtil.EncryptAsync(keys, new GuidStream(numberOfRandomLines), storageStream, 4096);
    ReportMemoryUsage();
}

// Decryption in chunks...
Console.WriteLine("Decrypting data...");

using (var storageStream = new FileStream(encrypteFileName, FileMode.Open))
using (var decryptionStream = new FileStream(decryptedFileName, FileMode.Create))
using (var privateKeyStream = new FileStream(privateKeyFileName, FileMode.Open))
using (var pgpDecrypt = new PGP(new EncryptionKeys(privateKeyStream, password)))
{
    int linesProcessed = 0;
    pgpDecrypt.DecryptStream(storageStream, new LineInterceptor(line =>
    {
        if (linesProcessed % 100000 == 0)
        {
            Console.WriteLine($"Lines processed: {linesProcessed}");
            ReportMemoryUsage();
        }

        // var decodedLine = System.Text.Encoding.UTF8.GetString(line.ToArray());
        decryptionStream.Write(line.GetBuffer(), 0, (int)line.Length);
        linesProcessed++;
    }));
}

ReportMemoryUsage();