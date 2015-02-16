using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace TC
{
    partial class FormMain
    {
        private string HTTPRequest(string url, string account, string body = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = UserAgent;
            request.Headers.Add("Cookie", GetAccountCookie(account));

            if (!string.IsNullOrEmpty(body))
            {
                var codedBytes = new ASCIIEncoding().GetBytes(body);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = codedBytes.Length;
                request.GetRequestStream().Write(codedBytes, 0, codedBytes.Length);
            }

            // var response = (HttpWebResponse)request.GetResponse();
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                SetAccountCookie(account, response.Headers["Set-Cookie"]);

                string content = string.Empty;
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    content = reader.ReadToEnd();
                }

                response.Close();
                return content;
            }
        }

        private string Time2Str(int timeval)
        {
            int secs = timeval % 60;
            int mins = (timeval / 60) % 60;
            int hours = timeval / 3600;
            string fmt = "{0:D2}:{1:D2}:{2:D2}";
            return string.Format(fmt, hours, mins, secs);
        }

        private string CalcGroupType(TeamInfo team)
        {
            if (team.isGroupTeam)
            {
                return "群组";
            }

            if (team.isDefendTeam)
            {
                return "防御";
            }

            return "攻击";
        }

        private void SyncTasksToTaskListView()
        {
            listViewTasks.Items.Clear();

            foreach (TeamInfo team in teamList)
            {

                ListViewItem newli = new ListViewItem();
                newli.SubItems[0].Text = team.AccountName;
                newli.SubItems.Add(team.TeamId);
                newli.SubItems.Add(team.PowerIndex.ToString());
                newli.SubItems.Add(Time2Str(team.Duration));
                newli.SubItems.Add(Time2Str(team.TimeLeft));
                newli.SubItems.Add(team.GroupId);
                newli.SubItems.Add(CalcGroupType(team));
                newli.Tag = team;
                listViewTasks.Items.Add(newli);
            }

        }

        private string ConvertStatusStr(string status)
        {
            if (status == "on-line")
                return "已登录";
            if (status == "in-login")
                return "登录中";
            if (status == "login-failed")
                return "登录失败";
            if (status == "submitting")
                return "提交中";
            if (status == "sync-time")
                return "同步系统时间中";
            return "未登录";
        }

        private void SyncAccountsStatus()
        {

            listViewAccounts.Items.Clear();
            int loginnum = 0;
            foreach (string accountkey in this.accountTable.Keys)
            {
                AccountInfo account = this.accountTable[accountkey];

                ListViewItem newli = new ListViewItem();
                {
                    newli.SubItems[0].Text = account.UserName;
                    newli.SubItems.Add(ConvertStatusStr(account.LoginStatus));
                }
                newli.Tag = account;
                listViewAccounts.Items.Add(newli);

                if (account.LoginStatus == "on-line")
                {
                    loginnum++;
                    hostname = account.AccountType;
                }
            }


            if (loginnum >= this.accountTable.Keys.Count)
            {
                btnLoginAll.Text = "登录所有";
                btnAutoAttack.Enabled = true;
                btnLoginAll.Enabled = false;
                btnScanCity.Enabled = true;
                btnQuickAttack.Enabled = true;
            }
        }

        private void SendTroop(TeamInfo team)
        {
            Task.Run(() =>
            {
                m_sendTroopLock.WaitOne();
                {
                    OpenAttackCityPage(team.TeamId, destCityID, team.AccountName);
                    AttackTarget(team.TeamId, destCityID, team.AccountName);
                }
                m_sendTroopLock.Set();
            });
        }

        delegate void DoSomething();

        private static int CompareTeamInfo(TeamInfo x, TeamInfo y)
        {
            if (x.Duration == y.Duration)
            {
                return 0;
            }
            else if (x.Duration > y.Duration)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        private int SortTeamList(int predicttime)
        {
            //adjust priority and get max duration task
            int maxduration = predicttime;
            int attack_diff = 0;
            int attack_ajust = 0;

            //calculate time left to trigger
            foreach (TeamInfo team in teamList)
            {
                if (maxduration > team.Duration)
                {
                    team.TimeLeft = maxduration - team.Duration + attack_ajust;
                    attack_ajust += attack_diff;
                }
                else
                {
                    team.TimeLeft = 0;
                    team.IsTroopSent = true;
                }
            }

            return maxduration;
        }

        /////////////////////////////////////////////////////////////////////
        // send troop timer

        bool SendTroopTimerRunning = false;
        System.Timers.Timer m_SendTroopTimer = new System.Timers.Timer(500);

        private void SendTroopTimerProc(object source, System.Timers.ElapsedEventArgs e)
        {
            if (SendTroopTimerRunning) { return; }
            else
            {
                SendTroopTimerRunning = true;

                int avlcnt = 0;
                foreach (TeamInfo team in teamList)
                {
                    if (team.TimeLeft <= 0 && !team.IsTroopSent && !team.isDefendTeam)
                    {
                        OpenAttackCityPage(team.TeamId, destCityID, team.AccountName);
                        if (team.isGroupTeam)
                            GroupAttackTarget(team.TeamId, destCityID, team.AccountName);
                        else
                            AttackTarget(team.TeamId, destCityID, team.AccountName);
                        team.IsTroopSent = true;
                    }

                    if (team.IsTroopSent)
                    {
                        avlcnt++;
                    }
                }

                if (avlcnt >= teamList.Count)
                {
                    StopUITimer();
                }

                SendTroopTimerRunning = false;
            }
        }

        private void StartSendTroopTimer()
        {
            m_SendTroopTimer.Elapsed += new System.Timers.ElapsedEventHandler(SendTroopTimerProc);
            m_SendTroopTimer.AutoReset = true;
            m_SendTroopTimer.Start();
        }

        private void StopSendTroopTimer()
        {
            m_SendTroopTimer.Stop();
        }

        /////////////////////////////////////////////////////////////////////
        // UI timer

        bool UITimerRunning = false;
        System.Timers.Timer m_UITimer = new System.Timers.Timer(1000);
        DateTime m_lastCheckPoint;

        private void UITimerProc(object source, System.Timers.ElapsedEventArgs e)
        {
            if (UITimerRunning)
            {
                return;
            }

            UITimerRunning = true;
            DateTime now = DateTime.Now;
            TimeSpan diff = now - m_lastCheckPoint;
            if (diff.Seconds <= 0)
            {
                UITimerRunning = false;
                return;
            }

            m_lastCheckPoint = now;
            int avlcnt = 0;
            foreach (TeamInfo team in teamList)
            {
                if (team.TimeLeft <= 0)
                {
                    if (team.Duration > 0)
                    {
                        team.Duration -= diff.Seconds;
                        avlcnt++;
                    }
                }
                else
                {
                    team.TimeLeft -= diff.Seconds;
                    avlcnt++;
                }
            }

            if (avlcnt == 0)
            {
                StopUITimer();
            }
            else
            {
                this.Invoke(new DoSomething(SyncTasksToTaskListView));
            }
            UITimerRunning = false;
        }

        private void StartUITimer()
        {
            m_UITimer.Elapsed += new System.Timers.ElapsedEventHandler(UITimerProc);
            m_UITimer.AutoReset = true;
            m_lastCheckPoint = DateTime.Now;
            m_UITimer.Start();
        }

        private void StopUITimer()
        {
            m_UITimer.Stop();
        }

        /////////////////////////////////////////////////////////////////////
        // System time sync timer
        private object remoteTimeSyncLock = null;
        private System.Timers.Timer remoteTimeSyncTimer = new System.Timers.Timer(500);
        private DateTime remoteTimeSyncLastCheckPoint;
        private DateTime remoteTime;

        private void StartTimeSyncTimer()
        {
            remoteTimeSyncTimer.Elapsed += new System.Timers.ElapsedEventHandler(
                (obj, args) =>
                {
                    lock (this.remoteTimeSyncLock)
                    {
                        DateTime now = DateTime.Now;
                        TimeSpan diff = now - this.remoteTimeSyncLastCheckPoint;
                        if (diff.Seconds >= 1)
                        {
                            this.remoteTimeSyncLastCheckPoint = now;
                            remoteTime = remoteTime.AddSeconds(diff.Seconds);
                            this.Invoke(new DoSomething(() => { this.textBoxSysTime.Text = remoteTime.ToString(); }));
                        }
                    }
                });

            remoteTimeSyncTimer.AutoReset = true;
            remoteTimeSyncLastCheckPoint = DateTime.Now;
            remoteTimeSyncTimer.Start();
        }

        private void StopTimeSyncTimer()
        {
            remoteTimeSyncTimer.Stop();
        }

        /////////////////////////////////////////////////////////////////////
        private void AutoAttackProc()
        {
            // fetch city id through all accounts
            foreach (string account in this.accountTable.Keys)
            {
                if (string.IsNullOrEmpty(this.destCityID))
                {
                    this.destCityID = GetTargetCityID(m_srcCityID, m_dstCityName, account);
                }

                var teamlist = GetAttackTeamsInfo(m_srcCityID, destCityID, account);
                this.teamList.AddRange(teamlist);

                var groupTeamList = GetGroupAttackTeamsInfo(m_srcCityID, destCityID, account);
                this.teamList.AddRange(groupTeamList);
            }

            if (this.teamList.Count == 0)
            {
                this.Invoke(new DoSomething(() =>
                {
                    this.btnAutoAttack.Enabled = true;
                }));
            }
            else
            {
                remoteTime = GetRemoteSysTime();
                StartTimeSyncTimer();
                this.Invoke(new DoSomething(SyncTeamListToUI));
            }
        }

        private void SyncTeamListToUI()
        {
            SyncTasksToTaskListView();
            btnConfirmMainTeams.Enabled = true;
            dateTimePickerArrival.Value = remoteTime;
        }

        private void LoadCityList()
        {
            using (var sr = new StreamReader("cities.txt", System.Text.Encoding.Default))
            {
                string line = sr.ReadLine();
                while (line != null)
                {
                    string[] strs = line.Split('|', ':');
                    if (strs.Length > 1)
                    {
                        cityList.Add(strs[1], strs[0]);
                    }

                    line = sr.ReadLine();
                }
            }
        }

        private void LoadMultiLoginConf()
        {
            using (var sr = new StreamReader("multi_login.conf", Encoding.Default))
            {
                string line = sr.ReadLine();
                while (line != null)
                {
                    string[] strs = line.Split('|');
                    if (strs.Length == 6)
                    {
                        var conf = new LoginParam()
                        {
                            Name = strs[0].Trim(' '),
                            LoginURL = strs[1].Trim(' '),
                            UsernameElmID = strs[2].Trim(' '),
                            PasswordElmID = strs[3].Trim(' '),
                            LoginTitle = strs[4].Trim(' '),
                            HomeTitle = strs[5].Trim(' '),
                        };

                        this.multiLoginConf.Add(conf.Name, conf);
                    }

                    line = sr.ReadLine();
                }
            }
        }

        private void BatchLoginProc()
        {
            foreach (string key in this.accountTable.Keys)
            {
                LoginAccount(key);
            }
        }

        private bool IsOwnCity(string name)
        {
            string cityId = cityList[name];
            foreach (var account in this.accountTable.Values)
            {
                if (account.CityIDList.Contains(cityId))
                {
                    return true;
                }
            }

            return false;
        }

        private string BuildSubHeroesString(ref List<string> heroList)
        {
            var validHeros = heroList.Where((hero, index) => index < 4);
            return string.Join("%7C", validHeros.ToArray());
        }

        private string BuildSoldierString(ref List<Soldier> soldierList, int number)
        {
            if (number == 0)
            {
                return "";
            }

            foreach (var item in soldierList)
            {
                if (item.SoldierNumber >= number)
                {
                    item.SoldierNumber -= number;
                    return string.Format("{0}%3A{1}", item.SoldierType, number);
                }
            }

            return "";
        }

        private IEnumerable<long> CalculateDonations(List<long> resNeeds, List<long> resHave)
        {
            for (int i = 0; i < 4; ++i)
            {
                long toDonate = resNeeds[i] > resHave[i] ? resHave[i] : resNeeds[i];
                yield return toDonate > 10000 ? toDonate : 0;
            }
        }

        private void BatchDonate(int i, ref List<string> accountList)
        {
            foreach (var account in accountList)
            {
                string influenceSciencePage = OpenInfluenceSciencePage(account);
                var influenceRes = ParseInfluenceResource(influenceSciencePage).ToList();
                var resNeeds = influenceRes.Select(resPair => resPair.Value - resPair.Key).ToList();

                this.Invoke(new DoSomething(() =>
                {
                    foreach (ListViewItem lvItem in this.listViewInfluence.Items)
                    {
                        if (lvItem.SubItems[0].Text == account)
                        {
                            for (int j = 1; j < 5; ++j)
                            {
                                lvItem.SubItems[j].Text = influenceRes[j - 1].Key.ToString();
                            }

                            return;
                        }
                    }

                    var newLvItem = new ListViewItem(account);
                    newLvItem.SubItems.AddRange(influenceRes.Select(ar => ar.Key.ToString()).ToArray());
                    this.listViewInfluence.Items.Add(newLvItem);

                }));

                if (resNeeds[3] < 1000000)
                {
                    accountList.Remove(account);
                    continue;
                }

                var accountRes = GetAccountResources(account).ToList();
                while (accountRes[3] < 10000)
                {
                    if (!OpenResourceBox(account))
                    {
                        accountList.Remove(account);
                        break;
                    }

                    accountRes = GetAccountResources(account).ToList();
                }

                var resToContribute = CalculateDonations(resNeeds, accountRes).ToList();

                this.Invoke(new DoSomething(() =>
                {
                    this.txtInfo.Text = string.Format("Donate: {0}:{1}/{2} - {3}", account, accountRes[3], resNeeds[3], i);
                }));

                DonateResource(account, resToContribute[0], resToContribute[1], resToContribute[2], resToContribute[3]);
            }
        }

        private Dictionary<string, List<TeamInfo>> CategorizeTeams(IEnumerable<TeamInfo> teamList)
        {
            var result = new Dictionary<string, List<TeamInfo>>();
            foreach (var team in teamList)
            {
                List<TeamInfo> accountTeams;
                if (!result.TryGetValue(team.AccountName, out accountTeams))
                {
                    accountTeams = new List<TeamInfo>();
                    result.Add(team.AccountName, accountTeams);
                }

                accountTeams.Add(team);
            }

            return result;
        }
    }
}
