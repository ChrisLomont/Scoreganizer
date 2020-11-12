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
    [MvxViewFor(typeof(PdfViewModel))]
    public partial class PdfView : MvxWpfView
    {
        public PdfView()
        {
            InitializeComponent();

            // todo - clean somehow?
            // things that don't bind...
            // mediaPlayer.NaturalDuration NaturalDuration = "{Binding MediaDuration, Converter={StaticResource DurationConverter}, Mode=OneWayToSource}"
        }
    }
}