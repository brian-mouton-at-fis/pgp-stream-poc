// Generate keys...

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


Console.WriteLine("Generating keys...");

var publicKeyFileName = "public.asc";
var privateKeyFileName = "private.asc";
var numberOfRandomLines = 10000000;

using (var pgpGenerate = new PGP())
{
    pgpGenerate.GenerateKey(new FileInfo(publicKeyFileName), new FileInfo(privateKeyFileName), "brian.mouton@fisglobal.com", "password");
}

ReportMemoryUsage();

//Encryption in chunks...
Console.WriteLine("Encrypting random data...");

EncryptionKeys keys;

using (var publicKeyStream = new FileStream(publicKeyFileName, FileMode.Open))
    keys = new EncryptionKeys(publicKeyStream);


var randomDataStream = new RandomDataStream(numberOfRandomLines);
using (var storageStream = new FileStream("encryptionStream.txt", FileMode.Create)) // Imagine this is an output or custom stream to write to an S3 bucket.
{
    using (var pgpEncrypt = new PGP(keys))
    {
        pgpEncrypt.EncryptStream(randomDataStream, storageStream);
    }

    ReportMemoryUsage();
}

// Decryption in chunks...
Console.WriteLine("Decrypting data...");

using (var storageStream = new FileStream("encryptionStream.txt", FileMode.Open))
using (var decryptionStream = new FileStream("decryptionStream.txt", FileMode.Create))
using (var privateKeyStream = new FileStream(privateKeyFileName, FileMode.Open))
using (var pgpDecrypt = new PGP(new EncryptionKeys(privateKeyStream, "password")))
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