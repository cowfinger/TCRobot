namespace TC
{
    using System;
    using System.Windows.Forms;

    using Timer = System.Timers.Timer;

    internal abstract class TCTask
    {
        private DateTime executionTime;

        private bool isCompleted;

        private Timer timer;

        public TCTask(AccountInfo account, DateTime executionTime)
        {
            this.Executing = false;
            this.ExecutionTime = executionTime;
            this.Account = account;
            this.IsCompleted = false;
        }

        public TCTask(AccountInfo account, int intervalInMiliseconds)
        {
            this.Executing = false;
            var nextDueTime = FormMain.RemoteTime.AddMilliseconds(intervalInMiliseconds);
            this.ExecutionTime = nextDueTime;
            this.Account = account;
            this.IsCompleted = false;
        }

        public TCTask ParaentTask { get; set; }

        public int IntervalMiliseconds
        {
            get
            {
                return this.timer == null ? 0 : (int)this.timer.Interval;
            }

            set
            {
                if (value <= 0)
                {
                    return;
                }

                this.executionTime = FormMain.RemoteTime.AddMilliseconds(value);
                if (this.timer == null)
                {
                    this.timer = new Timer();
                    this.timer.Elapsed += (obj, arg) =>
                        {
                            if (this.TaskAction != null)
                            {
                                this.TaskAction(this);
                            }

                            this.executionTime = this.executionTime.AddMilliseconds(value);
                        };
                }
                this.timer.Stop();
                this.timer.Interval = value;
                this.timer.Start();
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
                            if (this.TaskAction != null)
                            {
                                this.TaskAction(this);
                            }
                        };
                }
                this.timer.Stop();
                this.timer.Interval = diff.TotalMilliseconds;
                this.timer.Start();
            }
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