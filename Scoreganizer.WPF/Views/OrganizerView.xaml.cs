﻿using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Lomont.Scoreganizer.Core.ViewModels;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;

namespace Lomont.Scoreganizer.WPF.Views
{
    [MvxViewFor(typeof(OrganizerViewModel))]
    public partial class OrganizerView : MvxWpfView
    {
        public OrganizerView()
        {
            InitializeComponent();
        }

        void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is OrganizerViewModel m)
                m.SelectedFileNode = e.NewValue as OrganizerViewModel.FileNode;
        }
    }
}