using System;
using System.Collections.Generic;

namespace NomaiVR
{
    public static class TimerHelper
    {
        private static readonly HashSet<System.Threading.Timer> timers = new HashSet<System.Threading.Timer>();

        public static void ExecuteAfter(Action action, int milliseconds)
        {
            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer(s =>
            {
                action();
                timer.Dispose();
                lock (timers)
                {
                    timers.Remove(timer);
                }
            }, null, milliseconds, uint.MaxValue - 10);
            lock (timers)
            {
                timers.Add(timer);
            }
        }

        public static void ExecuteRepeating(Action action, int milliseconds)
        {
            action();
            ExecuteAfter(() =>
            {
                action();
                ExecuteRepeating(action, milliseconds);
            }, milliseconds);
        }
    }
}
