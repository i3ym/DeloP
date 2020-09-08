using System;
using osu.Framework;

namespace Painter
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            using (var host = Host.GetSuitableHost("Painter"))
                try { host.Run(new MainScreen()); }
                catch (Exception ex) { Console.WriteLine(ex); }
        }
    }
}
