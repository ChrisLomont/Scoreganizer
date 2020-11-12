using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Lomont.Scoreganizer.Core.Model;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;

namespace Lomont.Scoreganizer.Core.ViewModels
{
    public class OrganizerViewModel : MvxViewModel<DataModel>
    {
        #region init and nav
        private readonly IMvxNavigationService navigationService;

        public OrganizerViewModel(IMvxNavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public override void Prepare(DataModel data)
        {
            model = data;
            Songs = model.Songs;
            Files.Clear();
            foreach (var fd in model.Files)
                Files.Add(new FileView(fd));
            Messages = model.Messages;
        }

        public async Task Initialize()
        {
            await base.Initialize();

            // do the heavy work here
            // todo 
            // Songs.Add(new SongData("Dookey Dog"));
        }

        public async void ViewSongs()
        {
            model.SaveSongs();
            var result = await navigationService.Navigate<SongsViewModel, DataModel>(model);
            //Do something with the result MyReturnObject that you get back
        }

        #endregion



        public IMvxCommand ViewSongsCommand => new MvxCommand(ViewSongs);
        public IMvxCommand AddSongCommand => new MvxCommand(AddSong);
        public IMvxCommand DeleteSongCommand => new MvxCommand(DeleteSong);
        public IMvxCommand LoadSongsCommand => new MvxCommand(LoadSongs);
        public IMvxCommand SaveSongsCommand => new MvxCommand(SaveSongs);

        public IMvxCommand AddToSongCommand => new MvxCommand(AddToSong);
        public IMvxCommand RemoveFromSongCommand => new MvxCommand(RemoveFromSong);
        public IMvxCommand ScanFilesCommand => new MvxCommand(ScanFiles);
        public IMvxCommand MatchFilesCommand => new MvxCommand(MatchFiles);


        DataModel model = null;
        public MvxObservableCollection<FileView> Files { get; } = new MvxObservableCollection<FileView>();
        public MvxObservableCollection<SongData> Songs { get; private set;  }

        public class FileView : MvxViewModel
        {
            public FileView(FileData fileData)
            {
                FileData = fileData;
            }

            public FileData FileData { get; }

            int useCount = 0;
            public int UseCount {
                get => useCount;
                set => SetProperty(ref useCount, value);
            }

        }

        void MatchFiles()
        {
            var counts = new Dictionary<string, int>();
            foreach (var s in Songs)
                foreach (var f in s.Files)
                {
                    var h = f.Hash;
                    if (!counts.ContainsKey(h))
                        counts.Add(h, 0);
                    counts[h]++;
                }
            foreach (var f in Files)
            {
                var h = f.FileData.Hash;
                if (counts.ContainsKey(h))
                    f.UseCount = counts[h];
            }
        }
        void NotImplemented()
        {
            model.AddMessage("Not implemented!");
        }
        public MvxObservableCollection<string> Messages { get; private set;  } 

        void AddSong()
        {
            var name = SongName;
            if (String.IsNullOrEmpty(name))
            {
                model.AddMessage("Must have song name entered");
                return;
            }

            Songs.Add(new SongData(name));
        }

        void DeleteSong()
        {
            var song = SelectedSong;
            if (song == null)
            {
                model.AddMessage("Must have song selected to delete");
                return;
            }

            SelectedSong = null;
            Songs.Remove(song);
        }

        void LoadSongs()
        {

#if false
            Songs.Clear();
            var filename = Path.Combine(model.BasePath, model.SaveFilename);
            var songIter = GetSongs(
                File.ReadLines(filename),
                model.version,
                model.AddMessage,
                model.BasePath
            );

            BackgroundLoader<SongData>.LoadItemsAsync(
                songIter,
                Songs.Add,
                5
                );

#else
            model.LoadSongsAsync();
#endif

        }

        void SaveSongs()
        {
            model.SaveSongs();
        }

        void AddToSong()
        {
            var song = SelectedSong;
            var file = SelectedFile;
            if (song == null || file == null)
            {
                model.AddMessage("Must have song and file(s) selected to add files");
                return;
            }

            song.AddFile(file.FileData);

            // trigger repaint, restore selection
            var index = Songs.IndexOf(song);
            Songs.Remove(song);
            Songs.Insert(index, song);
            SelectedSong = song;
        }

        void RemoveFromSong()
        {
            var song = SelectedSong;
            var file = SelectedFile;
            if (song == null || file == null)
            {
                model.AddMessage("Must have song and file(s) selected to remove files");
                return;
            }

            song.RemoveFile(file.FileData);

            // trigger repaint
            var index = Songs.IndexOf(song);
            Songs.Remove(song);
            Songs.Insert(index, song);

            SelectedSong = song; // put back
        }


        string songName = "<none>";

        public string SongName
        {
            get => songName;
            set => SetProperty(ref songName, value);
        }

        SongData selectedSong = null;

        public SongData SelectedSong
        {
            get => selectedSong;
            set => SetProperty(ref selectedSong, value);
        }

        FileView selectedFile = null;

        public FileView SelectedFile
        {
            get => selectedFile;
            set => SetProperty(ref selectedFile, value);
        }

        /// <summary>
        /// Scan files, load async into system
        /// </summary>
        public void ScanFiles()
        {
            Files.Clear();

            BackgroundLoader<FileData>.LoadItemsAsync(
                FileScanner.ScanEnum(model.Options.BasePath),
                fd=>Files.Add(new FileView(fd))
            );
        }
    }
}