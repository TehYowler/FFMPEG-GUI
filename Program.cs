using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO;
using System.Text;

namespace FFMPEG_GUI;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)  {

        if(!File.Exists("ffmpeg.exe") && RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            System.IO.Compression.ZipFile.ExtractToDirectory("../../../ffmpeg-6.1-win-64.zip", "ffmpeg-extract");
            File.Move("ffmpeg-extract/ffmpeg.exe","./ffmpeg.exe");
        }

        File.WriteAllText("./FilePaths.txt","");


        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
