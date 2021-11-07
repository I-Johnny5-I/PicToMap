using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Avalonia.Media;
using PicToMap.Models;
using ReactiveUI;

namespace PicToMap.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private Settings _settings;
        public Settings Settings
        {
            get => _settings;
            set => this.RaiseAndSetIfChanged(ref _settings, value);
        }
        public int _x;
        public int X
        {
            get => _x;
            set => this.RaiseAndSetIfChanged(ref _x, value);
        }
        public string _status;
        public string Status
        {
            get => _status;
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }
        public ISolidColorBrush _statusForeground;
        public ISolidColorBrush StatusForeground
        {
            get => _statusForeground;
            set => this.RaiseAndSetIfChanged(ref _statusForeground, value);
        }
        public BackgroundWorker BackgroundWorker { get; set; }
        public MainWindowViewModel()
        {
            _status = "Waiting";
            _statusForeground = Brushes.Orange;
            _settings = new Settings()
            {
                X = -64,
                Y = 127,
                Z = -64,
                WidthInMaps = 1,
                HeightInMaps = 1,
                StaircaseSelected = true,
                HighQualitySelected = true,
                DitheringChecked = true,
                CurrentDirectory = Directory.GetCurrentDirectory()
            };
            BackgroundWorker = new BackgroundWorker();
            BackgroundWorker.DoWork += BackgroundWorker_DoWork;
            BackgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
        }
        private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            var mapGenerator = new MapGenerator();
            Status = "Generating Map";
            StatusForeground = Brushes.Yellow;
            mapGenerator.Generate();
        }
        private void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            Status = "Done";
            StatusForeground = Brushes.LimeGreen;

        }
    }
}
