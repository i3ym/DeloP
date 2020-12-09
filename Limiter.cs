using System;
using System.Diagnostics;

namespace DeloP
{
    public class Limiter
    {
        public bool InvokeAllowed => Stopwatch.ElapsedMilliseconds > Interval;
        readonly Stopwatch Stopwatch = Stopwatch.StartNew();
        public int Interval;

        public Limiter(int interval) => Interval = interval;

        public void Reset() => Stopwatch.Restart();
        public bool Invoke(Action action)
        {
            if (!InvokeAllowed) return false;

            Stopwatch.Restart();
            action();

            return true;
        }
    }
}