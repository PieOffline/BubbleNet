using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BubbleNet.Services
{
    /// <summary>
    /// Mubble encryption service - "Muddled Bubble" encryption for secure payload transfer.
    /// Provides XOR-based encryption using a user-defined code for simple but effective
    /// scrambling of payload data across the local network.
    /// 
    /// Note: This is NOT cryptographically secure encryption - it's designed for
    /// privacy on trusted local networks, not for securing sensitive data.
    /// </summary>
    public static class MubbleService
    {
        /// <summary>
        /// Encrypts data using the Mubble code.
        /// Uses XOR encryption with key derivation for simple scrambling.
        /// </summary>
        /// <param name="data">The data to encrypt</param>
        /// <param name="code">The Mubble code (password)</param>
        /// <returns>Encrypted data bytes</returns>
        public static byte[] Encrypt(byte[] data, string code)
        {
            // Return original data if no code provided
            if (string.IsNullOrEmpty(code) || data == null || data.Length == 0)
                return data ?? Array.Empty<byte>();

            // Generate a key from the code
            byte[] key = DeriveKey(code, data.Length);

            // XOR each byte with the corresponding key byte
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ key[i % key.Length]);
            }

            return result;
        }

        /// <summary>
        /// Decrypts data using the Mubble code.
        /// XOR encryption is symmetric, so encryption and decryption use the same operation.
        /// </summary>
        /// <param name="encryptedData">The encrypted data to decrypt</param>
        /// <param name="code">The Mubble code (password)</param>
        /// <returns>Decrypted data bytes</returns>
        public static byte[] Decrypt(byte[] encryptedData, string code)
        {
            // XOR is symmetric - decryption is the same as encryption
            return Encrypt(encryptedData, code);
        }

        /// <summary>
        /// Encrypts a string using the Mubble code.
        /// </summary>
        /// <param name="text">The text to encrypt</param>
        /// <param name="code">The Mubble code (password)</param>
        /// <returns>Base64-encoded encrypted string</returns>
        public static string EncryptString(string text, string code)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(code))
                return text ?? string.Empty;

            byte[] data = Encoding.UTF8.GetBytes(text);
            byte[] encrypted = Encrypt(data, code);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Decrypts a Base64-encoded string using the Mubble code.
        /// </summary>
        /// <param name="encryptedText">The Base64-encoded encrypted text</param>
        /// <param name="code">The Mubble code (password)</param>
        /// <returns>Decrypted string</returns>
        public static string DecryptString(string encryptedText, string code)
        {
            if (string.IsNullOrEmpty(encryptedText) || string.IsNullOrEmpty(code))
                return encryptedText ?? string.Empty;

            try
            {
                byte[] encrypted = Convert.FromBase64String(encryptedText);
                byte[] decrypted = Decrypt(encrypted, code);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch
            {
                // Return original text if decryption fails (invalid Base64, etc.)
                return encryptedText;
            }
        }

        /// <summary>
        /// Verifies if a given code matches the expected code.
        /// Used by the receiver to check if they can decrypt the payload.
        /// </summary>
        /// <param name="receivedCode">The code received with the payload</param>
        /// <param name="localCode">The local Mubble code setting</param>
        /// <returns>True if codes match (case-sensitive)</returns>
        public static bool CodesMatch(string receivedCode, string localCode)
        {
            // Both must be non-empty and match exactly
            if (string.IsNullOrEmpty(receivedCode) || string.IsNullOrEmpty(localCode))
                return false;

            return receivedCode == localCode;
        }

        /// <summary>
        /// Derives an encryption key from the Mubble code.
        /// Uses SHA256 hashing to expand the code into a consistent key.
        /// </summary>
        /// <param name="code">The Mubble code</param>
        /// <param name="minLength">Minimum key length needed</param>
        /// <returns>Derived key bytes</returns>
        private static byte[] DeriveKey(string code, int minLength)
        {
            // Use SHA256 to create a consistent 32-byte key from any code
            using var sha256 = SHA256.Create();
            byte[] codeBytes = Encoding.UTF8.GetBytes(code);
            byte[] baseKey = sha256.ComputeHash(codeBytes);

            // If we need more bytes, expand the key by hashing repeatedly
            if (minLength <= baseKey.Length)
                return baseKey;

            // Expand key for larger payloads by concatenating multiple hashes
            using var ms = new MemoryStream();
            byte[] current = baseKey;
            while (ms.Length < minLength)
            {
                ms.Write(current, 0, current.Length);
                // Hash the previous hash to get the next block
                current = sha256.ComputeHash(current);
            }
            return ms.ToArray();
        }
    }
}
