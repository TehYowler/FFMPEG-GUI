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

public readonly struct FileDetails
{
    public FileDetails(string Path, string Filename, string Directory, string Extention, string FilenamePure)
    {
        absolutePath = Path;
        filename = Filename;
        filenamePure = FilenamePure;
        directory = Directory;
        extention = Extention;
    }

    public string absolutePath { get; init; }
    public string filename { get; init; }
    public string filenamePure { get; init; }
    public string directory { get; init; }
    public string extention { get; init; }

    public override string ToString() => $"({absolutePath})";

    public static FileDetails FromFile(IStorageFile file) {
        string path = Uri.UnescapeDataString(file.Path.AbsolutePath);
        string[] split = path.Split("/");
        
        string filename = split.Last();
        string extention = filename.Split(".").Last();
        string filenamePure = String.Join<string>("/", new ArraySegment<string>( filename.Split("."), 0, filename.Split(".").Length - 1 ));

        string directory = String.Join<string>("/", new ArraySegment<string>( split, 0, split.Length - 1 )) + "/";

        return new FileDetails(path,filename,directory,extention,filenamePure);

        // Console.WriteLine(path);
        // Console.WriteLine(filename);
        // Console.WriteLine(directory);
        // Console.WriteLine(extention);
        // Console.WriteLine(filenamePure);
    }

    public static FileDetails FromPath(string absolutePath) {
        string[] split = absolutePath.Split("/");
        
        string filename = split.Last();
        string extention = filename.Split(".").Last();
        string filenamePure = String.Join<string>("/", new ArraySegment<string>( filename.Split("."), 0, filename.Split(".").Length - 1 ));

        string directory = String.Join<string>("/", new ArraySegment<string>( split, 0, split.Length - 1 )) + "/";

        return new FileDetails(absolutePath,filename,directory,extention,filenamePure);

        // Console.WriteLine(path);
        // Console.WriteLine(filename);
        // Console.WriteLine(directory);
        // Console.WriteLine(extention);
        // Console.WriteLine(filenamePure);
    }
}

public class MainWindowViewModel : ViewModelBase
{

    static FileSystemWatcher watcher = new(".") {
        Filter = "FilePaths.txt"
    };

    public MainWindowViewModel()
    {   
        watcher.Changed += (object sender, FileSystemEventArgs e) => {
            string FileUpdate = "\nOperating on files:\n" + File.ReadAllText(@"./FilePaths.txt");
            foreach(PageViewModelBase model in Pages) {
                model.FileUpdate = FileUpdate;
            }
            // Console.WriteLine(FileUpdate);
        };
        watcher.EnableRaisingEvents = true;

        // Set current page to first on start up
        _CurrentPage = Pages[0];

        // Create Observables which will activate to deactivate our commands based on CurrentPage state
        var canNavNext = this.WhenAnyValue(x => x.CurrentPage.CanNavigateNext);
        var canNavPrev = this.WhenAnyValue(x => x.CurrentPage.CanNavigatePrevious);

        NavigateNextCommand = ReactiveCommand.Create(NavigateNext, canNavNext);
        NavigatePreviousCommand = ReactiveCommand.Create(NavigatePrevious, canNavPrev);
    }
    

    // A readonly array of possible pages.
    private readonly PageViewModelBase[] Pages =
    {
        new MainModel(),
        new ConcatenateModel(),
        new StitchModel(),
        new TrimModel(),
        new ConvertModel()
    };

    // The default is the first page.
    private PageViewModelBase _CurrentPage;

    public PageViewModelBase CurrentPage
    {
        get { return _CurrentPage; }
        private set { this.RaiseAndSetIfChanged(ref _CurrentPage, value); }
    }

    public ICommand NavigateNextCommand { get; }

    private void NavigateNext()
    {
        // get the current index and add 1
        var index = Array.IndexOf(Pages,CurrentPage) + 1;

        //  /!\ Be aware that we have no check if the index is valid. You may want to add it on your own. /!\
        CurrentPage = Pages[index];
    }
    public ICommand NavigatePreviousCommand { get; }
    private void NavigatePrevious()
    {
        // get the current index and subtract 1
        var index = Array.IndexOf(Pages,CurrentPage) - 1;

        //  /!\ Be aware that we have no check if the index is valid. You may want to add it on your own. /!\
        CurrentPage = Pages[index];
    }

    public static async Task<List<FileDetails>?> PathFromAll(Control control) => await PageViewModelBase.PathFromAll(control);

    public async Task SetFilePaths(Control control) {
        List<FileDetails>? filePaths = await PathFromAll(control);
        if(filePaths == null) return;

        try {
            StreamWriter sw = new StreamWriter("./FilePaths.txt", false);

            foreach(FileDetails detail in filePaths) {
                sw.WriteLine(detail.absolutePath);
            }

            sw.Close();
        }
        catch(Exception error) {
            Console.WriteLine("Error: " + error.Message);
        }

    }

    
}