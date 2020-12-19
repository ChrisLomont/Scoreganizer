using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Lomont.Scoreganizer.Core.Model
{
    /// <summary>
    /// Things saved to system
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Base filepath for library
        /// </summary>
        public string BasePath { get => basePath;
            set
            {
                basePath = value;
                Save();
            }
        }//  = @"d:\ccltext\SheetMusic"; // todo - load from options

        string basePath;

        /// <summary>
        /// Save file name, not including path
        /// </summary>
        public string SaveFilename { get; } = "Scoreganizer.txt";

        /// <summary>
        /// Used to track most recently played songs
        /// </summary>
        public int MostRecentlyPlayedSongsSize = 5;
        public Options()
        {
            Load();
        }

        string GetFilename()
        {
            var fpath= Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            return Path.Combine(fpath, "Options.txt");

        }
        void Save()
        {
            try
            {
                var fname = GetFilename();
                using (var f = File.CreateText(fname))
                    f.WriteLine($"BasePath: {BasePath}");
            }
            catch (Exception ex)
            {
                //Trace.TraceError("");
            }
        }

        void Load()
        {
            var fname = GetFilename();
            if (!File.Exists(fname))
                return;
            foreach (var line in File.ReadAllLines(fname))
            {
                var s = line.IndexOf(":");
                if (s == -1) continue;
                var token = line.Substring(0, s);
                var param = line.Substring(s + 1).Trim();
                if (token == "BasePath")
                    BasePath = param;
            }
        }
    }
}
