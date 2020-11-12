using System;
using System.Collections.Generic;
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

namespace Lomont.Scoreganizer.Core.ViewModels
{
    public class OptionsViewModel : MvxViewModel<DataModel>
    {
        #region init and nav

        private readonly IMvxNavigationService navigationService;

        public OptionsViewModel(IMvxNavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public override void Prepare(DataModel data)
        {
            model = data;
            BasePath = model.Options.BasePath;
            // first callback. Initialize parameter-agnostic stuff here
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            // do the heavy work here
        }

        public async void SomeMethodToClose()
        {
            // here you returned the value
            await navigationService.Close(this);
        }
        #endregion
        public IMvxCommand BackCommand => new MvxCommand(SomeMethodToClose);

        DataModel model;

        string basePath = "";
        public string BasePath
        {
            get => basePath;
            set => SetProperty(ref basePath, value, () => model.Options.BasePath = BasePath);
        }

    }
}
