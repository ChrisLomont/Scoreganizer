using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Lomont.Scoreganizer.Core.Model
{
    /// <summary>
    /// File data for each scanned file
    /// </summary>
    public class FileData : IEquatable<FileData>
    {
        public string Filename { get; private set; }
        public FileData(string filename)
        {
            Filename = filename;
            Hash = Hasher.HashFileToText(filename);
        }

        public string Hash { get; set; }


        public override string ToString()
        {
            return Filename;
        }

        public bool Equals(FileData other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Filename == other.Filename && Hash == other.Hash;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FileData) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Filename, Hash);
        }
    }
}
