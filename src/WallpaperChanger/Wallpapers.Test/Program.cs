using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Wallpapers.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            if(Connection.Check())
            {
                Source.IEngine engine = new Source.Bing.Engine(1366, 7685);
                string uid = engine.GetImageUid();
                string url = engine.GetImageUrl();
            }
        }
    }
}
