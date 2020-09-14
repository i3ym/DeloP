using System;
using osu.Framework;

namespace DeloP
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            using (var host = Host.GetSuitableHost("DeloP"))
                try { host.Run(new MainScreen()); }
                catch (Exception ex) { Console.WriteLine(ex); }
        }
    }
}
