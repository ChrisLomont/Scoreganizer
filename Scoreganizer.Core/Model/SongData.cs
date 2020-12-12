using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Lomont.Scoreganizer.Core.Model
{
    /// <summary>
    /// Group song items
    /// </summary>
    public class SongData
    {
        public string Title { get; set; }
        public string Artist { get; set; } = "<unknown>";

        public int Year { get; set; } = 0;
        public Genre Genre { get; set; } = Genre.Unknown;
        public List<FileData> Files { get;  } = new List<FileData>();
        public string Comments { get; set; } = "";
        public int PlayCounter { get; set; } = 0;
        public int Rating { get; set; } = 0;

        public DateTime LastPlayDate
        {
            get
            {
                var dt = DateTime.MinValue;
                if (StartDates.Any())
                    dt = DateTime.Compare(dt, StartDates.Max()) > 0 ? dt : StartDates.Max();
                if (EndDates.Any())
                    dt = DateTime.Compare(dt, EndDates.Max()) > 0 ? dt : EndDates.Max();
                return dt;
            }
        }

        public int BeatsPerMinute { get; set; } = 120;

        public List<DateTime> StartDates { get; } = new List<DateTime>();
        public List<DateTime> EndDates { get; } = new List<DateTime>();

        /// <summary>
        /// Call when opening to view, tracks usage
        /// </summary>
        public void StartView()
        {
            StartDates.Add(DateTime.Now);
            PlayCounter++;
        }

        /// <summary>
        /// Call when viewing closing, tracks usage
        /// </summary>
        public void EndView()
        {
            EndDates.Add(DateTime.Now);
        }

        /// <summary>
        /// Get first PDF file if exists
        /// </summary>
        public string PdfName
        {
            get
            {
                var f = Files.FirstOrDefault(fi => fi.Filename.ToLower().EndsWith(".pdf"))?.Filename;
                if (f == null) return "";
                return f;
            }
        }

        public SongData(string title)
        {
            Title = title;
        }

        public void AddFile(FileData file)
        {
            if (!Files.Contains(file))
                Files.Add(file);
        }
        public void RemoveFile(FileData file)
        {
            Files.Remove(file);
        }

        // item to play in media on play screen. Null if no media open
        public string SelectedMedia { get; set; } = null;

        /// <summary>
        /// Current page view
        /// </summary>
        public int PageToView { get; set; } = 1;

        // specific viewing style for pdf
        public string ViewStyle { get; set; } = null;
        public override string ToString()
        {
            return $"{Title} ({Files.Count})";
        }

        // let's UI cache things here
        public object UIHolder = null;
    }
}
