# The Grand Chase's Networking
This topic will cover the Grand Chase's networking by explaining the packets' structure, encryption, authentication, compression and so on. It's important to be aware of the aspects discussed here to have a good understanding of the networking of Grand Chase as well as the GCNet Library.
## **Summary**
* [The Overall Structure](https://github.com/syntax-dev-br/GCNet/blob/doc-test/docs/info/Packets.md#the-overall-structure)
* [The Encryption](https://github.com/syntax-dev-br/GCNet/blob/doc-test/docs/info/Packets.md#the-encryption)
* The Payload
* The "First" Packet

## **The Overall Structure**
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

Located between the 16 first (header) and the 10 last (auth code) bytes, this is the main part of the packet. At first sight, it's encrypted and doesn't reveal much, but when decrypted, contains the effective data, the one that tell us something relevant such as the login inputted by the user or the information of the players inside a dungeon room. Due to its importance, the payload will be discussed in its [own]() section and likewise will be the [encryption](https://github.com/syntax-dev-br/GCNet/blob/doc-test/docs/info/Packets.md#the-encryption).

### Authentication Code
> E3 57 33 57 A6 79 A3 F6 53 57

Represented by the last 10 bytes of the buffer, this is the portion of the packet which is meant to assure the authenticity of the rest. In Grand Chase, it consists in a [MD5](https://en.wikipedia.org/wiki/MD5)-[HMAC](https://en.wikipedia.org/wiki/Hash-based_message_authentication_code) (Hash-based Message Authentication Code). 
> 6A 00 ***E7 8E 02 00 00 00 58 58 58 58 58 58 58 58 CD 05 A5 3D 7B 8C 1D CD 03 15 B1 DE 85 36 72 D9 1F B6 03 7D 77 5A 01 BE 78 D4 0A 22 EB 63 BB D1 77 D2 C6 9F DB 17 BC 0A E2 CF D8 75 B2 9E 2E 30 DD 24 3E AA 3E 5B 90 FE 61 F2 C2 D1 05 A7 1C FD 9E 1B 69 A3 76 CE 3A 9D 69 21 21 9B 82 D7 00 DF*** E3 57 33 57 A6 79 A3 F6 53 57

The authentication code calculation is done based on the portion of the packet's buffer shown above (from the first byte after the packet size until the last byte of the encrypted payload). The calculation also takes an auth key which is defined at the beginning of the networking session. It will be better detailed in the [last section]().

Normally, a MD5-HMAC would have a size of 16 bytes. But if we take a look at our packet's HMAC we will realize it's only 10 byte long. That's because the game truncates the hash to leave it with the size of ten bytes.
> ***E3 57 33 57 A6 79 A3 F6 53 57*** 10 17 F0 5F 40 F1

Above, you can see the entire HMAC (everything) compared to the part present in the packet (bold).

## **The Encryption**
> Note: for better understanding, it's highly recommended that you read the content of the links provided in this section.

The payloads of the Grand Chase's packets are encrypted using the [DES algorithm](https://en.wikipedia.org/wiki/Data_Encryption_Standard) through the [CBC mode](https://en.wikipedia.org/wiki/Block_cipher_mode_of_operation#Cipher_Block_Chaining_.28CBC.29) (Cipher Block Chaining mode). 

The encryption process, as well as the decryption, once DES is a symmetric key algorithm, takes an _IV_, a _key_ and a _plaintext_.
* As pointed before, the IV is generated for each packet and is sent together with it;
* The encryption key is defined at the start of the session;
* The plaintext is the unencrypted payload;

The data is processed in blocks of 8 bytes each. But what if the size of our data is not divisible by 8? That's what we will see below.

***Padding***
> 00 1C 00 00 00 40 00 00 00 00 03 00 00 00 0C 61 00 69 00 2E 00 6B 00 6F 00 6D 00 00 00 00 10 6D 00 61 00 69 00 6E 00 2E 00 65 00 78 00 65 00 00 00 00 14 73 00 63 00 72 00 69 00 70 00 74 00 2E 00 6B 00 6F 00 6D 00 00 00 00 ***00 01 02 03 04 04***

The whole thing you see above is the decrypted payload from our packet, but for now, let's limit ourselves only to the part in bold. This portion of the data is the _padding_. It serves to fill the data until it reaches a length divisible by block size (in our case, 8).

Let's now analyze our payload individually. Without padding, it would be 74 bytes long, but 74 is not divisible by 8. The next number divisible by 8 after 74 is 80, so our padding should be 6 bytes long (74 + _6_ = 80). Then we start to count: 00, 01, 02, 03, 04 and 04 again. The last byte of the padding is always equal to the penultimate byte. 

After this all, we now have ***00 01 02 03 04 04***: a 6-bytes long padding.

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
for (byte i = 0; i < paddingLength; i++)
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
