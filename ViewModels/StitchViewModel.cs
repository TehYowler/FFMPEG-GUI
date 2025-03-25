namespace FFMPEG_GUI.ViewModels;
using System;

public class StitchModel : PageViewModelBase
{

    public StitchModel(): base() {

    }

    //The basic title and description of the page.
    public string Title => "Stitch";
    public string Details => "This stitchs pictures into media <insert more descriptions here>.";

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