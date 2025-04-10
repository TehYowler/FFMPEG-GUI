namespace FFMPEG_GUI.ViewModels;
using System;
using FFMPEG_GUI.Views;

public class MainModel : PageViewModelBase
{

    public MainModel(MainWindowViewModel overseer): base(overseer) {

    }

    //The basic title and description of the page.
    public string Title => "Navigation";
    public string Details => @"Welcome to the media alteration tool used for altering or creating gifs and videos! Here you can:
•    Use the ""Stitch"" menu to combine multiple images into a video or gif.
•    Use the ""Concatenate"" menu to append one piece of media to another (they MUST be of the same types and resolution).
•    Use the ""Trim"" menu to precisely shorten the length of a piece of a video or gif.
•    Use the ""Convert"" menu to convert one type of video or gif to another.";

    //The "get" variables determine if you can navigate back and forth from a page.
    public override bool CanNavigateNext
    {
        get => true;
        protected set => throw new NotSupportedException();
    }
    public override bool CanNavigatePrevious
    {
        get => false;
        protected set => throw new NotSupportedException();
    }
}
