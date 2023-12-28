The purpose of this proof of concept (PoC) is to find a way to encrypt and decrypt files in chunks allowing the processing of VERY large files without running out of memory.

For encryption `VirtualLineStream` offers up a simplified way of providing lines for encryption.
EncryptionUtils includes a helper method for encrypting an input stream (like `VirtualLineStream`) to any output stream.

For decryption this PoC uses the `LineInterceptor` class to intercept decrypted lines of text and provide an oppurtunity to process them.

Encrypting a file with 1 million guid's (~37 MB) using PgpCore yields the following results:

```
Starting memory: 72.1640625 MB
Generating keys...
Memory usage: 84.1640625 MB
        Delta: 12 MB

Encrypting random data...
Memory usage: 164.2421875 MB
        Delta: 80.078125 MB

Decrypting data...
Lines processed: 0
Memory usage: 164.2421875 MB
        Delta: 0 MB

...

Lines processed: 800000
Memory usage: 163.6328125 MB
        Delta: -0.65625 MB

Lines processed: 900000
Memory usage: 163.6328125 MB
        Delta: 0 MB

Memory usage: 163.6328125 MB
        Delta: 0 MB
```

Encrypting a file with 1 million using BouncyCastle directly yields the following results:

```
Starting memory: 72.1640625 MB
Generating keys...
Memory usage: 84.03515625 MB
        Delta: 11.87109375 MB

Encrypting random data...
Memory usage: 189.234375 MB
        Delta: 105.19921875 MB

Decrypting data...
Lines processed: 0
Memory usage: 189.41796875 MB
        Delta: 0.18359375 MB

...

Lines processed: 800000
Memory usage: 189.58984375 MB
        Delta: 0 MB

Lines processed: 900000
Memory usage: 189.58984375 MB
        Delta: 0 MB

Memory usage: 189.58984375 MB
        Delta: 0 MB
```

As you can see there isn't much a of difference in size. In fact processing in chunks uses more memory.
I have attempted to increase the buffer sizes used in BouncyCastle and it has resulted in more memory use.

Armored output makes the encrypted files more easily readable, however, they do take up more space.

Critical files to look at are [VirtualLineStream.cs](VirtualLineStream.cs), [EncryptionUtil.cs](EncryptionUtil.cs), and [LineInterceptor.cs](LineInterceptor.cs).