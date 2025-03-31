namespace FFMPEG_GUI.ViewModels;
using ReactiveUI;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FFMPEG_GUI.Views;
using System;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using System.Linq;

using Avalonia.Controls;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.IO;
using LibVLCSharp.Shared;
using LibVLCSharp.Avalonia;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;
using Avalonia;

public class MainWindowViewModel : ViewModelBase, IDisposable {

    private string filePlay = "Playing: Nothing";
    public string FilePlay {
        get => filePlay;
        set => this.RaiseAndSetIfChanged(ref filePlay, value);
    }

    private string lastTime = "";

    private long playTime = 0;
    private string playTimeSeconds = "0";
    public long PlayTime {
        get => playTime;
        set {
            this.RaiseAndSetIfChanged(ref playTime, value);
            PlayTimeSeconds = ((float)playTime/1000).ToString();
            MediaPlayer.Time = Math.Min(playTime,MediaPlayer.Length);
        }
    }
    public string PlayTimeSeconds {
        get => playTimeSeconds;
        set => this.RaiseAndSetIfChanged(ref playTimeSeconds, value);
    }

    private long playLength = 0;
    public long PlayLength {
        get => playLength;
        set => this.RaiseAndSetIfChanged(ref playLength, value);
    }
    
    static FileSystemWatcher watcher = new(".") {
        Filter = "FilePaths.txt"
    };

    private static readonly LibVLC _libVlc = new LibVLC();
        
    public MediaPlayer MediaPlayer { get; }

    private void OnChanged(object sender, FileSystemEventArgs e) {

        string fileNames = File.ReadAllText(@"./FilePaths.txt");;

        if(fileNames.Trim() != "") {
            string FileUpdate = "\nOperating on files:\n" + fileNames;
            foreach(PageViewModelBase model in Pages) {
                model.FileUpdate = FileUpdate;
            }
        }
        else {
            string FileUpdate = "\nSelect file(s) to operate on. Selecting a single file will allow you to preview it in the video player if it is an applicable file.\n" + fileNames;
            foreach(PageViewModelBase model in Pages) {
                model.FileUpdate = FileUpdate;
            }
        }

        try {
            string[] display = File.ReadAllText(@"./FilePaths.txt").Split("\n");
            if(display.Length != 1 || display[0] == "") {
                FilePlay = "Playing: Nothing";
                return;
            }
            FileDetails getDetails = FileDetails.FromPath(display[0]);
            FilePlay = "Play / Pause: \"" + getDetails.filename + "\"";

            MediaPlayer.Stop();
            using var media = new Media(_libVlc, new Uri(display[0]));
            EventHandler<EventArgs>? pauseOnPlay = null;
            pauseOnPlay = async (sender, e) => {
                MediaPlayer.Playing -= pauseOnPlay;
                await Task.Delay(15);
                MediaPlayer.Pause();
                PlayLength = MediaPlayer.Length;
            };
            MediaPlayer.Playing += pauseOnPlay;
            MediaPlayer.Play(media);
            lastTime = display[0];
        }
        catch(Exception ex) {
            Console.WriteLine(ex.Message);
            FilePlay = "Playing: Nothing";
        }
    }

    public MainWindowViewModel() {   

        MediaPlayer = new MediaPlayer(_libVlc);

        Pages = [
            new MainModel(this),
            new ConcatenateModel(this),
            new StitchModel(this),
            new TrimModel(this),
            new ConvertModel(this)
        ];

        MediaPlayer.TimeChanged += (object? sender, MediaPlayerTimeChangedEventArgs e) => {
            playTime = e.Time;
            PlayLength = Math.Max(MediaPlayer.Length,0);
        };

        MediaPlayer.MediaChanged += (sender, e) => {
            PlayLength = Math.Max(MediaPlayer.Length,0);
            PlayTime = 0;
            MediaPlayer.Pause();
        };

        // if(Design.IsDesignMode == true){};

        watcher.Changed += OnChanged;
        watcher.EnableRaisingEvents = true;

        // Set current page to first on start up
        _CurrentPage = Pages[0];

        // Create Observables which will activate to deactivate our commands based on CurrentPage state
        var canNavNext = this.WhenAnyValue(x => x.CurrentPage.CanNavigateNext);
        var canNavPrev = this.WhenAnyValue(x => x.CurrentPage.CanNavigatePrevious);

        NavigateNextCommand = ReactiveCommand.Create(NavigateNext, canNavNext);
        NavigatePreviousCommand = ReactiveCommand.Create(NavigatePrevious, canNavPrev);
    }

    public void Play()
        {
            if (Design.IsDesignMode)
            {
                return;
            }

            try {
                string[] display = File.ReadAllText(@"./FilePaths.txt").Split("\n");
                if(display.Length != 1 || display[0] == "") return;
                if(lastTime != display[0]) {
                    MediaPlayer.Stop();
                    using var media = new Media(_libVlc, new Uri(display[0]));
                    MediaPlayer.Play(media);
                    lastTime = display[0];
                }
                else {
                    MediaPlayer.Pause();
                }
            }
            catch(Exception e) {
                Console.WriteLine(e.Message);
            }
        }
        
    public void Unload()
    {            
        MediaPlayer.Stop();
    }

    public void Dispose()
    {
        MediaPlayer?.Dispose();
        _libVlc?.Dispose();
        watcher.Changed -= OnChanged;
        watcher.Dispose();
        Console.WriteLine("Disposing...");
        // GC.SuppressFinalize(this);
    }

    // private void PointerEntered(VideoView sender, object e)
    // {
    //     sender.IsVisible = true;
    // }

    // private void PointerExited(VideoView sender, object e)
    // {
    //     sender.IsVisible = false;
    // }
    
    


    // A readonly array of possible pages.
    private readonly PageViewModelBase[] Pages;

    // The default is the first page.
    private PageViewModelBase _CurrentPage;

    public PageViewModelBase CurrentPage {
        get { return _CurrentPage; }
        private set { this.RaiseAndSetIfChanged(ref _CurrentPage, value); }
    }

    public ICommand NavigateNextCommand { get; }

    private void NavigateNext() {
        var index = Array.IndexOf(Pages,CurrentPage) + 1;

        CurrentPage = Pages[index];
    }
    public ICommand NavigatePreviousCommand { get; }
    private void NavigatePrevious() {
        var index = Array.IndexOf(Pages,CurrentPage) - 1;

        CurrentPage = Pages[index];
    }

    public void NavigateTo(int index) {
        CurrentPage = Pages[index];
    }

    public static async Task<List<FileDetails>?> PathFromAll(Control control) => await PageViewModelBase.PathFromAll(control);

    public async Task SetFilePaths(Control control) => await PageViewModelBase.setFilePaths(control);

}