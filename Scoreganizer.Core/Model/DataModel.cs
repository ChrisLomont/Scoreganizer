using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Xps;
using Lomont.Scoreganizer.Core.ViewModels;
using MvvmCross.ViewModels;

namespace Lomont.Scoreganizer.Core.Model
{
    /// <summary>
    /// Class to represent the entire system of data
    /// Passed between view models
    /// </summary>
    public class DataModel
    {
        // todo - remove observable thing here?
        public MvxObservableCollection<SongData> Songs { get; } = new MvxObservableCollection<SongData>();
        public MvxObservableCollection<FileData> Files { get; } = new MvxObservableCollection<FileData>();

        public MvxObservableCollection<string> Messages { get; } = new MvxObservableCollection<string>();

        public MostRecentlyUsed<SongData> MostRecentlyPlayedSongs { get;  } = new MostRecentlyUsed<SongData>();
        public void AddMessage(string message)
        {
            Messages.Add(message);
        }

        public Options Options { get; } = new Options();

        public string Version { get; } = "0.1"; // todo - make global, visible on UI - this also file version.. test changes

        // make backup for today
        public void Backup(string filename)
        {
            var dt = DateTime.Now;
            var suffix = $"{dt.Year}_{dt.Month}_{dt.Day}";
            var f2 = filename + "." + suffix;
            if (!File.Exists(f2) && File.Exists(filename))
                File.Copy(filename, f2);
        }

        bool wasLoaded = false;

        public void SaveSongs()
        {
            if (!wasLoaded)
                return; // do not overwrite old file with empty
            if (String.IsNullOrEmpty(Options.BasePath) || String.IsNullOrEmpty(Options.SaveFilename)) return;
            var filename = Path.Combine(Options.BasePath, Options.SaveFilename);
            Backup(filename); // ensure backup

            using var outfile = File.CreateText(filename);

            // each file entry is of form "token: params"
            outfile.WriteLine($"Version: {Version}");

            // write for now as list of tokens
            foreach (var song in Songs)
            {
                outfile.WriteLine($"Song: {song.Title}"); // start song
                outfile.WriteLine($"Artist: {song.Artist}");
                outfile.WriteLine($"Year: {song.Year}");
                outfile.WriteLine($"Genre: {song.Genre}");
                foreach (var file in song.Files)
                    outfile.WriteLine($"File: {file.Hash} {file.Filename.Replace(Options.BasePath, "")}");
                outfile.WriteLine($"Comments: {song.Comments}"); // todo - endlines?
                outfile.WriteLine($"PlayCounter: {song.PlayCounter}");
                outfile.WriteLine($"Rating: {song.Rating}");

                foreach (var sd in song.StartDates)
                    outfile.WriteLine($"StartPlayDate: {sd:o}");
                foreach (var ed in song.StartDates)
                    outfile.WriteLine($"EndPlayDate: {ed:o}");

                outfile.WriteLine($"BeatsPerMinute: {song.BeatsPerMinute}");
                outfile.WriteLine($"SelectedMedia: {song.SelectedMedia}");
                outfile.WriteLine($"PageToView: {song.PageToView}");
                outfile.WriteLine($"ViewStyle: {song.ViewStyle}");
            }

            foreach (var song in MostRecentlyPlayedSongs.UsedItems)
                outfile.WriteLine($"MostRecentlyPlayedSong: {song.Title}"); // todo - maybe need better id than this

            outfile.WriteLine("End:"); // end of file
        }


        /// <summary>
        /// Find first song with this title. Return null if not found
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public SongData FindSongByTitle(string title)
        {
            return Songs.FirstOrDefault(s => s.Title == title);
        }


        public void LoadSongsAsync()
        {
            Songs.Clear();
            if (string.IsNullOrEmpty(Options.BasePath))
                return;
            var filename = Path.Combine(Options.BasePath, Options.SaveFilename);
            if (!File.Exists(filename))
                return; // todo - log error, or show message

            // read file all at once, lets other uses happen async
            var lines = File.ReadAllLines(filename);

            var songIter = GetSongs(
                lines,
                Version,
                AddMessage,
                Options.BasePath,
                this
            );

            BackgroundLoader<SongData>.LoadItemsAsync(
                songIter,
                Songs.Add,
                5
            );

        }


        // stream of songs from list of string defining them
        // todo - move to song data?
        // todo - parse other items too...
        static IEnumerable<SongData> GetSongs(
            IEnumerable<string> lines, string version, Action<string> errorMessage,
            string basePath,
            DataModel model
            )
        {
            model.MostRecentlyPlayedSongs.UsedItems.Clear();

            SongData song = null; // current song
            SongData readySong = null; // song finished, ready to return it
            var done = false;
            var mruSongs = new List<string>();
            foreach (var line in lines)
            {
                var tokenEnd = line.IndexOf(":", StringComparison.Ordinal);
                if (tokenEnd == -1)
                {
                    errorMessage($"Error: Line didn't have ':' {line}");
                    continue;
                }

                var token = line.Substring(0, tokenEnd).Trim();
                var parameters = line.Substring(tokenEnd + 1).Trim();

                // local helpers
                bool CheckVersion()
                {
                    return token == "Version" && parameters == version;
                }

                bool StartSong()
                {
                    if (token != "Song") return false;
                    if (song != null)
                        readySong = song; // send to outside
                    song = new SongData(parameters);
                    return true;

                }

                bool CheckString(string str, Action<string> action)
                {
                    if (token != str) return false;
                    action(parameters);
                    return true;
                }

                bool CheckInt(string str, Action<int> action)
                {
                    if (token != str) return false;
                    var n = Int32.Parse(parameters);
                    action(n);
                    return true;
                }

                bool CheckEnd()
                {
                    if (token != "End") return false;
                    done = true;
                    return true;
                }

                bool CheckFile()
                {
                    if (token != "File")
                        return false;

                    // get hash, file, append BasePath?
                    var hash = parameters.Substring(0, 64); // length
                    var fn = parameters.Substring(65);
                    fn = Path.Combine(basePath, fn.Substring(1));
                    try
                    {
                        var file = new FileData(fn) { Hash = hash };

                        song.Files.Add(file);
                    }
                    catch (Exception e)
                    {
                        return false;
                    }

                    return true;
                }

                bool CheckGenre()
                {
                    if (token != "Genre")
                        return false;
                    song.Genre = Enum.Parse<Genre>(parameters);
                    return true;
                }

                bool CheckLastPlayDate()
                {
                    if (token != "LastPlayDate")
                        return false;
                    //song.LastPlayDate = DateTime.Parse(parameters);
                    return true;
                }

                bool CheckPlayDate()
                {
                    if (token == "StartPlayDate")
                    {
                        song.StartDates.Add(DateTime.Parse(parameters));
                        return true;
                    }

                    if (token == "EndPlayDate")
                    {
                        song.EndDates.Add(DateTime.Parse(parameters));
                        return true;
                    }

                    return false;
                }



                // check is legal
                if (
                    CheckVersion() ||
                    StartSong() ||
                    CheckString("Artist", p => song.Artist = p) ||
                    CheckInt("Year", n => song.Year = n) ||
                    CheckString("Comments", p => song.Comments = p) ||
                    CheckGenre() ||
                    CheckFile() ||
                    CheckLastPlayDate() ||
                    CheckPlayDate() ||
                    CheckInt("PlayCounter", n => song.PlayCounter = n) ||
                    CheckInt("Rating", n => song.Rating = n) ||
                    CheckInt("BeatsPerMinute", n => song.BeatsPerMinute = n) ||
                    CheckString("SelectedMedia", p => song.SelectedMedia = p) ||
                    CheckString("ViewStyle", p => song.ViewStyle = p) ||
                    CheckInt("PageToView", n => song.PageToView = n) ||
                    CheckEnd()
                )
                {
                    // ok
                    if (readySong != null)
                        yield return readySong;
                    readySong = null;
                }
                else if (token == "MostRecentlyPlayedSong")
                {
                    mruSongs.Add(parameters);
                }
                else
                {
                    // no match
                    errorMessage($"Error: unknown line {line}");
                }

                if (done)
                    break;
            }
            // add final song
            if (song != null)
                yield return song;

            model.MostRecentlyPlayedSongs.UsedItems.Clear();
            foreach (var title in mruSongs)
            {
                // find song
                var song1 = model.FindSongByTitle(title);
                if (song1 != null)
                    model.MostRecentlyPlayedSongs.Add(song1);
                else
                    Trace.TraceError($"Cannot find most recent song title {title} in load");
            }

            // this allows saving to happen
            model.wasLoaded = true; 

        }

    }
}

