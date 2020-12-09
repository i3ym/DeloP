using System.Diagnostics;
using System;
using osu.Framework;

namespace DeloP
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            using var host = Host.GetSuitableHost("DeloP");
            host.Run(new MainScreen());
        }
    }
}
