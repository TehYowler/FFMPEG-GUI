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
using Avalonia.Metadata;
using System.Linq;
using CliWrap;

public abstract class PageViewModelBase : ViewModelBase
{
    public abstract bool CanNavigateNext { get; protected set; }
    public abstract bool CanNavigatePrevious { get; protected set; }

    private string textGeneric = "rat";
    public string TextGeneric { get { return textGeneric; } set { textGeneric = value; } }

    private string operateGeneric = "Press the button below to choose the files you want to operate on.";
    public string OperateGeneric { get { return operateGeneric; } set { operateGeneric = value; } }

    private string fileUpdate = "\nSelect file(s) to operate on at the bottom left...\n";
    public string FileUpdate {
        get => fileUpdate;
        set => this.RaiseAndSetIfChanged(ref fileUpdate, value);
    }

    public PageViewModelBase(MainWindowViewModel overseer) {
        _overseer = overseer;
    }

    private MainWindowViewModel _overseer;


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

    private static void runCommand(String program,IEnumerable<string> command) {
        Cli.Wrap(program).WithArguments(command).ExecuteAsync();;
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

    public static async Task SetFilePaths(Control control) {
        List<FileDetails>? filePaths = await PathFromAll(control);
        if(filePaths == null) return;

        List<string> writeTo = [];

        try {

            foreach(FileDetails detail in filePaths) {
                writeTo.Add(detail.absolutePath);
            }

            File.WriteAllText("./FilePaths.txt", String.Join("\n", writeTo));
        }
        catch(Exception error) {
            Console.WriteLine("Error: " + error.Message);
        }

    }
    public static void Convert(FileDetails from, FileDetails to) {

        // string fullCommand = $"-y -i '{from.absolutePath.Replace("'",@"'\''")}' '{to.absolutePath.Replace("'",@"'\''")}'";
        // string fullCommand = $"-y -i {from.absolutePath} {to.absolutePath}";

        try {
            // Process process;

            // runCommand("ffmpeg",fullCommand, out process,true,true);

            // if (process.ExitCode == 0) {
            //     Console.WriteLine($"Sucessfully converted file \"{to.absolutePath}\"!");
            // }
            // else {
            //     Console.WriteLine($"FAILED to convert \"{from.absolutePath}\"!");
            // }

            runCommand("ffmpeg",["-y","-i",from.absolutePath,to.absolutePath]);
        }

        catch {
            Console.WriteLine($"FAILED to convert \"{from.absolutePath}\"!");
            return;
        }
        
    }

    public static void Concatenate(IEnumerable<FileDetails> from, FileDetails to) {

        IEnumerable<string> fileTo = from.Select(e => "file '" + e.absolutePath + "'");

        File.WriteAllText("./FilePathsList.txt",String.Join("\n",fileTo));

        // string fullCommand = $"-safe 0 -f concat -i ./FilePathsList.txt -c:v libx264 -preset ultrafast {to.absolutePath}";

        // Console.WriteLine(fullCommand);

        try {
            // Process process;

            // runCommand("ffmpeg",fullCommand, out process,true,true);

            // if (process.ExitCode == 0) {
            //     Console.WriteLine($"Sucessfully converted file \"{to.absolutePath}\"!");
            // }
            // else {
            //     Console.WriteLine($"FAILED to convert \"{String.Join(", ",fileTo)}\"!");
            // }

            runCommand("ffmpeg",["-safe", "0", "-f", "concat", "-i", "./FilePathsList.txt", "-c:v", "libx264", "-preset", "ultrafast", to.absolutePath]);
        }

        catch {
            Console.WriteLine($"FAILED to convert \"{String.Join(", ",fileTo)}\"!");
            return;
        }
        
    }

    public static void Trim(FileDetails from, FileDetails to, string secondsStart, string secondsEnd) {

        // string fullCommand = $"-safe 0 -f concat -i ./FilePathsList.txt -c:v libx264 -preset ultrafast {to.absolutePath}";
        // string fullCommand = $"-ss {secondsStart} -to {secondsEnd} -i {from.absolutePath} -c copy {to.absolutePath}";

        // Console.WriteLine(fullCommand);

        try {
            runCommand("ffmpeg",["-ss", secondsStart, "-to", secondsEnd, "-i", from.absolutePath, "-c", "copy", to.absolutePath]);
        }

        catch {
            Console.WriteLine($"FAILED to convert \"{from.absolutePath}\"!");
            return;
        }
        
    }


    
    public MainWindowViewModel? mainWindow = null;

}


