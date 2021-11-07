using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PicToMap.ViewModels;
using ReactiveUI;

namespace PicToMap.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif  
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        async private void OnButtonClick_Browse(object sender, RoutedEventArgs args)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Choose an image"
            };
            var dialogResult = await dialog.ShowAsync(this);
            if (dialogResult == null) return;
            if (DataContext is not MainWindowViewModel viewModel) return;
            viewModel._settings.ImagePath = dialogResult[0];              
            this.Get<Image>("Display").Source = new Avalonia.Media.Imaging.Bitmap(dialogResult[0]);
        }
        async private void Generate_Click(object sender, RoutedEventArgs args)
        {
            if (DataContext is not MainWindowViewModel viewModel) return;
            if (viewModel._settings.ImagePath == null) return;
            var dialog = new OpenFolderDialog
            {
                Title = "Choose the destination folder (datapack will end up here)"
            };
            var dialogResult = await dialog.ShowAsync(this);
            if (dialogResult != null)
            {
                viewModel._settings.DestinationDirectory = dialogResult;
                viewModel._settings.Name = this.Get<TextBox>("NameBox").Text;
                var worker = viewModel.BackgroundWorker;
                worker.RunWorkerAsync();
            }
        }
    }
}
