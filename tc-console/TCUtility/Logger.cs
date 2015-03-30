namespace TC.TCUtility
{
    internal interface ILogger
    {
        void Verbose(string format, params object[] args);
    }

    internal class FormMainLogger : ILogger
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

    internal static class Logger
    {
        private static ILogger LoggerInst;

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