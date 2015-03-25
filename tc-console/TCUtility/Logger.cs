using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCUtility
{
    interface ILogger
    {
        void Verbose(string format, params object[] args);
    }

    class FormMainLogger : ILogger
    {
        private readonly FormMain formMain;

        public FormMainLogger(FormMain form)
        {
            this.formMain = form;
        }

        public void Verbose(string format, params object[] args)
        {
            this.formMain.DebugLog(format, args);
        }
    }

    static class Logger
    {
        private static ILogger LoggerInst = null;

        public static void Initialize(ILogger logger)
        {
            LoggerInst = logger;
        }

        public static void Verbose(string format, params object[] args)
        {
            LoggerInst.Verbose(format, args);
        }
    }
}
