namespace TC.TCTasks
{
    using System;
    using System.Windows.Forms;

    using Timer = System.Timers.Timer;

    internal abstract class TCTask
    {
        private static Random randGen = new Random();

        private DateTime executionTime;

        private bool isCompleted = false;

        private int randomSeed = 0;

        private int interval = 0;

        private Timer timer;

        protected TCTask(AccountInfo account, DateTime executionTime)
        {
            this.ExecutionTime = executionTime;
            this.Account = account;
        }

        protected TCTask(AccountInfo account, int intervalInMiliseconds, int randomSeed = 0)
        {
            this.Account = account;

            var nextDueTime = FormMain.RemoteTime.AddMilliseconds(intervalInMiliseconds);
            this.ExecutionTime = nextDueTime;
            this.IntervalMiliseconds = intervalInMiliseconds;
        }

        private DateTime CalcNextExecutionTime()
        {
            if (this.interval == 0)
            {
                return DateTime.MinValue;
            }

            var nextExecution = this.interval + randGen.NextDouble() * this.randomSeed;
            return FormMain.RemoteTime.AddMilliseconds(nextExecution);
        }

        public TCTask ParaentTask { get; set; }

        public int RandomSeed
        {
            get
            {
                return this.randomSeed;
            }

            set
            {
                this.randomSeed = value;
            }
        }

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
                this.SetExecutionTime(CalcNextExecutionTime());
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

                        if (this.TaskAction != null)
                        {
                            this.TaskAction(this);
                        }

                        this.SetExecutionTime(this.CalcNextExecutionTime());
                        this.Leave();
                    };
            }
            this.timer.Stop();
            this.timer.Interval = diff.TotalMilliseconds;
            this.timer.Start();
        }

        public bool IsCompleted
        {
            get
            {
                return this.isCompleted;
            }

            set
            {
                if (value)
                {
                    this.timer.Stop();
                }
                this.isCompleted = value;
            }
        }

        public AccountInfo Account { get; private set; }

        public bool Executing { get; set; }

        public Action<object> TaskAction { get; set; }

        public abstract string TaskId { get; set; }

        public static string Time2Str(int timeval)
        {
            if (timeval < 0)
            {
                return "N/A";
            }

            var secs = timeval % 60;
            var mins = (timeval / 60) % 60;
            var hours = timeval / 3600;
            var fmt = "{0:D2}:{1:D2}:{2:D2}";
            return string.Format(fmt, hours, mins, secs);
        }

        public void SyncToListViewItem(ListViewItem lvItem, DateTime now)
        {
            lvItem.SubItems.Add(this.Account.UserName);
            lvItem.SubItems.Add(this.ExecutionTime.ToString());
            lvItem.SubItems.Add(this.ExecutionTime.ToString());

            var totalSeconds = (int)(now - this.ExecutionTime).TotalSeconds;
            var etaString = totalSeconds >= 0 ? totalSeconds.ToString() : "N/A";
            lvItem.SubItems.Add(etaString);
            lvItem.SubItems.Add(this.GetTaskHint());
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

        public abstract string GetTaskHint();
    }
}