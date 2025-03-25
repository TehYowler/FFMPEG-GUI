namespace FFMPEG_GUI.ViewModels;
using System;
using FFMPEG_GUI.Views;

public class MainModel : PageViewModelBase
{

    public MainModel(): base() {

    }

    //The basic title and description of the page.
    public string Title => "Navigation";
    public string Details => @"Welcome to the media alteration tool used for altering or creating GIFs and videos! Here you can:
    Use the ""Stitch"" menu to combine multiple images into a type of media.
    Use the ""Concatenate"" menu to append one piece of media to another.
    Use the ""Trim"" menu to precisely shorten the length of a piece of media.
    Use the ""Convert"" menu to convert one type of media to another.";

    // This is our first page, so we can navigate to the next page in any case
    public override bool CanNavigateNext
    {
        get => true;
        protected set => throw new NotSupportedException();
    }

    // You cannot go back from this page.
    public override bool CanNavigatePrevious
    {
        get => false;
        protected set => throw new NotSupportedException();
    }
}
