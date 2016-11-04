# The Grand Chase's Networking
This topic will cover the Grand Chase's networking by explaining the packets' structure, encryption, authentication, compression and so on. It's important to be aware of the aspects discussed here to have a good understanding of the networking of Grand Chase as well as the GCNet Library.
## **Summary**
* [The Overall Structure](https://github.com/syntax-dev-br/GCNet/blob/doc-test/docs/info/Packets.md#the-overall-structure)
* [The Encryption](https://github.com/syntax-dev-br/GCNet/blob/doc-test/docs/info/Packets.md#the-encryption)
* [The Payload](https://github.com/syntax-dev-br/GCNet/blob/doc-test/docs/info/Packets.md#the-payload)
* [The Beginning of the Session](https://github.com/syntax-dev-br/GCNet/blob/doc-test/docs/info/Packets.md#the-beginning-of-the-session)

## **The Overall Structure**
In Grand Chase, the packets are divided primarily into three sections: header, payload and authentication code. Let's explain them one by one.

> For demonstration purposes, we will be using the acknowledgement packet SHA_FILENAME_LIST (ID: 0x001C).

If we had sniffed this packet, its data would look like this:

> ![](http://i.imgur.com/zbJ7iV4.png)

Now let's analyze its parts:
### Header
> 6A 00 E7 8E 02 00 00 00 58 58 58 58 58 58 58 58

In all packets, it represents the first 16 bytes of the received *buffer*. It contains some basic informations about the packet, which will be explained in detail below.
> Note: all the data in the header is written in the [little-endian](https://en.wikipedia.org/wiki/Endianness#Little-endian) format.

___
***Size***
> ***6A 00*** E7 8E 02 00 00 00 58 58 58 58 58 58 58 58

As the name suggests, it represents the packet's buffer length. It is in little-endian format, so it's actually _00 6A_, which, in decimal, is _106_. If you count each byte of our sniffed packet's data, you will realize that it contains exactly 106 bytes. :smiley:
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

### Payload (encrypted)
> CD 05 A5 3D 7B 8C 1D CD 03 15 B1 DE 85 36 72 D9 1F B6 03 7D 77 5A 01 BE 78 D4 0A 22 EB 63 BB D1 77 D2 C6 9F DB 17 BC 0A E2 CF D8 75 B2 9E 2E 30 DD 24 3E AA 3E 5B 90 FE 61 F2 C2 D1 05 A7 1C FD 9E 1B 69 A3 76 CE 3A 9D 69 21 21 9B 82 D7 00 DF

Located between the 16 first (header) and the 10 last (auth code) bytes, this is the main part of the packet. At first sight, it's encrypted and doesn't reveal much, but when decrypted, contains the effective data, the one that tell us something relevant such as the login inputted by the user or the information of the players inside a dungeon room. Due to its importance, the payload will be discussed in its [own]() section and likewise will be the [encryption](https://github.com/syntax-dev-br/GCNet/blob/doc-test/docs/info/Packets.md#the-encryption).

### Authentication Code
> E3 57 33 57 A6 79 A3 F6 53 57

Represented by the last 10 bytes of the buffer, this is the portion of the packet which is meant to assure the authenticity of the rest. In Grand Chase, it consists in a [MD5](https://en.wikipedia.org/wiki/MD5)-[HMAC](https://en.wikipedia.org/wiki/Hash-based_message_authentication_code) (Hash-based Message Authentication Code). 
> 6A 00 ***E7 8E 02 00 00 00 58 58 58 58 58 58 58 58 CD 05 A5 3D 7B 8C 1D CD 03 15 B1 DE 85 36 72 D9 1F B6 03 7D 77 5A 01 BE 78 D4 0A 22 EB 63 BB D1 77 D2 C6 9F DB 17 BC 0A E2 CF D8 75 B2 9E 2E 30 DD 24 3E AA 3E 5B 90 FE 61 F2 C2 D1 05 A7 1C FD 9E 1B 69 A3 76 CE 3A 9D 69 21 21 9B 82 D7 00 DF*** E3 57 33 57 A6 79 A3 F6 53 57

The authentication code calculation is done based on the portion of the packet's buffer shown above (from the first byte after the packet size until the last byte of the encrypted payload). The calculation also takes an 8-byte auth key which is defined at the beginning of the networking session. It will be better detailed in the [last section]().

Normally, a MD5-HMAC would have a size of 16 bytes. But if we take a look at our packet's HMAC we will realize it's only 10 byte long. That's because the game truncates the hash to leave it with the size of ten bytes.
> ***E3 57 33 57 A6 79 A3 F6 53 57*** 10 17 F0 5F 40 F1

Above, you can see the entire HMAC (everything) compared to the part present in the packet (bold).

## **The Encryption**
> Note: for better understanding, it's highly recommended that you read the content of the links provided in this section.

The payloads of the Grand Chase's packets are encrypted using the [DES algorithm](https://en.wikipedia.org/wiki/Data_Encryption_Standard) through the [CBC mode](https://en.wikipedia.org/wiki/Block_cipher_mode_of_operation#Cipher_Block_Chaining_.28CBC.29) (Cipher Block Chaining mode). 

The encryption process, as well as the decryption, once DES is a symmetric key algorithm, takes an _IV_, a 8-byte _key_ and a _plaintext_.
* As pointed before, the IV is generated for each packet and is sent together with it;
* The encryption key is defined at the start of the session like the auth key;
* The plaintext is the unencrypted payload;

The data is processed in blocks of 8 bytes each. But what if the size of our data is not divisible by 8? That's what we will see below.
___
***Padding***
> 00 1C 00 00 00 40 00 00 00 00 03 00 00 00 0C 61 00 69 00 2E 00 6B 00 6F 00 6D 00 00 00 00 10 6D 00 61 00 69 00 6E 00 2E 00 65 00 78 00 65 00 00 00 00 14 73 00 63 00 72 00 69 00 70 00 74 00 2E 00 6B 00 6F 00 6D 00 00 00 00 ***00 01 02 03 04 04***

The whole thing you see above is the decrypted payload from our packet, but for now, let's limit ourselves only to the part in bold. This portion of the data is the _padding_. It serves to fill the data until it reaches a length divisible by block size (in our case, 8).

Let's now analyze our payload individually. Without padding, it would be 74 bytes long, but 74 is not divisible by 8. The next number divisible by 8 after 74 is 80, so our padding should be 6 bytes long (74 + _6_ = 80). Then we start to count: 00, 01, 02, 03, 04 and 04 again. The last byte of the padding is always equal to the penultimate byte. 

After this all, we now have ***00 01 02 03 04 04***: a 6-bytes long padding.
___
***The Padding Algorithm***

As you may have thought, it is impossible that the padding has the last byte equal to the penultimate if, for example, it is 1 byte long. Let's explain the algorithm a little better now.

It's actually very simple: when the distance from the payload length to the next number divisible by 8 is greater or equal to 3, the padding length will be this distance. When it is smaller, it will be the block size (8) plus the distance. After this, the only thing left is to write the bytes. This code snippet should be more enlightening:
```C#
// Calculates the distance between the length and the next value divisible by 8
int distance = 8 - (dataLength % 8);

if (distance >= 3)
{
  paddingLength = distance;
}
else
{
  paddingLength = 8 + distance;
}
for (byte i = 0; i < (paddingLength - 1); i++)
{
  padding[i] = i;
}
padding[paddingLength - 1] = padding[paddingLength - 2];
```
And here's a table with all the 8 possible paddings for the payloads of the game to kill any remaining doubt:

| Distance          | Padding Length | Padding Bytes                       |
| ----------------- | -------------- | ----------------------------------- |
| 0                 | 8              | ***00 01 02 03 04 05 06 06***       |
| 1                 | 9              | ***00 01 02 03 04 05 06 07 07***    |
| 2                 | 10             | ***00 01 02 03 04 05 06 07 08 08*** |
| 3                 | 3              | ***00 01 01***                      |
| 4                 | 4              | ***00 01 02 02***                   |
| 5                 | 5              | ***00 01 02 03 03***                |
| 6                 | 6              | ***00 01 02 03 04 04***             |
| 7                 | 7              | ***00 01 02 03 04 05 05***          |

## **The Payload**

What you see below is the decrypted payload of our packet (now with the padding removed).
```
00 1C 00 00 00 40 00 00 00 00 03 00 00 00 0C 61 00 69 00 2E 00 6B 00 6F 00 6D 00 00 00 00 10 6D 00 61 00 69 00 
6E 00 2E 00 65 00 78 00 65 00 00 00 00 14 73 00 63 00 72 00 69 00 70 00 74 00 2E 00 6B 00 6F 00 6D 00 00 00 00
```
As previously stated, it's the holder of the most important data in all the packet.

Like the packet buffer, the decrypted payload has its sections: the header, the content and the null bytes padding. Again, let's explain them one by one.
> Note: there are some exceptions to this division like the ping packet, whose payload contains only null bytes.

### Header
> 00 1C 00 00 00 40 00

The payload header contains three essential informations: packet ID, content size and compression flag. Next, we will take a closer look at these values.
> Note: unlike the header, all the data in the payload is written in the [big-endian](https://en.wikipedia.org/wiki/Endianness#Big-endian) format.

___
***ID***
> ***00 1C*** 00 00 00 40 00

The ID, as the name suggests, is the packet identifier. It indicates what the packet is meant for, what it is. 

For example, the packet with the ID 0x0001 is the packet in which the session keys are defined; the one with the ID 0x001C is the acknowledgement packet SHA_FILENAME_LIST, which serves to inform the client about the files which will be verified through SHA checksum.
___
***Content Size***
> 00 1C ***00 00 00 40*** 00

This is the size in bytes of the _content_ of the payload. In our case, it is _00 00 00 40_ in hex values, denoting that the size is _64_ in decimal. Check for yourself: count each byte from the 8th to the 4th last byte. Your count should reach 64.
___
***Compression Flag***
> 00 1C 00 00 00 40 ***00***

The _compression flag_ is a boolean value that indicates whether the content data is compressed or not. When it is _true_ (***01***), it means the data is compressed. Otherwise, it is _false_ (***00***) and indicates that the data is uncompressed, which is the case of our packet. The compression itself will be discussed soon.

### Content
> ![](http://image.prntscr.com/image/ec3c97561f4a427693a1a08e90f4ef5e.png)
  
Basically, the content is the message in its rawest state. It's actually the information that the packet really is intended to transmit.

Its structure will vary for each packet type, but there's yet one common "pattern". Taking the "main.exe" value as example, see what follows.

> ![](http://image.prntscr.com/image/276d51bc2b4e4b2e820c1abefad4ab21.png)
  
The portion marked in purple is an [unicode](https://en.wikipedia.org/wiki/Unicode) string that represents the filename _main.exe_. But what about the piece in red (_00 00 00 10_)? It is an 4-byte integer that represents the size of the following value. In decimal, _00 00 00 10_ is _16_, which is exactly our string's size in bytes.

But remember: there may be values that aren't preceded by its size!
### Null Bytes Padding
> 00 00 00

It is just an ordinary padding composed by three _00_ (null) bytes at the end of each payload.

### Compression

Some of the Grand Chase's packets have their payload content compressed. 

To compress the data, Grand Chase uses [zlib](https://en.wikipedia.org/wiki/Zlib). We can know this because of the presence of one of the zlib headers (_78 01_) in every payload which have its data compressed.

Let's take a look at one compressed payload.
> ![](http://image.prntscr.com/image/09858f6f18bb4cc9b597f7e884ae9576.png)

(As you can see in the bytes in red, the compression flag is _true_ and the zlib header is present)

Actually, only the highlighted portion is compressed: the header plus 4 bytes remains uncompressed, as well as the _00 00 00_ padding at the end.

After the data is decompressed, it "becomes a normal packet" and may be read normally.

## **The Beginning of the Session**

In this section we will be discussing something very important: the beginning of a nwtworking session of Grand Chase. More specifically, the packet where the keys are set up.

In Grand Chase, the encryption and auth keys for the session are randomly generated by the server and are sent to the client in the packet of ID 0x0001, which is the first packet with any information sent. Its data would look like this one:

> Note: the packet exposed here is from the season eternal. Haven't checked the packets from other seasons, but it may be different.

```
52 00 00 00 31 18 00 00 59 59 59 59 59 59 59 59 91 98 7C 57 C3 D1 13 CE 9C 97 AC 0C 31 B0 79 78 DC AF 50 F4 F1 B0
61 9F 95 E3 0F DE 22 32 19 6A 86 FA B6 28 12 F4 1A E8 BC 40 02 84 0E 19 BF C6 46 26 3E 96 FA 52 6B 64 A2 3C 82 90
2C 9E 32 BB DA 8D
```
First, let's talk about two things: the _prefix_ and the _count_.

* **Prefix**: in the first packet, the prefix isn't a random number. Instead, it is always _00 00_;
* **Count**: in the first packet, (obviously) the count does not measure the quantity of the packets sent.

> Note: _I still have my doubts about what the count represents in the first packet. It doesn't seem to be a random value. What I know at moment is that if it is 00 00 00 00, the client does not acknowledge the packet. It just a matter of time until I get a clearer idea about this, I just need to analyze some more samples of this type of packet._

You may be wondering: "If the session keys are inside the encrypted data, what keys were used to encrypt the packet and generate the auth code for the first packet?"

For this packet, Grand Chase uses two default keys which are stored by both client and server:

* **Default Encryption Key:** C7 D8 C4 BF B5 E9 C0 FD
* **Default Auth Key:** C0 D3 BD C3 B7 CE B8 B8

Having it explained, let's now analyze the packet payload.
> ![](http://i.imgur.com/nISFz3e.png?1)

It's like any other packet: it has a header, a content and 3 null bytes at the end. 

The highlighted values are the auth and encryption keys defined for the rest of networking session. In our case:

* **The part in purple is the authentication key**: C9 F8 7C 96 04 9C 1E BE
* **The part in red is the encryption key**: FA DE 9C F3 13 91 C8 38


And for now, that's all :smile:
