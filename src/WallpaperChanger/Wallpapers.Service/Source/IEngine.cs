using System.Drawing;

namespace Wallpapers.Service.Source
{
    public interface IEngine
    {
        string Name { get; }
        Point ScreenSize { get;  }

        string GetImageUrl();
        string GetImageUid();
    }
}
