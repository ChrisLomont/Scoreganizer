using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lomont.Scoreganizer.Core.Model
{
    /// <summary>
    /// Scan a directory for all files
    /// </summary>
    public class FileScanner
    {
        public static void Scan(string pathname, Action<FileData> foundItem)
        {
            foreach (var filename in Directory.EnumerateFiles(pathname))
                foundItem(new FileData(filename));

            foreach (var dirname in Directory.EnumerateDirectories(pathname))
                Scan(dirname, foundItem);
        }

        /// <summary>
        /// walk dir tree, yield each filedata
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        public static IEnumerable<FileData> ScanEnum(string pathname)
        {
            foreach (var filename in Directory.EnumerateFiles(pathname))
                yield return new FileData(filename);

            foreach (var dirname in Directory.EnumerateDirectories(pathname))
            {
                foreach (var fd in ScanEnum(dirname))
                    yield return fd;
            }
        }

    }
}
