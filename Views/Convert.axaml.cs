using Avalonia.Controls;
using Avalonia.Platform.Storage;
using FFMPEG_GUI.ViewModels;


namespace FFMPEG_GUI.Views
{
    public partial class Convert : UserControl {
        public Convert() {
            var ViewModel = new MainWindowViewModel();
            InitializeComponent();
        }

    }
}