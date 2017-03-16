using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Turbina
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
//                .UseUrls("http://192.168.1.219:55047")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
