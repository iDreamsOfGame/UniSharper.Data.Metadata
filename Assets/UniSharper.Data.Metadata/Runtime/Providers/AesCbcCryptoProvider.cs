// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using ReSharp.Security.Cryptography;

// ReSharper disable RedundantArgumentDefaultValue

namespace UniSharper.Data.Metadata.Providers
{
    /// <summary>
    /// Provides AES encryption and decryption functionality using CBC (Cipher Block Chaining) mode.
    /// This class implements the <see cref="UniSharper.Data.Metadata.Providers.IDatabaseCryptoProvider"/> interface for secure data encryption in save database files.
    /// </summary>
    internal class AesCbcCryptoProvider : IDatabaseCryptoProvider
    {
        private const int IvDataLength = 16;
        
        public int EncryptionKeyLength => 32;

        public byte[] Encrypt(byte[] data, byte[] key)
        {
            var iv = CryptoUtility.GenerateRandomKey(IvDataLength);
            var cipherData = AesCryptoUtility.Encrypt(data, key, iv);
            var result = new byte[IvDataLength + cipherData.Length];
            WriteBytes(result, 0, iv);
            WriteBytes(result, IvDataLength, cipherData);
            return result;
        }

        public byte[] Decrypt(byte[] data, byte[] key)
        {
            var iv = ReadBytesFromMemory(data, 0, IvDataLength);
            var cipherData = ReadBytesFromMemory(data, IvDataLength, data.Length - IvDataLength);
            return AesCryptoUtility.Decrypt(cipherData, key, iv);
        }
        
        private static void WriteBytes(byte[] destination, int destinationOffset, byte[] source)
        {
            Buffer.BlockCopy(source, 0, destination, destinationOffset, source.Length);
        }

        private static byte[] ReadBytesFromMemory(byte[] source, int sourceOffset, int length)
        {
            var data = new byte[length];
            Buffer.BlockCopy(source, sourceOffset, data, 0, length);
            return data;
        }
    }
}