namespace FFMPEG_GUI.ViewModels;

using System;
using Avalonia.Platform.Storage;
using System.Linq;

public readonly struct FileDetails {
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
