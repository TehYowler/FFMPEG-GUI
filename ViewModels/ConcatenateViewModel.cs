namespace FFMPEG_GUI.ViewModels;
using System;


public class ConcatenateModel : PageViewModelBase
{

    public ConcatenateModel(): base() {

    }

    //The basic title and description of the page.
    public string Title => "Concatenate";
    public string Details => "This page concats media <add more description>.";

    // This is our first page, so we can navigate to the next page in any case
    public override bool CanNavigateNext
    {
        get => true;
        protected set => throw new NotSupportedException();
    }

    // You cannot go back from this page.
    public override bool CanNavigatePrevious
    {
        get => true;
        protected set => throw new NotSupportedException();
    }


    
}
