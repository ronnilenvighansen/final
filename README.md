# final

How i use cryptography:

E2EE encryption with hybrid cryptographic approach.
RSA for key exchange.
Each client generates a public and private RSA key pair. 
Client sends public key to server.
Client encrypts randomly generated AES with HMAC key using the public RSA key. 
Only receiver can decrypt the AES sessions key with their private RSA key.

This prevents third parties/server from intercepting and reading shared session key.

AES-CBC for message encryption.
A random IV is generated per message.

AES-CBC + RSA for confidentiality.

HMAC for integrity.

Why i chose to do it this way:

RSA is good for exchanging keys, not efficient for large data.

AES-CBC is efficient and fast for encrypting the actual message.

HMAC adds integrity without complicating the encryption.

Instructions on how to use the app:

Download the full "final folder."

After downloading, run this: 
dotnet dev-certs https --trust

Open 3 terminals.

Write the following commands.

Terminal 1:
cd SecureChat

Terminal 2:
cd ChatClient

Terminal 3:
cd ChatClient

Then run the app in all terminals with:
dotnet run

Then just follow the instructions on the CLI.