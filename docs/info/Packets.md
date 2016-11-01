# The Grand Chase's Networking
This topic will cover the Grand Chase's networking by explaining the packets' structure, encryption, authentication, compression and so on. It's important to be aware of the aspects discussed here to have a good understanding of the networking of Grand Chase as well as the GCNet Library.
## Summary
* The Overall Structure
* The Encryption
* The Payload
* The "First" Packet

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

As the name suggests, it represents the packet's buffer length. It is in little-endian format, so it's actually _00 6A_, which, in decimal, is 106. If you count each byte of our sniffed packet's data, you will realize that it contains exactly 106 bytes. :smiley:
___
***Prefix***
> 6A 00 ***E7 8E*** 02 00 00 00 58 58 58 58 58 58 58 58

We're now faced with the *prefix*. These 2 bytes are present in all the packets and contains a random value which is generated at the beginning of the session and used for all the following packets. There's only one exception: the packet in which the session keys are defined, where the prefix is represented by _00 00_ (this packet will be particularly discussed later).
___
***Count***
> 6A 00 E7 8E ***02 00 00 00*** 58 58 58 58 58 58 58 58

It's a 32-bit integer that represents the count of sent packets within a session. In our case, the packet count is 2 since it's _00 00 00 02_. Note that both client and server have their own counts. Like the prefix, the count has as exception the packet in which are defined the keys for the session.
___
***IV (Initialization Vector)***
> 6A 00 E7 8E 02 00 00 00 ***58 58 58 58 58 58 58 58***

It's the IV used to encrypt the packet's payload. Each packet has its own generated IV, which consists on 8 bytes equal ranging from _00_ to _FF_ in hex values. You should take a look at the [encryption section]() to have a better understanding of this concept.
___
### Payload (encrypted)
> CD 05 A5 3D 7B 8C 1D CD 03 15 B1 DE 85 36 72 D9 1F B6 03 7D 77 5A 01 BE 78 D4 0A 22 EB 63 BB D1 77 D2 C6 9F DB 17 BC 0A E2 CF D8 75 B2 9E 2E 30 DD 24 3E AA 3E 5B 90 FE 61 F2 C2 D1 05 A7 1C FD 9E 1B 69 A3 76 CE 3A 9D 69 21 21 9B 82 D7 00 DF

This is the main part of the packet. At first sight, it's encrypted and doesn't reveal much, but when decrypted, contains the effective data, the one that tell us something relevant such as the login inputted by the user or the information of the players inside a dungeon room. Due to its importance, the payload will be discussed in its [own]() section and likewise will be the [encryption]().

### Authentication Code
> E3 57 33 57 A6 79 A3 F6 53 57

This is the portion of the packet which is meant to assure the authenticity of the rest. In Grand Chase, it consists in a [MD5](https://en.wikipedia.org/wiki/MD5)-[HMAC](https://en.wikipedia.org/wiki/Hash-based_message_authentication_code) (Hash-based Message Authentication Code). 
> 6A 00 ***E7 8E 02 00 00 00 58 58 58 58 58 58 58 58 CD 05 A5 3D 7B 8C 1D CD 03 15 B1 DE 85 36 72 D9 1F B6 03 7D 77 5A 01 BE 78 D4 0A 22 EB 63 BB D1 77 D2 C6 9F DB 17 BC 0A E2 CF D8 75 B2 9E 2E 30 DD 24 3E AA 3E 5B 90 FE 61 F2 C2 D1 05 A7 1C FD 9E 1B 69 A3 76 CE 3A 9D 69 21 21 9B 82 D7 00 DF*** E3 57 33 57 A6 79 A3 F6 53 57

The authentication code calculation is done based on the portion of the packet's buffer shown above (from the first byte after the packet size until the last byte of the encrypted payload). The calculation also takes an auth key which is defined at the beginning of the networking session. It will be better detailed in the [last section]().

Normally, a MD5-HMAC would have a size of 16 bytes. But if we take a look at our packet's HMAC we will realize it's only 10 byte long. That's because the game truncates the hash to leave it with the size of ten bytes.
> ***E3 57 33 57 A6 79 A3 F6 53 57*** 10 17 F0 5F 40 F1

Above, you can see the entire HMAC (everything) compared to the part present in the packet (bold).

## The Encryption
Under construction!
