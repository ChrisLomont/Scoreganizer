using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using Lomont.Scoreganizer.Core.ViewModels;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;

namespace Lomont.Scoreganizer.WPF.Views
{
    [MvxViewFor(typeof(OptionsViewModel))]
    public partial class OptionsView : MvxWpfView
    {
        public OptionsView()
        {
            InitializeComponent();
        }
    }
}