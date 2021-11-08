using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using PicToMap.ViewModels;

namespace PicToMap.Models
{
    public class MainModel 
    {     
        public BackgroundWorker Worker { get; set; }

        public MainModel()
        {
            Worker = new BackgroundWorker();
            Worker.DoWork += Worker_DoWork;
            Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }
        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            if (e.Argument is not MainWindowViewModel viewModel) throw new ArgumentException("viewModel");
            var assets = new Assets(viewModel.ImagePath);
            var mapGenerator = new MapGenerator(viewModel, assets);
            mapGenerator.Generate();
        }
        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
