namespace FFMPEG_GUI.ViewModels;
using System;
using Avalonia.Controls;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class ConcatenateModel : PageViewModelBase
{

    public ConcatenateModel(MainWindowViewModel overseer): base(overseer) {

    }

    //The basic title and description of the page.
    public string Title => "Concatenate";
    public string Details => @"This page concatenates animated media into a single file.
    Choose all of the files you want to operate on and concatenate.
    Then, click the button below to choose your file path and extension.
    Make sure that all concatenated files are the same type and resolution!";

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

    public async void PerformConcatenate(Control control) {
        try {
            IEnumerable<FileDetails> from = File.ReadAllText(@"./FilePaths.txt").Split("\n").Select(e => FileDetails.FromPath(e));
            FileDetails to = (FileDetails)await PathTo(control);

            Concatenate(from,to);
        }
        catch{}
        
    }


    
}
