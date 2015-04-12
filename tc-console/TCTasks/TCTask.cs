using TC.TCUtility;

namespace TC.TCTasks
{
    using System;
    using System.Windows.Forms;

    using Timer = System.Timers.Timer;

    internal abstract class TCTask
    {
        protected static readonly Random RandGen = new Random();

        private DateTime executionTime;

        private int interval;

        private Timer timer;

        private ListViewItem uiItem = null;

        protected TCTask(AccountInfo account, DateTime executionTime)
        {
            this.RandomSeed = 0;
            this.ExecutionTime = executionTime;
            this.IsCompleted = false;
            this.Account = account;
        }

        protected TCTask(AccountInfo account, int intervalInMiliseconds, int randomSeed = 0)
        {
            this.RandomSeed = randomSeed;
            this.Account = account;

            var nextDueTime = FormMain.RemoteTime.AddMilliseconds(intervalInMiliseconds);
            this.ExecutionTime = nextDueTime;
            this.IntervalMiliseconds = intervalInMiliseconds;
            this.IsCompleted = false;
        }

        public TCTask ParaentTask { get; set; }

        public int RandomSeed { get; set; }

        public int IntervalMiliseconds
        {
            get
            {
                return this.interval;
            }

            set
            {
                if (value <= 0)
                {
                    return;
                }

                this.interval = value;
                this.SetExecutionTime(this.CalcNextExecutionTime());
            }
        }

        public DateTime ExecutionTime
        {
            get
            {
                return this.executionTime;
            }

            set
            {
                this.SetExecutionTime(value);
            }
        }

        public bool IsCompleted { get; protected set; }

        public AccountInfo Account { get; private set; }

        public bool Executing { get; set; }

        public abstract string TaskId { get; set; }

        private DateTime CalcNextExecutionTime()
        {
            if (this.interval == 0)
            {
                return DateTime.MinValue;
            }

            var nextExecution = this.interval + RandGen.NextDouble() * this.RandomSeed;
            return FormMain.RemoteTime.AddMilliseconds(nextExecution);
        }

        private void SetExecutionTime(DateTime value)
        {
            var diff = value - FormMain.RemoteTime;
            if (diff.TotalMilliseconds <= 0)
            {
                return;
            }

            this.executionTime = value;
            if (this.timer == null)
            {
                this.timer = new Timer();
                this.timer.Elapsed += (obj, arg) =>
                    {
                        if (!this.TryEnter())
                        {
                            return;
                        }

                        this.TaskWorker();

                        if (this.IsCompleted)
                        {
                            this.timer.Stop();
                        }
                        else
                        {
                            this.SetExecutionTime(this.CalcNextExecutionTime());
                        }
                        this.Leave();
                    };
            }
            this.timer.Stop();
            this.timer.Interval = diff.TotalMilliseconds;
            this.timer.Start();
        }

        public static string Time2Str(int timeval)
        {
            if (timeval < 0)
            {
                return "N/A";
            }

            var secs = timeval % 60;
            var mins = (timeval / 60) % 60;
            var hours = timeval / 3600;
            const string fmt = "{0:D2}:{1:D2}:{2:D2}";
            return string.Format(fmt, hours, mins, secs);
        }

        public void SyncToListViewItem(ListViewItem lvItem, DateTime now)
        {
            var totalSeconds = (int)(now - this.ExecutionTime).TotalSeconds;
            var etaString = totalSeconds >= 0 ? totalSeconds.ToString() : "N/A";

            if (this.uiItem != lvItem)
            {
                lvItem.Tag = this;
                this.uiItem = lvItem;
                lvItem.SubItems.Add(this.Account.UserName);
                lvItem.SubItems.Add(this.ExecutionTime.ToString());
                lvItem.SubItems.Add(this.ExecutionTime.ToString());

                lvItem.SubItems.Add(etaString);
                lvItem.SubItems.Add(this.GetTaskHint());
            }
            else
            {
                lvItem.SubItems[3].Text = etaString;
                lvItem.SubItems[4].Text = this.GetTaskHint();
            }
        }

        public bool TryEnter()
        {
            lock (this)
            {
                if (this.Executing)
                {
                    return false;
                }

                this.Executing = true;
                return true;
            }
        }

        public void Leave()
        {
            lock (this)
            {
                this.Executing = false;
            }
        }

        public void Stop()
        {
            this.IsCompleted = true;
            this.timer.Stop();
        }

        protected void Verbose(string format, params object[] args)
        {
            var fmt = string.Format("{0}[{1}]:{2}",
                this.Account.UserName,
                this.TaskId,
                format);
            var log = string.Format(fmt, args);
            Logger.Verbose(log);
        }

        public abstract string GetTaskHint();

        public abstract void TaskWorker();
    }
}