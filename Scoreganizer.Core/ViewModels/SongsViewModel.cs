using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Lomont.Scoreganizer.Core.Model;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;

using System.Drawing;
using System.IO;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
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
        }

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


        public async void ViewPdf()
        {
            model.SaveSongs();
            var sf = SelectedSong;

            if (sf == null || !sf.PdfName.ToLower().EndsWith("pdf"))
            {
                model.AddMessage("Select a song with a PDF file"); // todo - message box?
                return;
            }

            var result = await navigationService.Navigate<PdfViewModel, PdfToken, PdfReturn>(new PdfToken(sf));
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
            var sourceItem = dropInfo.Data as SongData;
            var targetItem = dropInfo.TargetItem as SongData;

            if (sourceItem != null && targetItem != null)// && targetItem.CanAcceptChildren)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as SongData;
            var targetItem = dropInfo.TargetItem as SongData;
            if (sourceItem != null && targetItem != null)
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
        public IMvxCommand ViewPdfCommand => new MvxCommand(ViewPdf);
        public IMvxCommand ViewOptionsCommand => new MvxCommand(ViewOptions);
        public IMvxCommand ViewOrganizerCommand => new MvxCommand(ViewOrganizer);
        public IMvxCommand MoveUpCommand => new MvxCommand(()=>MoveSelected(-1));
        public IMvxCommand MoveDownCommand => new MvxCommand(()=>MoveSelected(1));

        /// <summary>
        /// Move selected song in list
        /// </summary>
        /// <param name="dir"></param>
        void MoveSelected(int dir)
        {
            var s = SelectedSong;
            if (s != null)
            {
                var index = Songs.IndexOf(s) + dir;
                if (0 <= index && index < Songs.Count)
                {
                    Songs.Remove(s);
                    Songs.Insert(index,s);
                    SelectedSong = null;
                    SelectedSong = s;
                }
            }
        }

        SongData selectedSong = null;

        public SongData SelectedSong
        {
            get => selectedSong;
            set => SetProperty(ref selectedSong, value);
        }
        void SongClick()
        {
            Debug.WriteLine($"Song {selectedSong.Title} clicked");
            ViewPdf();
        }
    }
}
