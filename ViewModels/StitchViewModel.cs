namespace FFMPEG_GUI.ViewModels;
using System;
using ReactiveUI;
using System.IO;
using Avalonia.Controls;
using System.Collections.Generic;
using System.Linq;

public class StitchModel : PageViewModelBase
{

    public StitchModel(MainWindowViewModel overseer): base(overseer) {

    }

    //The basic title and description of the page.
    public string Title => "Stitch";
    public string Details => @"This page stitches multiple images into a single piece of animated media.
    Choose the files you want to operate on and make into a piece of media.
    Then choose the desired framerate.
    Then, click the button below to choose your file path and extension.";

    private string frameLength = "1";
    public string FrameLength {
        get => frameLength;
        set => this.RaiseAndSetIfChanged(ref frameLength, value);
    }

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

    public async void PerformStitch(Control control) {
        try {
            IEnumerable<FileDetails> from = File.ReadAllText(@"./FilePaths.txt").Split("\n").Select(e => FileDetails.FromPath(e));
            FileDetails to = (FileDetails)await PathTo(control);

            decimal frame = decimal.Parse(FrameLength);

            string frameString = "00:" + frame;

            Stitch(from,to, FrameLength);
        }
        catch{}
        
    }
}