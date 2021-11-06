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
                Title = "Select an image"
            };
            var dialogResult = await dialog.ShowAsync(this);
            if (dialogResult == null) return;
            if (DataContext is MainWindowViewModel viewModel)
            {
                var settings = viewModel.Settings;
                settings.ImagePath = dialogResult[0];
            }                
            this.Get<Image>("Display").Source = new Avalonia.Media.Imaging.Bitmap(dialogResult[0]);
        }
        private void OnButtonClick_Generate(object sender, RoutedEventArgs args)
        {
            
        }
    }
}
