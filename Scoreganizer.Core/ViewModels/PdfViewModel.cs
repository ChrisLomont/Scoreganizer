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
using System.Linq;
using Windows.ApplicationModel.Email.DataProvider;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace Lomont.Scoreganizer.Core.ViewModels
{
    public class PdfViewModel : MvxViewModel<PdfToken,PdfReturn>
    {
        #region init and nav
        private PdfToken token;
        DataModel model = null;

        private readonly IMvxNavigationService navigationService;

        public PdfViewModel(IMvxNavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public override void Prepare()
        {
            // first callback. Initialize parameter-agnostic stuff here
        }

        public override void Prepare(PdfToken parameter)
        {
            // receive and store the parameter here
            token = parameter;
            model = token.Model;
            token.Song.StartView();
            LoadMediaFilenames(token.Song);
            
            MRU.Clear();

            foreach (var sd in model.MostRecentlyPlayedSongs.UsedItems)
                MRU.Add(sd);
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            // how to render pages
            RestoreViewStyle();

            // do the heavy work here
            await PdfToImages(token.Song.PdfName);
        }

        public async void SomeMethodToClose()
        {
            token.Song.EndView();
            // here you returned the value
            await navigationService.Close(this, new PdfReturn());
        }
        #endregion
        
        public IMvxCommand CloseCommand => new MvxCommand(SomeMethodToClose);
        public IMvxCommand NextCommand => new MvxCommand(NextPage);
        public IMvxCommand PrevCommand => new MvxCommand(PrevPage);

        void NextPage()
        {
            if (PageIndex < Images.Count-1)
                PageIndex+=2;
        }

        void PrevPage()
        {
            if (2 < PageIndex)
                PageIndex-=2;
        }

        void UpdateViews()
        {
            var left = PageIndex - 1;
            var right = PageIndex;
            var max = Images.Count;

            LeftView  = 0 <= left && left < max ? Images[left] : null;
            RightView = 0 <= right && right < max ? Images[right] : null;
        }

        const string NoName = "<none>";
        void LoadMediaFilenames(SongData song)
        {
            MediaFilenames.Clear();
            MediaFilenames.Add(NoName);
            foreach (var f in song.Files)
            {
                if (IsMedia(f.Filename))
                    MediaFilenames.Add(f.Filename);
            }

            if (!String.IsNullOrEmpty(song.SelectedMedia))
            {
                SelectedMediaFilename = song.SelectedMedia;
                ShowMediaChecked = true;
            }
            else
            {
                ShowMediaChecked = false;
                
                // triggers setter
                var s = song.SelectedMedia;
                SelectedMediaFilename = MediaFilenames[0];
                song.SelectedMedia = s;
            }
        }

        bool showMediaChecked = false;
        public bool ShowMediaChecked
        {
            get => showMediaChecked;
            set => SetProperty(ref showMediaChecked, value);
        }

        string selectedMediaFilename = "";
        public string SelectedMediaFilename
        {
            get => selectedMediaFilename;
            set
            {
                if (SetProperty(ref selectedMediaFilename, value))
                {
                    if (SelectedMediaFilename == NoName)
                    {
                        token.Song.SelectedMedia = null;
                        ShowMediaChecked = false;
                    }
                    else
                    {
                        token.Song.SelectedMedia = value;
                        ShowMediaChecked = true;
                    }
                }
            }
        }
        bool IsMedia(string filename)
        {
            var lext = Path.GetExtension(filename).ToLower();
            return
                lext == ".mp3" ||
                lext == ".mp4" ||
                lext == ".mid"
                ;
        }





        public MvxObservableCollection<Bitmap> Images { get;  } = new MvxObservableCollection<Bitmap>();
        public MvxObservableCollection<string> MediaFilenames { get; } = new MvxObservableCollection<string>();
        public MvxObservableCollection<SongData> MRU { get; } = new MvxObservableCollection<SongData>();

        Bitmap leftView = null;
        public Bitmap LeftView
        {
            get => leftView;
            set => SetProperty(ref leftView, value);
        }
        Bitmap rightView = null;
        public Bitmap RightView
        {
            get => rightView;
            set => SetProperty(ref rightView, value);
        }


        int pageIndex = 0;
        public int PageIndex
        {
            get => pageIndex;
            set
            {
                if (SetProperty(ref pageIndex, value, UpdateViews))
                    token.Song.PageToView = PageIndex;
            }
        }

        void SetViewStyle()
        {
            var s = "";
            if (Filtered) s += "F";
            if (Inverted) s += "I";
            if (Colorized) s += "C";
            token.Song.ViewStyle = s;
        }

        bool triggerRender = true;
        void RestoreViewStyle()
        {
            triggerRender = false;
            if (token.Song.ViewStyle == null) 
                SetViewStyle(); // get default
            var s = token.Song.ViewStyle;
            Filtered = s.Contains('F');
            Colorized = s.Contains('C');
            Inverted = s.Contains('I');
            triggerRender = true;
        }

        bool filtered = true;
        public bool Filtered
        {
            get => filtered;
            set => SetProperty(ref filtered, value, Process);
        }

        bool inverted = false;
        public bool Inverted
        {
            get => inverted;
            set => SetProperty(ref inverted, value, Process);
        }

        bool colorized = false;
        public bool Colorized
        {
            get => colorized;
            set => SetProperty(ref colorized, value, Process);
        }

        async void Process()
        {
            if (!triggerRender)
                return;
            SetViewStyle();
            await PdfToImages(token.Song.PdfName);
        }

        async Task PdfToImages(string path)
        {
            Images.Clear();
            var desiredPage = token.Song.PageToView;
            PageIndex = 1; // changes setting
            token.Song.PageToView = desiredPage;

            // see blog for details
            // had to add references to C:\Program Files (x86)\Windows Kits\10\UnionMetadata\winmd
            // and C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETCore\v4.5". Add System.Runtime.WindowsRuntime.dll 

            // Load the PfdDocument from a path
            var file = await StorageFile.GetFileFromPathAsync(path);
            var pdf = await PdfDocument.LoadFromFileAsync(file);

            // Convert each page into a Bitmap
            for (var index = 0U; index < pdf.PageCount; ++index)
            {

                using var page = pdf.GetPage(index);

                using var stream = new InMemoryRandomAccessStream();
                await page.RenderToStreamAsync(stream, new PdfPageRenderOptions
                    {
                        // BackgroundColor = 
                        DestinationHeight = 1080
                        // DestinationWidth = 
                    }
                );

                var bmp = new Bitmap(stream.AsStream());

                if (filtered)
                {
                    Stopwatch s = Stopwatch.StartNew();
                    bmp = ImageFilters.ToBlackAndWhite(bmp);
                    var t1 = s.ElapsedTicks;
                    bmp = ImageFilters.Sharpen(bmp);
                    var t2 = s.ElapsedTicks;
                    bmp = ImageFilters.Stretch(bmp);
                    var t3 = s.ElapsedTicks;
                    s.Stop();
                    var ms1 = t1 * 1000.0 / Stopwatch.Frequency;
                    var ms2 = (t2-t1) * 1000.0 / Stopwatch.Frequency;
                    var ms3 = (t3-t2) * 1000.0 / Stopwatch.Frequency;
                    var ms = t3 * 1000.0 / Stopwatch.Frequency;
                    Trace.WriteLine($"Image {ms:F3} ms: BW {ms1:F3}, Sharp {ms2:F3}, Stretch {ms3:F3}");
                }

                if (colorized)
                    bmp = ImageFilters.Colorize(bmp);
                if (inverted)
                    bmp = ImageFilters.Invert(bmp);

                Images.Add(bmp);
                if (Images.Count == 1)
                    LeftView = bmp;
                if (Images.Count == 2)
                    RightView = bmp;
            }
            //PageIndex = 1;
            //UpdateViews();

            if (PageIndex != 1)
                PageIndex = desiredPage; // restore page - todo - jumps if moved during...
        }


    }
}
