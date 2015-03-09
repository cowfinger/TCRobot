using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TC
{
    abstract class TCTask
    {
        private System.Timers.Timer timer = null;
        private DateTime executionTime;
        private bool isCompleted = false;

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
                    this.timer = new System.Timers.Timer();
                    this.timer.Elapsed += new System.Timers.ElapsedEventHandler((obj, arg) =>
                    {
                        if (this.TaskAction != null)
                        {
                            this.TaskAction(this);
                        }

                        this.executionTime = this.executionTime.AddMilliseconds(value);
                    });
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
                    this.timer = new System.Timers.Timer();
                    this.timer.Elapsed += new System.Timers.ElapsedEventHandler((obj, arg) =>
                    {
                        if (this.TaskAction != null)
                        {
                            this.TaskAction(this);
                        }
                    });
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

        public static string Time2Str(int timeval)
        {
            int secs = timeval % 60;
            int mins = (timeval / 60) % 60;
            int hours = timeval / 3600;
            string fmt = "{0:D2}:{1:D2}:{2:D2}";
            return string.Format(fmt, hours, mins, secs);
        }

        public void SyncToListViewItem(ListViewItem lvItem, DateTime now)
        {
            lvItem.SubItems.Add(this.Account.UserName);
            lvItem.SubItems.Add(ExecutionTime.ToString());
            lvItem.SubItems.Add(ExecutionTime.ToString());

            int totalSeconds = (int)(now - this.ExecutionTime).TotalSeconds;
            string etaString = totalSeconds >= 0 ? totalSeconds.ToString() : "N/A";
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
        public abstract string TaskId { get; set; }
    }

    class SendTroopTask : TCTask
    {
        public const int OpenAttackPageTime = 5;

        public enum TaskStatus
        {
            OpenAttackPage,
            ConfirmAttack,
        }

        private CityInfo fromCity = null;
        private CityInfo toCity = null;

        public TroopInfo taskData = null;
        public HttpClient webClient = null;

        public TaskStatus Status = TaskStatus.OpenAttackPage;

        public SendTroopTask(
            AccountInfo account,
            CityInfo fromCity,
            CityInfo toCity,
            TroopInfo data,
            DateTime arrivalTime)
            : base(account, arrivalTime.AddSeconds(-(data.Duration - OpenAttackPageTime)))
        {
            if (!data.isGroupTroop)
            {
                this.ExecutionTime = this.ExecutionTime.AddMilliseconds(300);
            }

            this.taskData = data;
            this.fromCity = fromCity;
            this.toCity = toCity;
            this.TaskId = data.isGroupTroop ? data.GroupId : data.TroopId;
            this.webClient = new HttpClient(this.Account.CookieStr);
        }

        public override string GetTaskHint()
        {
            return string.Format("{0} => {1}, duration:{2}", this.fromCity.Name, this.toCity.Name, this.taskData.Duration);
        }

        public override string TaskId
        {
            get
            {
                return this.taskData.isGroupTroop ? this.taskData.GroupId : this.taskData.TroopId;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }

    class MoveTroopTask : TCTask
    {
        public CityInfo CurrentCity {get; set;}
        public CityInfo NextCity {get; set;}
        public CityInfo TerminalCity {get;set;}
        public List<string> HeroIdList = new List<string>();
        public List<Soldier> SoldierList = new List<Soldier>();
        public List<string> Path = null;
        public int BrickNum = 0;
        public int RetryCount = 0;
        private string taskId = "";

        public MoveTroopTask(
            AccountInfo account,
            CityInfo from,
            CityInfo next,
            CityInfo terminal,
            int BrickNum,
            string taskId)
            : base(account, FormMain.RemoteTime.AddMinutes(2))
        {
            this.CurrentCity = from;
            this.NextCity = next;
            this.TerminalCity = terminal;
            this.BrickNum = BrickNum;
            this.taskId = taskId;
        }

        public override string GetTaskHint()
        {
            var pathString = this.Path == null ? "" : string.Join("=>", this.Path.ToArray());
            return string.Format("Move Troop: {0}=>{1}", this.CurrentCity.Name, pathString);
        }

        public override string TaskId
        {
            get
            {
                return this.taskId;
            }
            set
            {
                this.taskId = value;
            }
        }
    }

    class ShipBrickTask : TCTask
    {
        public CityInfo TargetCity
        {
            get;
            private set;
        }

        public List<Soldier> SoldierList = new List<Soldier>();

        public TCTask SubTask = null;

        public ShipBrickTask(AccountInfo account, CityInfo targetCity)
            : base(account, 60 * 1000)
        {
            this.TargetCity = targetCity;
        }

        public override string GetTaskHint()
        {
            return this.TaskId;
        }

        public override string TaskId
        {
            get
            {
                return string.Format("{0}=>{1}", this.Account.UserName, this.TargetCity.Name);
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
