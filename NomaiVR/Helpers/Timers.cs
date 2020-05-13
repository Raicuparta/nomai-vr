using System;
using System.Collections.Generic;

namespace NomaiVR
{
    public static class Timers
    {
        public static void ExecuteAfter(Action action, int milliseconds)
        {
            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer(s =>
            {
                action();
                timer.Dispose();
                lock (timers)
                    timers.Remove(timer);
            }, null, milliseconds, UInt32.MaxValue - 10);
            lock (timers)
                timers.Add(timer);
        }

        private static HashSet<System.Threading.Timer> timers = new HashSet<System.Threading.Timer>();
    }
}
