# The Grand Chase's Networking
This topic will cover the Grand Chase's networking by explaining the packets' structure, encryption, authentication, compression and so on. It's important to be aware of the aspects discussed here to have a good understanding of the networking of Grand Chase as well as the GCNet Library.
## Summary
* The Overall Structure
* The Encryption
* The Payload

## The Overall Structure
In Grand Chase, the packets are divided primarily into three sections: header, payload and authentication code. Let's explain them one by one.

> For demonstration purposes, we will be using the SHA_FILENAME_LIST acknowledge packet (ID: 0x001C).

If we had sniffed this packet, its raw data would look like this:
```
6A 00 E7 8E 02 00 00 00 58 58 58 58 58 58 58 58 CD 05 A5 3D 7B 8C 1D CD 03 15 B1 DE 85 36 72 D9 1F B6 03 7D 77
5A 01 BE 78 D4 0A 22 EB 63 BB D1 77 D2 C6 9F DB 17 BC 0A E2 CF D8 75 B2 9E 2E 30 DD 24 3E AA 3E 5B 90 FE 61 F2
C2 D1 05 A7 1C FD 9E 1B 69 A3 76 CE 3A 9D 69 21 21 9B 82 D7 00 DF E3 57 33 57 A6 79 A3 F6 53 57
```

Now let's analyze its parts:
### Header
> 6A 00 E7 8E 02 00 00 00 58 58 58 58 58 58 58 58 

In all packets, it represents the first 16 bytes of the received *buffer*. It contains some basic informations about the packet, which will be explained in detail below.
> Note: all the data in the header is written in the [little-endian](https://en.wikipedia.org/wiki/Endianness#Little-endian) format.

***Size***

> ***6A 00*** E7 8E 02 00 00 00 58 58 58 58 58 58 58 58 

As the name suggests, it represents the packet's buffer length. It is in little-endian format, so it's actually *00 6A*, which, in decimal, is 106. If you count each byte of our sniffed packet's data, you will realize that it contains exactly 106 bytes. :smiley:
***Prefix*** 

> 6A 00 ***E7 8E*** 02 00 00 00 58 58 58 58 58 58 58 58

We're now faced with the *prefix*. These 2 bytes are present in all the packets and contains a random value, with exception of the packet in which the session keys are defined, where the prefix is represented by *00 00*.
