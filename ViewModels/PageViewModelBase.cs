using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Interactivity;

namespace FFMPEG_GUI.ViewModels;
using System;
using ReactiveUI;

using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

public abstract class PageViewModelBase : ViewModelBase
{
    public abstract bool CanNavigateNext { get; protected set; }
    public abstract bool CanNavigatePrevious { get; protected set; }

    private string textGeneric = "rat";

    public string TextGeneric { get { return textGeneric; } set { textGeneric = value; } }

    private string fileUpdate = "\nSelect file(s) to operate on...\n";

    public string FileUpdate {
        get => fileUpdate;
        set => this.RaiseAndSetIfChanged(ref fileUpdate, value);
    }

    FileSystemWatcher watcher = new(".") {
        Filter = "FilePaths.txt"
    };

    public PageViewModelBase() {
        watcher.Changed += (object sender, FileSystemEventArgs e) => {
            FileUpdate = "\nOperating on files:\n" + File.ReadAllText(@"./FilePaths.txt");
            // Console.WriteLine(FileUpdate);
        };
        watcher.EnableRaisingEvents = true;
    }

    public void ClickHandle() {
        Console.WriteLine(TextGeneric);
    }

    public static FilePickerFileType VideoAll { get; } = new("All Images") {
        Patterns = new[] { "*.mp4", "*.ogg", "*.ogv", "*.avi", "*.mkv", "*.webm", "*.flv", "*.vob", "*.flv", "*.drc", "*.mov", "*.qt"},
        // AppleUniformTypeIdentifiers = new[] { "public.image" },
        MimeTypes = new[] { "video/*" }
    };

    private static void runCommand(String program,String command, out Process proc, bool startImmediate=false,bool waitFor = true) {
        proc = new Process() { EnableRaisingEvents = true, StartInfo = new ProcessStartInfo(program,command){RedirectStandardOutput = true} };
        if(startImmediate) {
            proc.Start();
            if(waitFor) proc.WaitForExit();
        }
    }

    public static async Task<FileDetails?> PathFrom(Control control) {
        var topLevel = TopLevel.GetTopLevel(control);

        if(topLevel == null) return null;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions() {
            Title = "Pick your file path.",
            FileTypeFilter = new[] { VideoAll },
            SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync("../../../Conversion")
        });

        try {
            var file = files[0];

            FileDetails details = FileDetails.FromFile(file);

            // Console.WriteLine(details.absolutePath);
            // Console.WriteLine(details.filename);
            // Console.WriteLine(details.directory);
            // Console.WriteLine(details.extention);
            // Console.WriteLine(details.filenamePure);

            return details;
        }
        catch {
            return null;
        }

    }

    public static async Task<List<FileDetails>?> PathFromAll(Control control) {
        var topLevel = TopLevel.GetTopLevel(control);

        if(topLevel == null) return null;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions() {
            Title = "Pick one or more file paths.",
            FileTypeFilter = new[] { VideoAll },
            AllowMultiple = true,
            SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync("../../../Conversion")
        });

        List<FileDetails> detailList = [];

        try {
            foreach(IStorageFile file in files) {
                FileDetails details = FileDetails.FromFile(file);
                detailList.Add(details);
            }
            return detailList;
        }
        catch {
            return null;
        }

    }

    public static async Task<FileDetails?> PathTo(Control control) {

        var topLevel = TopLevel.GetTopLevel(control);

        if(topLevel == null) return null;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions() {
            Title = "Pick your file path.",
            // FileTypeChoices = new[] { VideoAll },
            DefaultExtension = null,
            SuggestedFileName = null,
            SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync("../../../Conversion")
        });

        if(file is null) {
            return null;
        }

        try {
            FileDetails details = FileDetails.FromFile(file);
            return details;
        }
        catch {
            return null;
        }
        
    }

    public static void Convert(FileDetails from, FileDetails to) {

        // string fullCommand = $"-y -i '{from.absolutePath.Replace("'",@"'\''")}' '{to.absolutePath.Replace("'",@"'\''")}'";
        string fullCommand = $"-y -i {from.absolutePath} {to.absolutePath}";

        try {
            Process process;

            runCommand("ffmpeg",fullCommand, out process,true,true);

            if (process.ExitCode == 0) {
                Console.WriteLine($"Sucessfully converted file \"{to.absolutePath}\"!");
            }
            else {
                Console.WriteLine($"FAILED to convert \"{from.absolutePath}\"!");
            }
        }

        catch {
            Console.WriteLine($"FAILED to convert \"{from.absolutePath}\"!");
            return;
        }
        
    }
    public MainWindowViewModel? mainWindow = null;

}


