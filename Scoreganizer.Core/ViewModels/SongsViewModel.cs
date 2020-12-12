using System.Diagnostics;
using System.Threading.Tasks;
using Lomont.Scoreganizer.Core.Model;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using GongSolutions.Wpf.DragDrop;
using System.Windows;

namespace Lomont.Scoreganizer.Core.ViewModels
{
    public class SongsViewModel : MvxViewModel<DataModel>, IDropTarget
    {
        #region init and nav

        private readonly IMvxNavigationService navigationService;

        public SongsViewModel(IMvxNavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public override void Prepare()
        {
            // first callback. Initialize parameter-agnostic stuff here
        }

        public override void Prepare(DataModel data)
        {
            model = data;
            //Songs.Clear();
            //Songs = model.Songs;
            //
            //// load these incrementally for UI responsiveness
            //BackgroundLoader<SongData>.LoadItemsAsync(
            //   model.Songs,
            //   Songs.Add,
            //   1
            //   );

            // test C# 9.0

        }
        // static bool IsLetterOrSeparator(this char c) => c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or '.' or ',';

        public override async Task Initialize()
        {
            await base.Initialize();

            // do the heavy work here
            // await PdfToImages(token.Path);
            Songs = model.Songs;
            if (Songs == null || Songs.Count == 0)
                model.LoadSongsAsync();
        }

        public async void SomeMethodToClose()
        {
            model.SaveSongs();
            // here you returned the value
            await navigationService.Close(this);
        }


        public async void ViewSheetMusic()
        {
            model.SaveSongs();
            var sf = SelectedSong;

            if (sf == null || !sf.PdfName.ToLower().EndsWith("pdf"))
            {
                model.AddMessage("Select a song with a PDF file"); // todo - message box?
                return;
            }

            model.MostRecentlyPlayedSongs.Add(sf,model.Options.MostRecentlyPlayedSongsSize);

            var result = await navigationService.Navigate<PdfViewModel, PdfToken, PdfReturn>(new PdfToken(sf,model));
            //Do something with the result MyReturnObject that you get back
        }

        public async void ViewOptions()
        {
            model.SaveSongs();
            var result = await navigationService.Navigate<OptionsViewModel, DataModel>(model);
            //Do something with the result MyReturnObject that you get back
        }
        public async void ViewOrganizer()
        {
            model.SaveSongs();
            var result = await navigationService.Navigate<OrganizerViewModel, DataModel>(model);
            //Do something with the result MyReturnObject that you get back
        }


        #endregion

        #region Drag Drop
        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is SongData && 
                dropInfo.TargetItem is SongData // && targetItem.CanAcceptChildren
                )
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is SongData sourceItem && dropInfo.TargetItem is SongData targetItem)
            {
                var targetIndex = Songs.IndexOf(targetItem);
                Songs.Remove(sourceItem);
                Songs.Insert(targetIndex, sourceItem);
                SelectedSong = sourceItem;
            }
            // targetItem.Children.Add(sourceItem);
        }
        #endregion

        // todo - make in main app.cs, pass into first view
        DataModel model = new DataModel();

        // local copy so changes propagate incrementally, since images need set up
        public MvxObservableCollection<SongData> Songs { get; private set; } = new MvxObservableCollection<SongData>(); 
        public IMvxCommand CloseCommand => new MvxCommand(SomeMethodToClose);
        public IMvxCommand SongClickCommand => new MvxCommand(SongClick);
        public IMvxCommand ViewSheetMusicCommand => new MvxCommand(ViewSheetMusic);
        public IMvxCommand ViewOptionsCommand => new MvxCommand(ViewOptions);
        public IMvxCommand ViewOrganizerCommand => new MvxCommand(ViewOrganizer);


        bool playOnSelect = true;

        public bool PlayOnSelect
        {
            get => playOnSelect;
            set => SetProperty(ref playOnSelect, value);
        }

        SongData selectedSong = null;

        public SongData SelectedSong
        {
            get => selectedSong;
            set
            {
                if (SetProperty(ref selectedSong, value) && PlayOnSelect)
                {
                    if (SelectedSong != null)
                    {
                        // todo - does not work, set timer? SelectedSong = null; // on return, select none
                        ViewSheetMusic();
                    }

                }
            }
        }
        void SongClick()
        {
            Debug.WriteLine($"Song {selectedSong.Title} clicked");
            ViewSheetMusic();
        }
    }
}
