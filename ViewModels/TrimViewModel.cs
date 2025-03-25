namespace FFMPEG_GUI.ViewModels;
using System;

public class TrimModel : PageViewModelBase
{

    public TrimModel(): base() {

    }

    //The basic title and description of the page.
    public string Title => "Trim";
    public string Details => "This page trims images <more details ehre>.";

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
}