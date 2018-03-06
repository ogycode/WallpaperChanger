using System.ServiceProcess;

namespace Wallpapers.Service
{
    static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Brain()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
