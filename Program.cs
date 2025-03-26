using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO;
using System.Text;

namespace FFMPEG_GUI;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)  {

        // StreamWriter sw = new("./FilePaths.txt", false);
        // sw.Write("");
        // sw.Close();

        File.WriteAllText("./FilePaths.txt","");


        BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
        }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
