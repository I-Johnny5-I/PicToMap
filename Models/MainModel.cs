using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using PicToMap.ViewModels;
using Avalonia.Media;

namespace PicToMap.Models
{
    public class MainModel
    {
        public MainWindowViewModel _viewModel;
        public BackgroundWorker Worker { get; set; }

        public MainModel(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            Worker = new BackgroundWorker();
            Worker.DoWork += Worker_DoWork;
            Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }
        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            _viewModel.Status = "Generating";
            _viewModel.StatusForeground = Brushes.Yellow;
            var assets = new Assets(_viewModel.ImagePath, _viewModel.HighQualitySelected, _viewModel.WidthInMaps, _viewModel.HeightInMaps);
            var mapGenerator = new MapGenerator(_viewModel, assets);
            mapGenerator.Generate();
        }
        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            _viewModel.Status = "Done";
            _viewModel.StatusForeground = Brushes.Lime;
        }
    }
}
