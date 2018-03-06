using System.Drawing;

namespace Wallpapers.Source
{
    public interface IEngine
    {
        string Name { get; }
        Point ScreenSize { get;  }

        string GetImageUrl();
        string GetImageUid();
    }
}
