# GCNet
GCNet is a Grand Chase networking library created by SyntaxDev which provides cryptography and packet-related features.
## Features
The **CoreLib** namespace provides some core features like packet encryption and decryption, packet authentication with HMAC and key generation.

The **PacketLib** namespace gives an I/O packet managing by providing compression, data reading and writing in addition to the own cryptography functions.
## Example
Here is a very simple code snippet showing off the ease of use of the GCNet Library.
```C#
InPacket packet = new InPacket(receivedData, cryptoSession); // Processes the data
PayloadReader reader = new PayloadReader(packet.PayloadData); // Creates a reader

// Now you can read the decrypted packet data
int a = reader.ReadInt32();
```
## Build
To build the project, you should use Visual Studio 2015 with .NET Framework 4.5.2.
