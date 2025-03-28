namespace FFMPEG_GUI.ViewModels;
using System;
using System.Collections.Specialized;
using System.IO;
using Avalonia.Controls;
using ReactiveUI;

public class TrimModel : PageViewModelBase
{

    public TrimModel(MainWindowViewModel overseer): base(overseer) {

    }

    private string startSeconds = "0";
    public string StartSeconds {
        get => startSeconds;
        set => this.RaiseAndSetIfChanged(ref startSeconds, value);
    }

    private string endSeconds = "5";
    public string EndSeconds {
        get => endSeconds;
        set => this.RaiseAndSetIfChanged(ref endSeconds, value);
    }

    //The basic title and description of the page.
    public string Title => "Trim";
    public string Details => @"This page trims animated media from and to a specific time.
    Choose a single file you want to operate on and trim.
    Then choose the desired timestamps.
    Then, click the button below to choose your file path and extension.";

    //The "get" variables determine if you can navigate back and forth from a page.
    public override bool CanNavigateNext
    {
        get => true;
        protected set => throw new NotSupportedException();
    }
    public override bool CanNavigatePrevious
    {
        get => true;
        protected set => throw new NotSupportedException();
    }

    public async void PerformTrim(Control control) {
        try {
            FileDetails from = FileDetails.FromPath(File.ReadAllText(@"./FilePaths.txt").Split("\n")[0]);
            FileDetails to = (FileDetails)await PathTo(control);

            decimal start = decimal.Parse(startSeconds);
            decimal end = decimal.Parse(endSeconds);

            string startString = "00:" + start;
            string endString = "00:" + end;

            Trim(from,to, startString, endString);
        }
        catch{}
        
    }
}