using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Lomont.Scoreganizer.Core.Model
{
    public class Hasher
    {
        // The cryptographic service provider, shared across all FileData instances
        private static readonly SHA256 Sha256 = SHA256.Create();

        // Compute the file's hash.
        public static byte[] GetHashSha256(string filename)
        {
            using var stream = File.OpenRead(filename);
            return Sha256.ComputeHash(stream);
        }

        // Return a byte array as a sequence of hex values.
        public static string BytesToString(byte[] bytes)
        {
            string result = "";
            foreach (byte b in bytes) 
                result += b.ToString("X2");
            return result;
        }

        public static string HashFileToText(string filename)
        {
            return BytesToString(GetHashSha256(filename));
        }

    }
}
