using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Avalonia.Media;
using PicToMap.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace PicToMap.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        [Reactive] public int X { get; set; }
        [Reactive] public int Y { get; set; }
        [Reactive] public int Z { get; set; }
        [Reactive] public int WidthInMaps { get; set; }
        [Reactive] public int HeightInMaps { get; set; }
        [Reactive] public bool StaircaseSelected { get; set; }
        [Reactive] public bool HighQualitySelected { get; set; }
        [Reactive] public bool DitheringChecked { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public string Status { get; set; }
        [Reactive] public ISolidColorBrush StatusForeground { get; set; }
        public string ImagePath { get; set; }
        public string CurrentDirectory { get; set; }
        public string DestinationDirectory { get; set; }

        public MainWindowViewModel()
        {
            X = -64;
            Y = 127;
            Z = -64;
            WidthInMaps = 1;
            HeightInMaps = 1;
            StaircaseSelected = true;
            HighQualitySelected = true;
            DitheringChecked = true;
            Name = "map";
            Status = "Waiting";
            StatusForeground = Brushes.Orange;
            ImagePath = string.Empty;
            CurrentDirectory = Directory.GetCurrentDirectory();
            DestinationDirectory = CurrentDirectory;
        }

        public void Generate()
        {
            var model = new MainModel(this);
            model.Worker.RunWorkerAsync();
        }
    }
}
