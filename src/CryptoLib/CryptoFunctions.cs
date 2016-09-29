﻿//-----------------------------------------------------------------------
// GCNet - A Grand Chase Networking Library
// Copyright © 2016  SyntaxDev
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//-----------------------------------------------------------------------

using System;
using System.Linq;
using System.Security.Cryptography;

namespace GCNet.CryptoLib
{
    /// <summary>
    /// Provides the basic cryptographic functions used in Grand Chase networking
    /// </summary>
    public static class CryptoFunctions
    {
        /// <summary>
        /// Gets the data to be encrypted and returns the encrypted data
        /// </summary>
        /// <param name="data">Packet data to be encrypted</param>
        /// <param name="key">DES encryption key</param>
        /// <param name="IV">Initialization vector</param>
        public static byte[] EncryptPacket(byte[] data, byte[] key, byte[] IV)
        {
            using (DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider())
            {
                desProvider.Mode = CipherMode.CBC;
                desProvider.Padding = PaddingMode.None;

                using (ICryptoTransform encryptor = desProvider.CreateEncryptor(key, IV))
                {
                    PadPacket(ref data);
                    return encryptor.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }

        /// <summary>
        /// Gets the received packet data and returns the decrypted data
        /// </summary>
        /// <param name="packetBuffer">The packet data the way it was received</param>
        /// <param name="key">DES Encryption Key</param>
        /// <param name="IV">Initialization vector</param>
        public static byte[] DecryptPacket(byte[] packetBuffer, byte[] key)
        {
            byte[] IV = Util.ReadBytes(packetBuffer, 8, 8);

            using (DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider())
            {
                desProvider.Mode = CipherMode.CBC;
                desProvider.Padding = PaddingMode.None;

                using (ICryptoTransform decryptor = desProvider.CreateDecryptor(key, IV))
                {
                    byte[] rawData = decryptor.TransformFinalBlock(packetBuffer, 16,
                        packetBuffer.Length - CryptoConstants.GC_HMAC_SIZE - 16);

                    return Util.ReadBytes(rawData, 0, (rawData.Length - (rawData[rawData.Length - 1] + 2)));
                }
            }
        }

        /// <summary>
        /// Receives the packet without size and HMAC hash and returns the packet ready to be sent
        /// </summary>
        /// <param name="data">Packet data (excluding the packet size and the HMAC hash)</param>
        /// <param name="hmacKey">HMAC Key</param>
        public static byte[] ClearPacket(byte[] data, byte[] hmacKey)
        {
            byte[] packetSize = new byte[2];
            byte[] hmac = new byte[10];

            packetSize = BitConverter.GetBytes((short)(2 + data.Length + 10));
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(packetSize);
            }

            hmac = CryptoGenerators.GenerateHmac(data, hmacKey);
            // Concatenates size + (prefix + IV + encrypted data) + hmac, composing the assembled packet
            return Util.ConcatBytes(packetSize, Util.ConcatBytes(data, hmac));
        }

        /// <summary>
        /// Verifies whether the packet is valid or not through the HMAC hash
        /// </summary>
        /// <param name="packetBuffer">The packet data the way it was received</param>
        /// <param name="hmacKey"></param>
        public static bool CheckHmac(byte[] packetBuffer, byte[] hmacKey)
        {
            using (HMACMD5 hmac = new HMACMD5(hmacKey))
            {
                // Gets the total data size from the packet buffer
                short totalSize = BitConverter.ToInt16(BitConverter.IsLittleEndian ?
             Util.ReadBytes(packetBuffer, 0, 2) : Util.ReadBytes(packetBuffer, 0, 2).Reverse().ToArray(), 0);

                short dataSize = (short)(totalSize - 2 - CryptoConstants.GC_HMAC_SIZE);

                byte[] computedHash = Util.ReadBytes(hmac.ComputeHash(packetBuffer, 2, dataSize),
                    0, CryptoConstants.GC_HMAC_SIZE);

                byte[] storedHash = Util.ReadBytes(packetBuffer,
                    totalSize - CryptoConstants.GC_HMAC_SIZE,
                    CryptoConstants.GC_HMAC_SIZE);

                // Compares both hashes to check the integrity
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i])
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Performs the data padding in the encryption process
        /// </summary>
        /// <param name="packetBuffer">Data to be encrypted</param>
        private static void PadPacket(ref byte[] packetBuffer)
        {
            int paddingLength = 8 + ((8 - packetBuffer.Length % 8) % 8);
            byte[] paddingBytes = new byte[paddingLength];

            int i = 0;
            while (i < (paddingLength - 1))
            {
                paddingBytes[i] = (byte)i;
                i++;
            }
            paddingBytes[i] = (byte)(i - 1);

            packetBuffer = Util.ConcatBytes(packetBuffer, paddingBytes);
        }
    }
}