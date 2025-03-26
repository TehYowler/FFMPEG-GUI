namespace FFMPEG_GUI.ViewModels;
using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Tmds.DBus.Protocol;
using System.Linq;
using System.Threading.Tasks;

public class ConvertModel : PageViewModelBase
{

    public ConvertModel(MainWindowViewModel overseer): base(overseer) {

    }

    //The basic title and description of the page.
    public string Title => "Convert.";
    public string Details => @"This page converts animated media from one type to another.
    The first file path from the operating file paths is what will be converted from.
    Then, click the button below to choose your file path and extension.";

    //The "get" variables determine if you can navigate back and forth from a page.
    public override bool CanNavigateNext
    {
        get => false;
        protected set => throw new NotSupportedException();
    }
    public override bool CanNavigatePrevious
    {
        get => true;
        protected set => throw new NotSupportedException();
    }

    public async void PerformConvert(Control control) {
        try {
            FileDetails from = FileDetails.FromPath(File.ReadAllText(@"./FilePaths.txt").Split("\n")[0]);
            FileDetails to = (FileDetails)await PathTo(control);

            Convert(from,to);
        }
        catch{}
        
    }
    
}

