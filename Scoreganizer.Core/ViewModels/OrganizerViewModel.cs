using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
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

        bool FilterSongs(object obj)
        {
            if (String.IsNullOrEmpty(songFilter))
                return true;
            if (obj is SongData s)
            {
                if (s.Title.Contains(songFilter) || s.Artist.Contains(songFilter))
                    return true;
                return false;
            }
            return true; 
        }

        ICollectionView songView;

        public override void Prepare(DataModel data)
        {
            model = data;
            Songs = model.Songs;

            // prepare filter
            songView = CollectionViewSource.GetDefaultView(Songs);
            songView.Filter = FilterSongs;

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

        // node in file system
        public class FileNode : MvxViewModel
        {
            public FileData File { get; set; } = null;
            public bool IsFile { get; set; }
            public string Name { get; set; }
            public MvxObservableCollection<FileNode> Children { get; } = new MvxObservableCollection<FileNode>();
        }

        public MvxObservableCollection<FileNode> Nodes { get; } = new MvxObservableCollection<FileNode>();


        DataModel model = null;
        public MvxObservableCollection<FileView> Files { get; } = new MvxObservableCollection<FileView>();
        public MvxObservableCollection<SongData> Songs { get; private set;  }

        // view for a song
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

            var s = new SongData(name);
            Songs.Add(s);
            SelectedSong = s;
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

        FileData GetSelectedFile()
        {
            if (SelectedFile != null)
                return SelectedFile.FileData;
            return SelectedFileNode?.File;
        }

        void AddToSong()
        {
            var song = SelectedSong;
            var file = GetSelectedFile();
            if (song == null || file == null)
            {
                model.AddMessage("Must have song and file(s) selected to add files");
                return;
            }

            song.AddFile(file);

            // trigger repaint, restore selection
            var index = Songs.IndexOf(song);
            Songs.Remove(song);
            Songs.Insert(index, song);
            SelectedSong = song;
        }

        void RemoveFromSong()
        {
            var song = SelectedSong;
            var file = GetSelectedFile();
            if (song == null || file == null)
            {
                model.AddMessage("Must have song and file(s) selected to remove files");
                return;
            }

            song.RemoveFile(file);

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

        string songFilter = "";

        public string SongFilter
        {
            get => songFilter;
            set
            {
                if (SetProperty(ref songFilter, value))
                    songView.Refresh();
            }
        }

        FileView selectedFile = null;

        public FileView SelectedFile
        {
            get => selectedFile;
            set
            {
                if (SetProperty(ref selectedFile, value))
                    SelectedFileNode = null;
            }
        }

        FileNode selectedFileNode = null;

        public FileNode SelectedFileNode
        {
            get => selectedFileNode;
            set
            {
                if (SetProperty(ref selectedFileNode, value))
                    SelectedFile = null;
            }
        }

        /// <summary>
        /// Scan files, load async into system
        /// </summary>
        public void ScanFiles()
        {
            Files.Clear();
            Nodes.Clear();

            BackgroundLoader<FileData>.LoadItemsAsync(
                FileScanner.ScanEnum(model.Options.BasePath),
                fd=>
                {
                    Files.Add(new FileView(fd));
                    InsertNode(fd);
                }
            );
        }

         /* todo
        * 1. bind to tree selected file
        2. clean up item names in this file
        3. Tree view of file better - match other view, tooltip, etc.
        * 4. Add FileData to leaf nodes in file tree
         */
        void InsertNode(FileData fd)
        {
            // fd.Filename
            var pieces = fd.Filename.Split(new [] { Path.DirectorySeparatorChar.ToString() }, StringSplitOptions.RemoveEmptyEntries);
            FileNode cur = null;
            for (var i = 0; i < pieces.Length; ++i)
            {
                var name = pieces[i];
                var isFile = i == pieces.Length - 1;

                // find a node
                var node = cur == null 
                    ? Nodes.FirstOrDefault(f => f.Name == name)
                    : cur.Children.FirstOrDefault(f => f.Name == name);
                if (node == null)
                { // not present, add new
                    node = new FileNode{Name = name, IsFile = isFile};
                    if (isFile)
                        node.File = fd;
 
                    if (cur == null)
                        Nodes.Add(node);
                    else
                        cur.Children.Add(node);
                    cur = node;
                }
                else
                { // found it, continue down tree
                    cur = node;
                }
            }
        }

    }
}