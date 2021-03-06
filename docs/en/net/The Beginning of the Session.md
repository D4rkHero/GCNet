# **The Beginning of the Session**

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
