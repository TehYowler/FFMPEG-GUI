using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.Diagnostics;
using System.IO;

namespace FFMPEG_GUI.Views;

public partial class MainWindow : Window {
    
    public MainWindow() {
        InitializeComponent();
    }

    private static void runCommand(String program,String command, out Process proc, bool startImmediate=false,bool waitFor = true) {
        proc = new Process() { EnableRaisingEvents = true, StartInfo = new ProcessStartInfo(program,command) };
        if(startImmediate) {
            proc.Start();
            if(waitFor) proc.WaitForExit();
        }
    }

    public static FilePickerFileType VideoAll { get; } = new("All Images") {
        Patterns = new[] { "*.mp4", "*.ogg", "*.ogv", "*.avi", "*.mkv", "*.webm", "*.flv", "*.vob", "*.flv", "*.drc", "*.mov", "*.qt"},
        // AppleUniformTypeIdentifiers = new[] { "public.image" },
        MimeTypes = new[] { "video/*" }
    };

    private async void PathFrom(object? sender, RoutedEventArgs e) {

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions() {
            Title = "Pick the file whose format you want to convert.",
            FileTypeFilter = new[] { VideoAll }
        });

        

        try {
            var file = files[0];
            var path = Uri.UnescapeDataString(file.Path.AbsolutePath);
            FFMPEGFrom.Text = path;
        }
        catch {
            return;
        }

    }

    private async void PathTo(object? sender, RoutedEventArgs e) {

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions() {
            Title = "Pick the file name for your new converted file.",
            // FileTypeChoices = new[] { VideoAll },
            DefaultExtension = null,
            SuggestedFileName = null
        });

        if(file is null) {
            return;
        }

        try {
            var path = Uri.UnescapeDataString(file.Path.AbsolutePath);
            FFMPEGTo.Text = path;
            if(Path.HasExtension(path)) {
                FormatFile.Text = Path.GetExtension(path).ToString();
            }
            
        }
        catch {
            return;
        }
        
    }

    private void Convert(object? sender, RoutedEventArgs e) {

        if(FFMPEGFrom.Text is null) return;

        String origin = FFMPEGFrom.Text;
        String to = Path.GetDirectoryName(FFMPEGTo.Text) + "/" + Path.GetFileNameWithoutExtension(FFMPEGTo.Text) + FormatFile.Text;
        String fullCommand = $"-y -i \"{origin}\" \"{to}\"";


        try {

            Process process;

            runCommand("ffmpeg",fullCommand, out process,true,true);

            if (process.ExitCode == 0) {
                ShowStatus.Text = $"Sucessfully converted file \"{Path.GetFileName(to)}\"!";
            }
            else {
                ShowStatus.Text = $"FAILED to convert \"{Path.GetFileName(origin)}\"!";
            }
        }

        catch {
            ShowStatus.Text = $"FAILED to convert \"{Path.GetFileName(origin)}\"!";
            return;
        }
        
    }
}