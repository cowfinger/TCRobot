using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TC
{
    static class Task
    {
        public static void Run(Action action)
        {
            new Thread(new ThreadStart(action)).Start();
        }
    }
}
