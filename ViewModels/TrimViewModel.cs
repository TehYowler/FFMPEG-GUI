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
    public string Details => "This page trims images <more details ehre>.";

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

            // value-Math.Truncate(value);

            decimal start = decimal.Parse(ControlExtensions.FindControl<TextBox>(control, "StartInput").Text);
            decimal end = decimal.Parse(ControlExtensions.FindControl<TextBox>(control, "EndInput").Text);

            string startString = "00:" + start;
            string endString = "00:" + end;

            Trim(from,to, startString, endString);
        }
        catch{}
        
    }
}