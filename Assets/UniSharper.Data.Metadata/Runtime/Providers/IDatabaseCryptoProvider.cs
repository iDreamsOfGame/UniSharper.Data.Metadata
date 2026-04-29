// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

namespace UniSharper.Data.Metadata.Providers
{
    /// <summary>
    /// Provides the methods to encrypt and decrypt database.
    /// </summary>
    public interface IDatabaseCryptoProvider
    {
        /// <summary>
        /// Gets the length of the encryption key.
        /// </summary>
        int EncryptionKeyLength { get; }

        /// <summary>
        /// Encrypts data.
        /// </summary>
        /// <param name="data">The data to encrypt.</param>
        /// <param name="key">The key to be used for the encryption algorithm.</param>
        /// <returns>The encrypted data.</returns>
        byte[] Encrypt(byte[] data, byte[] key);

        /// <summary>
        /// Decrypts data.
        /// </summary>
        /// <param name="data">The data to be decrypted.</param>
        /// <param name="key">The key to be used for the decryption algorithm.</param>
        /// <returns>The decrypted data.</returns>
        byte[] Decrypt(byte[] data, byte[] key);
    }
}