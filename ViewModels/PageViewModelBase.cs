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
using Tmds.DBus.Protocol;
using System.IO.Pipelines;
using CliWrap.Buffered;
using System.Reflection.Metadata.Ecma335;

public abstract class PageViewModelBase : ViewModelBase
{
    public abstract bool CanNavigateNext { get; protected set; }
    public abstract bool CanNavigatePrevious { get; protected set; }

    private string textGeneric = "rat";
    public string TextGeneric { get { return textGeneric; } set { textGeneric = value; } }

    private string operateGeneric = "Press the button below to choose the files you want to operate on.";
    public string OperateGeneric { get { return operateGeneric; } set { operateGeneric = value; } }

    private string fileUpdate = "\nSelect file(s) to operate on. Selecting a single file will allow you to preview it in the video player if it is an applicable file.\n";
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

    private static void RunCommand(String program,String command, out Process proc, bool startImmediate=false,bool waitFor = true) {
        proc = new Process() { EnableRaisingEvents = true, StartInfo = new ProcessStartInfo(program,command){RedirectStandardOutput = true} };
        if(startImmediate) {
            proc.Start();
            if(waitFor) proc.WaitForExit();
        }
    }

    private static CommandTask<BufferedCommandResult> RunCommand(String program,IEnumerable<string> command) {
        return Cli.Wrap(program).WithArguments(command).ExecuteBufferedAsync();
    }

    public static async Task<FileDetails?> PathFrom(Control control) {
        var topLevel = TopLevel.GetTopLevel(control);

        if(topLevel == null) return null;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions() {
            Title = "Pick your file path.",
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

    public async Task SetFilePaths(Control control) {
        await setFilePaths(control);
    }

    public static async Task setFilePaths(Control control) {
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
    public async static void Convert(FileDetails from, FileDetails to) {

        // string fullCommand = $"-y -i '{from.absolutePath.Replace("'",@"'\''")}' '{to.absolutePath.Replace("'",@"'\''")}'";
        // string fullCommand = $"-y -i {from.absolutePath} {to.absolutePath}";

        try {
            BufferedCommandResult result = await RunCommand("ffmpeg",["-y","-i",from.absolutePath,to.absolutePath]);
            if(result.ExitCode != 0) throw new Exception();
        }

        catch {
            Console.WriteLine($"FAILED to convert \"{from.absolutePath}\"!");
            return;
        }
        
    }

    public async static void Concatenate(IEnumerable<FileDetails> from, FileDetails to) {

        //First, try for both audio and video streams
        try {
            List<string> fileArgs = [];
            List<string> avArgs = [];

            int i = 0;
            foreach(FileDetails detail in from) {
                fileArgs.Add("-i");
                fileArgs.Add(detail.absolutePath);
                avArgs.Add($"[{i}:v]");
                avArgs.Add($"[{i}:a]");
                i += 1;
            }

            string filterComplexString = string.Join(" ", avArgs);
            filterComplexString += $" concat=n={i}:v=1:a=1 [v] [a]";
            BufferedCommandResult result = await RunCommand("ffmpeg",["-y", ..fileArgs,"-filter_complex", filterComplexString, "-map", "[v]", "-map", "[a]", to.absolutePath]);
            if(result.ExitCode != 0) throw new Exception();
        }

        catch {
            //Then, try for only video stream
            try {
                List<string> fileArgs = [];
                List<string> avArgs = [];

                int i = 0;
                foreach(FileDetails detail in from) {
                    fileArgs.Add("-i");
                    fileArgs.Add(detail.absolutePath);
                    avArgs.Add($"[{i}:v]");
                    i += 1;
                }

                string filterComplexString = string.Join(" ", avArgs);
                filterComplexString += $" concat=n={i}:v=1:a=0 [v]";
                Console.WriteLine("ffmpeg");
                Console.WriteLine("-y");
                Console.WriteLine(String.Join(' ',fileArgs));
                Console.WriteLine("-filter_complex");
                Console.WriteLine(filterComplexString);
                Console.WriteLine("-map");
                Console.WriteLine("[v]");
                Console.WriteLine(to.absolutePath);
                BufferedCommandResult result = await RunCommand("ffmpeg",["-y", ..fileArgs,"-filter_complex", filterComplexString, "-map", "[v]", to.absolutePath]);
                if(result.ExitCode != 0) throw new Exception();
            }
            //Otherwise report error
            catch {
                IEnumerable<string> fileString = from.Select(e => e.absolutePath);
                Console.WriteLine($"FAILED to convert \"{String.Join(", ",fileString)}\"!");
                return;
            }
        }
        
    }

    public async static void Trim(FileDetails from, FileDetails to, string secondsStart, string secondsEnd) {

        // string fullCommand = $"-safe 0 -f concat -i ./FilePathsList.txt -c:v libx264 -preset ultrafast {to.absolutePath}";
        // string fullCommand = $"-ss {secondsStart} -to {secondsEnd} -i {from.absolutePath} -c copy {to.absolutePath}";

        // Console.WriteLine(fullCommand);

        try {
            BufferedCommandResult result = await RunCommand("ffmpeg",["-y", "-ss", secondsStart, "-to", secondsEnd, "-i", from.absolutePath, "-c", "copy", to.absolutePath]);
            if(result.ExitCode != 0) throw new Exception();
        }

        catch {
            Console.WriteLine($"FAILED to convert \"{from.absolutePath}\"!");
            return;
        }
        
    }
    
    public async static void Stitch(IEnumerable<FileDetails> from, FileDetails to, string duration) {

        IEnumerable<string> fileTo = from.Select(e => "file '" + e.absolutePath + "'\n" + "duration " + duration);

        File.WriteAllText("./FilePathsList.txt",String.Join("\n",fileTo));

        try {
            BufferedCommandResult result = await RunCommand("ffmpeg",["-y", "-safe", "0", "-f", "concat", "-i", "./FilePathsList.txt", "-c:v", "libx264", "-preset", "ultrafast", to.absolutePath]);
            if(result.ExitCode != 0) throw new Exception();
        }

        catch {
            Console.WriteLine($"FAILED to convert \"{String.Join(", ",fileTo)}\"!");
            return;
        }
        
    }

    public async static void StitchGif(IEnumerable<FileDetails> from, FileDetails to, decimal duration) {



        try {
            Directory.CreateDirectory("file-splice"); //If the directory already exists, do nothing

            String extention = from.ElementAt(0).extention;

            foreach(FileInfo file in new DirectoryInfo("file-splice").EnumerateFiles()) {
                file.Delete();
            }

            int i = 0;
            foreach(FileDetails file in from) {
                File.CreateSymbolicLink($"file-splice/image{i.ToString().PadLeft(4,'0')}." + extention, file.absolutePath);
                i += 1;
            }

            BufferedCommandResult result = await RunCommand("ffmpeg",["-y", "-framerate",(1/duration).ToString(), "-f", "image2", "-i", "file-splice/image%04d." + extention, to.absolutePath]);
            if(result.ExitCode != 0) throw new Exception();
        }

        catch {
            IEnumerable<string> fileTo = from.Select(e => e.absolutePath);

            Console.WriteLine($"FAILED to convert: \"{String.Join("\n",fileTo)}\"!");
            return;
        }
        
    }

    public MainWindowViewModel? mainWindow = null;

}


