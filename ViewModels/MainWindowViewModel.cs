using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Avalonia.Media.Imaging;
using PicToMap.Models;

namespace PicToMap.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public Settings Settings { get; set; } 
        public BackgroundWorker BackgroundWorker { get; set; }
        public MainWindowViewModel()
        {
            Settings = new Settings()
            {
                X = -64,
                Y = 128,
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
            var mapGenerator = new MapGenerator(Settings);
        }
        private void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
