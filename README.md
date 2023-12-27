So far the key class is LineInterceptor which allows decrypting a file and processing it line by line without consuming much additional memory in the form of buffering.

Attempting to read a ~500 MB file full of guid's and provide a status update resulted in an increase from 167.1875 MB to 167.296875 MB.