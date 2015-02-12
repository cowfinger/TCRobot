using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;

namespace TC
{
    public partial class FormMain : Form
    {

        private static string UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.2; .NET4.0C; .NET4.0E)";
        private StreamWriter logStream = new StreamWriter("tc_log.log", false);
        private Dictionary<string, string> cityList = new Dictionary<string, string>();
        private List<TeamInfo> m_teamlist = new List<TeamInfo>();

        private string hostname = "yw1.tc.9wee.com";
        private Dictionary<string, LoginParam> multiLoginConf = new Dictionary<string, LoginParam>();

        private string m_strCookies = string.Empty;
        private string m_srcCityID = "";
        private string m_srcCityName = "";
        private string m_dstCityID = "";
        private string m_dstCityName = "";

        private AutoResetEvent loginLock = new AutoResetEvent(true);
        private AutoResetEvent m_sendTroopLock = new AutoResetEvent(true);

        private Random randGen = new Random(DateTime.Now.Millisecond);

        private Dictionary<string, AccountInfo> accountTable = new Dictionary<string, AccountInfo>();
        private string activeAccount;

        public FormMain()
        {
            InitializeComponent();
        }

        #region login
        ////////////////////////////////////////////////////////////////////////
        // Login Flow
        private void LoginAccount(string account)
        {
            loginLock.WaitOne();
            {
                logStream.WriteLine("login account <{0}> start ...", account);

                activeAccount = account;
                AccountInfo account_info = this.accountTable[activeAccount];

                account_info.LoginStatus = "in-login";
                string loginurl = this.multiLoginConf[account_info.AccountType].LoginURL;

                webBrowserMain.Navigate(loginurl);

                webBrowserMain.DocumentCompleted +=
                    new WebBrowserDocumentCompletedEventHandler(webBrowserMain_DocumentCompleted);
            }
        }

        private bool SubmitLoginRequest(LoginParam loginconf)
        {
            if (webBrowserMain.Document.GetElementById(loginconf.UsernameElmID) != null)
            {
                webBrowserMain.Document.GetElementById(loginconf.UsernameElmID).InnerText = activeAccount;
                webBrowserMain.Document.GetElementById(loginconf.PasswordElmID).InnerText = this.accountTable[activeAccount].Password;

                Thread.Sleep(1000);
                foreach (HtmlElement he in webBrowserMain.Document.GetElementsByTagName("input"))
                {
                    if (he.GetAttribute("type") == "submit")
                    {
                        he.InvokeMember("Click");
                        logStream.WriteLine("SubmitLoginRequest: Click event invoked");
                        return true;
                    }
                }

                logStream.WriteLine("SubmitLoginRequest: Click event NOT invoked");
                return false;
            }
            else
            {
                logStream.WriteLine("SubmitLoginRequest: GetElementById(vwriter) failed");
                return false;
            }
        }

        void webBrowserMain_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            logStream.WriteLine(
                "webBrowser.DocumentCompleted: Docuement.Title={0}",
                webBrowserMain.Document.Title
                );

            AccountInfo account = this.accountTable[activeAccount];

            if (!this.multiLoginConf.ContainsKey(account.AccountType))
                return;

            LoginParam loginpara = this.multiLoginConf[account.AccountType];

            if (account.LoginStatus == "in-login")
            {
                if (!webBrowserMain.Document.Title.Contains(loginpara.LoginTitle))
                    return;

                if (SubmitLoginRequest(loginpara))
                    account.LoginStatus = "submitting";
                else
                    account.LoginStatus = "login-failed";

            }
            else if (account.LoginStatus == "submitting")
            {
                if (webBrowserMain.Document.Title.Contains(loginpara.HomeTitle))
                {
                    SetAccountCookie(activeAccount, webBrowserMain.Document.Cookie);

                    account.LoginStatus = "on-line";
                    logStream.WriteLine(
                        "webBrowser.Navigated.LoginFin: <{0}> login succ",
                        activeAccount);
                }
                else
                {
                    account.LoginStatus = "login-failed";

                    logStream.WriteLine(
                        "webBrowser.Navigated.LoginFin: <{0}> login failed",
                        activeAccount);
                }
            }

            if (account.LoginStatus == "on-line")
            {
                webBrowserMain.DocumentCompleted -=
                    new WebBrowserDocumentCompletedEventHandler(webBrowserMain_DocumentCompleted);

                loginLock.Set();
            }

            this.Invoke(new DoSomething(SyncAccountsStatus));
            return;
        }

        //cookie operation
        void SetAccountCookie(string account, string val)
        {
            lock (this.accountTable)
            {
                var accountInfo = this.accountTable[account];
                string oldcookiestr = accountInfo.CookieStr;
                if (oldcookiestr.Length == 0)
                {
                    accountInfo.CookieStr = val;
                }
                else
                {
                    Dictionary<string, string> setcookies = ParseCookieStr(val);
                    Dictionary<string, string> oldcookies = ParseCookieStr(oldcookiestr);

                    foreach (string key in setcookies.Keys)
                    {
                        oldcookies[key] = setcookies[key];
                    }

                    accountInfo.CookieStr = ComposeCookieStr(oldcookies);
                }
            }
        }

        string GetAccountCookie(string account)
        {
            lock (this.accountTable)
            {
                return this.accountTable[account].CookieStr;
            }
        }

        Dictionary<string, string> ParseCookieStr(string cookiestr)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            if (cookiestr == null)
                return output;

            string[] strs = cookiestr.Split(';');
            foreach (string i in strs)
            {
                string key = "";
                string val = "";

                int seppos = i.IndexOf('=');
                if (seppos == -1)
                {
                    key = i.Trim(' ');
                }
                else
                {
                    key = i.Substring(0, seppos).Trim(' ');
                    val = i.Substring(seppos + 1);
                }

                if (key == "")
                    continue;

                if (output.ContainsKey(key))
                    output[key] = val;
                else
                    output.Add(key, val);

            }
            return output;
        }

        string ComposeCookieStr(Dictionary<string, string> input)
        {
            string output = "";
            foreach (string i in input.Keys)
            {
                if (i == "path")
                    continue;

                output += i;
                if (input[i] != "")
                {
                    output += "=";
                    output += input[i];
                }
                output += ";";
            }

            return output;
        }

        #endregion

        #region HttpRequest
        ////////////////////////////////////////////////////////////////////////
        // HTTP Request

        private string HTTPRequest(string url, string account, string body = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = UserAgent;
            request.Headers.Add("Cookie", GetAccountCookie(account));

            if (string.IsNullOrEmpty(body))
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

        #endregion

        #region requests
        //////////////////////////////////////////////////////////////////////////
        private List<string> GetAccountInflunceCitiesWithArmy(string account)
        {
            var cityList = new List<string>();
            string url = string.Format("http://{0}/index.php?mod=influence/influence&op=show&func=influence_city_army&r={1}",
                hostname, randGen.NextDouble());
            string resp = HTTPRequest(url, account);

            var pattern = new Regex("<td width=\"12%\">(.*)</td>");
            var matches = pattern.Matches(resp);
            foreach (Match match in matches)
            {
                string key = match.Groups[1].Value;
                if (cityList.Contains(key))
                {
                    cityList.Add(key);
                }
            }

            return cityList;
        }

        private DateTime GetRemoteSysTime()
        {
            string rsp = RefreshHomePage(activeAccount);
            return ParseSysTimeFromHomePage(rsp);
        }


        private string GetTargetCityID(string srccityid, string dstcityname, string account)
        {
            string response = OpenCityPage(srccityid, account);
            return ParseTargetCityID(response, dstcityname);
        }

        private List<TeamInfo> GetAttackTeamsInfo(string srccityid, string dstcityid, string account)
        {
            string response = OpenCityPage(srccityid, account);
            return ParseAttackTeams(response, dstcityid, account);
        }

        private List<TeamInfo> GetTeamsInfo(string srccityid, string account)
        {
            string cityPage = OpenCityPage(srccityid, account);
            return ParseTeams(cityPage, account);
        }

        private List<TeamInfo> GetGroupTeamsInfo(string srccityid, string dstcityid, string account)
        {
            string response = OpenGroupTeamListPage(srccityid, account);
            return ParseGroupAttackTeams(response, dstcityid, account);
        }

        private bool IsTeamInCity(string srcCityId, string account)
        {
            string url0 = string.Format("http://{0}/index.php?mod=world/world&op=show&func=get_node&r={1}", hostname, randGen.NextDouble());
            HTTPRequest(url0, account);

            string url1 = string.Format(
                "http://{0}/index.php?mod=influence/influence&op=show&func=influence_city_detail&node_id={1}&r={2}",
                hostname,
                srcCityId,
                randGen.NextDouble());
            string resp = HTTPRequest(url1, account);

            var regex = new Regex(@"user_hero_\d+");
            return regex.Matches(resp).Count > 0;
        }

        private List<string> OpenCreateTeamPage(string srcCityId, string account)
        {
            OpenCityPage(srcCityId, account);

            string url = string.Format("http://{0}/index.php?mod=military/world_war&op=show&func=create_team&team_type=1&from_address=1&r={1}",
                hostname, randGen.NextDouble());

            string content = HTTPRequest(url, account);
            return ParseHerosInCreateTeamPage(content);
        }

        private List<string> ParseHerosInCreateTeamPage(string content)
        {
            var pattern = new Regex(@"worldWarClass\.selectHero\('(\d+)',true\);");
            var tempHeroList = new List<string>();

            var matches = pattern.Matches(content);
            foreach (Match match in matches)
            {
                tempHeroList.Add(match.Groups[1].Value);
            }

            var statusPattern = new Regex("<p class=\"trans_70\">(.*)</p>");
            var statusList = new List<string>();
            var statusMatches = statusPattern.Matches(content);
            foreach (Match match in statusMatches)
            {
                statusList.Add(match.Groups[1].Value);
            }

            var heroList = new List<string>();
            for (int i = 0; i < tempHeroList.Count; ++i)
            {
                if (statusList[i] == "空闲")
                {
                    heroList.Add(tempHeroList[i]);
                }
            }

            return heroList;
        }

        private void CreateTeam(string srcCityId, string heroId, string teamType, string account)
        {
            string url = string.Format("http://{0}/index.php?mod=military/world_war&func=create_team&op=do&r={1}", hostname, randGen.NextDouble());
            string body = string.Format("team_type={0}&main_hero={1}&using_embattle_id=&sub_hero=&soldiers=&pk_hero_id=", teamType, heroId);
            HTTPRequest(url, account, body);
        }

        private void BatchCreateTeam(string srcCityId, string teamType, string account)
        {
            var heroList = OpenCreateTeamPage(srcCityId, account);
            foreach (var hero in heroList)
            {
                CreateTeam(srcCityId, hero, "1", account);
            }
        }

        private string OpenCityPage(string srccityid, string account)
        {
            string url0 = string.Format("http://{0}/index.php?mod=world/world&op=show&func=get_node&r={1}", hostname, randGen.NextDouble());
            HTTPRequest(url0, account);

            string fmt1 = "http://" + hostname + "/index.php?mod=influence/influence&op=show&func=influence_city_detail&node_id={0}&r=" + randGen.NextDouble().ToString();
            string url1 = string.Format(fmt1, srccityid);
            HTTPRequest(url1, account);

            string url2 = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=show&func=attack&team_id=0&r={1}",
                hostname, randGen.NextDouble());
            return HTTPRequest(url2, account);
        }

        private string OpenGroupTeamListPage(string srccityid, string account)
        {
            string url0 = string.Format("http://{0}/index.php?mod=world/world&op=show&func=get_node&r={1}", hostname, randGen.NextDouble());
            HTTPRequest(url0, account);

            string fmt1 = "http://" + hostname + "/index.php?mod=influence/influence&op=show&func=influence_city_detail&node_id={0}&r=" + randGen.NextDouble().ToString();
            string url1 = string.Format(fmt1, srccityid);
            HTTPRequest(url1, account);

            string url = string.Format("http://{0}/index.php?mod=military/world_war&op=show&func=join_attack&r={1}",
                hostname, randGen.NextDouble());
            return HTTPRequest(url, account);
        }

        private string OpenTeamDetailPage(string teamId, string account)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=show&func=team_detail&team_id={1}&from_address=1&r={2}",
                hostname, teamId, randGen.NextDouble()
                );
            return HTTPRequest(url, account);
        }

        private string OpenGroupTeamDetailPage(string teamId, string account)
        {
            string url = string.Format("http://{0}/index.php?mod=military/world_war&op=show&func=group_detail&group_id={1}&r={2}",
                hostname, teamId, randGen.NextDouble());
            return HTTPRequest(url, account);
        }

        private string OpenGroupTeamAttackPage(string teamId, string cityId, string account)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=show&func=join_attack_confirm&group_id={1}&to_city_id={2}&join_attack_type=1&r={3}",
                hostname, teamId, cityId, randGen.NextDouble());
            return HTTPRequest(url, account);
        }

        private string ParseTeamAttribute(string content, string attributeName)
        {
            var pattern = new Regex(@"<td>(.*)</td>");
            var headName = string.Format("<td>{0}</td>", attributeName);
            var lines = content.Split('\r', '\n');
            int status = 0;
            foreach (var line in lines)
            {
                switch (status)
                {
                    case 0:
                        if (line.Contains(headName))
                        {
                            status = 1;
                        }
                        break;

                    case 1:
                        Match match = pattern.Match(line);
                        if (match.Success)
                        {
                            return match.Groups[1].Value;
                        }
                        break;
                }
            }

            return "";
        }

        private List<string> OpenAttackPage(string cityId, string account)
        {
            string page = OpenCityPage(cityId, account);
            return ParseTargetCityList(page);
        }

        private string OpenAttackCityPage(string teamid, string cityid, string account)
        {
            string pattern = "http://" + hostname + "/index.php?mod=military/world_war&op=show&func=attack_confirm&team_id={0}&to_city_id={1}&r=" + randGen.NextDouble().ToString();
            string url = string.Format(pattern, teamid, cityid);
            return HTTPRequest(url, account);
        }

        private void AttackTarget(string team_id, string city_id, string account)
        {
            string url = "http://" + hostname + "/index.php?mod=military/world_war&op=do&func=attack&r=" + randGen.NextDouble().ToString();
            string bodyfmt = "team_id={0}&to_city_id={1}";
            string body = string.Format(bodyfmt, team_id, city_id);
            string ret = HTTPRequest(url, account, body);
        }

        private void GroupAttackTarget(string teamId, string cityId, string account)
        {
            string url = string.Format("/index.php?mod=military/world_war&op=do&func=join_attack&r=", randGen.NextDouble());
            string body = string.Format("group_id={0}&to_city_id={1}&join_attack_type=1", teamId, cityId);
            string ret = HTTPRequest(url, account, body);
        }

        private string RefreshHomePage(string account)
        {
            string url = "http://" + hostname + "/index.php";
            return HTTPRequest(url, account);
        }

        private DateTime ParseSysTimeFromHomePage(string content)
        {
            Regex re = new Regex("wee\\.timer\\.set_time\\( [0-9]+ \\);");

            MatchCollection ms = re.Matches(content);
            foreach (Match m in ms)
            {
                string timestr = m.Value.Split('(', ')')[1];
                DateTime ret = new DateTime(1970, 1, 1);
                int sec = Int32.Parse(timestr.Trim(' '));
                ret = ret.AddSeconds(sec);
                return ret.ToLocalTime();
            }
            return new DateTime();
        }

        private string ParseTargetCityID(string content, string keyword)
        {
            Regex re = new Regex("<option value=\\\"([0-9]+)\\\">([^<]*)</option>");

            MatchCollection ms = re.Matches(content);
            foreach (Match m in ms)
            {
                if (m.Groups[2].Value == keyword)
                    return m.Groups[1].Value;
            }
            return "";
        }

        private string ParseAttackDuration(string content)
        {
            Regex re = new Regex("[0-9][0-9]:[0-9][0-9]:[0-9][0-9]");

            MatchCollection ms = re.Matches(content);
            foreach (Match m in ms)
            {
                string[] strs = m.Value.Split(':');
                if (strs.Length == 3)
                {
                    return m.Value;
                }
            }
            return "";
        }

        private int TimeStr2Sec(string input)
        {
            string[] strs = input.Split(':');
            if (strs.Length == 3)
            {
                return Int32.Parse(strs[0]) * 3600 + Int32.Parse(strs[1]) * 60 + Int32.Parse(strs[2]);
            }
            return 0;
        }

        private List<TeamInfo> ParseGroupAttackTeams(string content, string destCityId, string account)
        {
            var pattern = new Regex("worldWarClass.showGroupDetail\\((\\d+)\\)");
            var teamlist = new List<TeamInfo>();
            var teamIdList = new List<string>();

            var matches = pattern.Matches(content);
            foreach (Match match in matches)
            {
                string teamId = match.Groups[1].Value;
                if (teamIdList.Contains(teamId))
                {
                    continue;
                }

                teamIdList.Add(teamId);

                var team = new TeamInfo();
                team.team_id = match.Groups[1].Value;
                team.isGroupTeam = true;

                string teamPageContent = OpenGroupTeamDetailPage(team.team_id, account);
                team.name = ParseTeamAttribute(teamPageContent, "小队名称");
                team.leader = ParseTeamAttribute(teamPageContent, "本小队队长");

                string attachPage = OpenGroupTeamAttackPage(team.team_id, destCityId, account);

                team.duration_str = ParseAttackDuration(attachPage);
                team.duration = TimeStr2Sec(team.duration_str);

                team.account = account;
                team.is_mainstream = false;
                team.time_left = 0;
                team.is_mainstream = false;

                if (team.duration_str != "")
                {
                    teamlist.Add(team);
                }
            }

            return teamlist;
        }

        private List<TeamInfo> ParseTeams(string content, string account)
        {
            Regex re = new Regex("worldWarClass\\.changeMyAttackTeam\\([0-9]+,[0-9]+,[0-9]+\\)");
            var heroPattern = new Regex("<img src=\"http://static\\.tc\\.9wee\\.com/hero/\\d+/\\d+\\.gif\" titleContent=\"(.*)\"/>");
            var teamlist = new List<TeamInfo>();

            MatchCollection ms = re.Matches(content);
            int team_no = 1;
            foreach (Match m in ms)
            {
                TeamInfo team = new TeamInfo();
                team.team_id = m.Value.Split('(', ',')[1];
                team.isGroupTeam = false;

                string teamDetailPage = OpenTeamDetailPage(team.team_id, account);
                var match = heroPattern.Match(teamDetailPage);
                if (match.Success)
                {
                    team.leader = match.Groups[1].Value;
                }

                team.duration_str = "";
                team.duration = 0;
                team.account = account;
                team.is_mainstream = false;
                team.time_left = 0;
                team.is_mainstream = false;

                teamlist.Add(team);
                team_no++;
            }

            return teamlist;
        }

        private List<TeamInfo> ParseAttackTeams(string content, string dstcityid, string account)
        {
            Regex re = new Regex("worldWarClass\\.changeMyAttackTeam\\([0-9]+,[0-9]+,[0-9]+\\)");
            var teamlist = new List<TeamInfo>();

            MatchCollection ms = re.Matches(content);
            int team_no = 1;
            foreach (Match m in ms)
            {
                TeamInfo team = new TeamInfo();
                team.team_id = m.Value.Split('(', ',')[1];
                team.isGroupTeam = false;
                team.leader = team_no.ToString(); //temporary solution
                string rsp = OpenAttackCityPage(team.team_id, dstcityid, account);
                team.duration_str = ParseAttackDuration(rsp);
                team.duration = TimeStr2Sec(team.duration_str);

                team.account = account;
                team.is_mainstream = false;
                team.time_left = 0;
                team.is_mainstream = false;

                if (team.duration_str != "")
                {
                    teamlist.Add(team);
                    team_no++;
                }
            }

            return teamlist;
        }

        void DismissTeam(string teamId, string account)
        {
            string url = string.Format("http://{0}/index.php?mod=military/world_war&op=do&func=disband_team&r={1}", hostname, randGen.NextDouble());
            string body = string.Format("team_id={0}&from_address=1&detail_flag=0", teamId);
            HTTPRequest(url, account, body);
        }

        private List<string> GetMoveTargetCities(string sourceCidyId, string account)
        {
            OpenCityPage(sourceCidyId, account);
            string url = string.Format("http://{0}/index.php?mod=military/world_war&op=show&func=move_army&r={1}", hostname, randGen);
            string content = HTTPRequest(url, account);

            return ParseTargetCityList(content);
        }

        private List<string> ParseTargetCityList(string content)
        {
            var targetBeginLine = "目的地";
            var targetPattern = new Regex("<option value=\"\\d+\">(.*)</option>");
            var lines = content.Split('\r');
            int status = 0;
            var cityList = new List<string>();
            foreach (var line in lines)
            {
                switch (status)
                {
                    case 0:
                        if (line.Contains(targetBeginLine))
                        {
                            status = 1;
                        }
                        break;

                    case 1:
                        var match = targetPattern.Match(line);
                        if (match.Success)
                        {
                            cityList.Add(match.Groups[1].Value);
                        }
                        break;
                }
            }
            return cityList;
        }
        #endregion

        #region helpers
        private string Time2Str(int timeval)
        {
            int secs = timeval % 60;
            int mins = (timeval / 60) % 60;
            int hours = timeval / 3600;
            string fmt = "{0:D2}:{1:D2}:{2:D2}";
            return string.Format(fmt, hours, mins, secs);
        }

        private void SyncTasksToTaskListView()
        {
            listViewTasks.Items.Clear();

            foreach (TeamInfo team in m_teamlist)
            {

                ListViewItem newli = new ListViewItem();
                newli.SubItems[0].Text = team.account;
                newli.SubItems.Add(team.team_id);
                newli.SubItems.Add(team.leader);
                newli.SubItems.Add(Time2Str(team.duration));
                newli.SubItems.Add(Time2Str(team.time_left));
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
            }
        }

        private void SendTroopProc(object obj)
        {
            TeamInfo team = (TeamInfo)obj;

            m_sendTroopLock.WaitOne();
            {
                OpenAttackCityPage(team.team_id, m_dstCityID, team.account);
                AttackTarget(team.team_id, m_dstCityID, team.account);
            }
            m_sendTroopLock.Set();
        }

        private void SendTroop(TeamInfo team)
        {
            Thread oThread = new Thread(new ParameterizedThreadStart(SendTroopProc));
            oThread.Start(team);
        }

        delegate void DoSomething();

        private static int CompareTeamInfo(TeamInfo x, TeamInfo y)
        {
            if (x.duration == y.duration)
            {
                return 0;
            }
            else if (x.duration > y.duration)
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
            int attack_diff = rbtnSeqAttack.Checked ? 1 : 0;
            int attack_ajust = 0;

            //calculate time left to trigger
            foreach (TeamInfo team in m_teamlist)
            {
                if (maxduration > team.duration)
                {
                    team.time_left = maxduration - team.duration + attack_ajust;
                    attack_ajust += attack_diff;
                }
                else
                {
                    team.time_left = 0;
                    team.is_troop_sent = true;
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
                foreach (TeamInfo team in m_teamlist)
                {
                    if (team.time_left <= 0 && !team.is_troop_sent)
                    {
                        OpenAttackCityPage(team.team_id, m_dstCityID, team.account);
                        if (team.isGroupTeam)
                            GroupAttackTarget(team.team_id, m_dstCityID, team.account);
                        else
                            AttackTarget(team.team_id, m_dstCityID, team.account);
                        team.is_troop_sent = true;
                    }

                    if (team.is_troop_sent)
                    {
                        avlcnt++;
                    }
                }

                if (avlcnt >= m_teamlist.Count)
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
            foreach (TeamInfo team in m_teamlist)
            {
                if (team.time_left <= 0)
                {
                    if (team.duration > 0)
                    {
                        team.duration -= diff.Seconds;
                        avlcnt++;
                    }
                }
                else
                {
                    team.time_left -= diff.Seconds;
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

        private void LoadValidCityList()
        {
            var validCityList = new Dictionary<string, bool>();
            foreach (var account in this.accountTable.Values)
            {
                this.Invoke(new DoSomething(() =>
                {
                    this.txtInfo.Text = string.Format("Scan Account:{0}", account.UserName);
                }));

                account.CityIDList = GetAccountInflunceCitiesWithArmy(account.UserName);
                foreach (var city in account.CityIDList)
                {
                    if (!validCityList.ContainsKey(city))
                        validCityList.Add(city, true);
                }
            }

            this.Invoke(new DoSomething(() =>
            {
                this.txtInfo.Text = string.Format("Scan Completed");
            }));

            var validCityNames = new List<string>();
            foreach (var cityId in validCityList)
            {
                foreach (var city in cityList)
                {
                    if (city.Value == cityId.Key)
                    {
                        validCityNames.Add(city.Key);
                        break;
                    }
                }
            }

            this.Invoke(new DoSomething(() =>
            {
                listBoxSrcCities.Items.Clear();
                foreach (var city in validCityNames)
                {
                    listBoxSrcCities.Items.Add(city);
                }

                btnScanCity.Enabled = true;
            }));
        }

        /////////////////////////////////////////////////////////////////////
        private void AutoAttackProc()
        {
            // fetch city id through all accounts
            foreach (string account in this.accountTable.Keys)
            {
                if (m_dstCityID.Length == 0)
                    m_dstCityID = GetTargetCityID(m_srcCityID, m_dstCityName, account);

                var teamlist = GetAttackTeamsInfo(m_srcCityID, m_dstCityID, account);
                m_teamlist.AddRange(teamlist);

                var groupTeamList = GetGroupTeamsInfo(m_srcCityID, m_dstCityID, account);
                m_teamlist.AddRange(groupTeamList);
            }

            if (m_teamlist.Count == 0)
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

        private Dictionary<string, string[]> m_cityMap = new Dictionary<string, string[]>();
        private void LoadCityMap()
        {
            using (var sr = new StreamReader("city_map.txt", System.Text.Encoding.Default))
            {
                string line = sr.ReadLine();
                while (line != null)
                {
                    string[] strs = line.Split(':', ',');
                    if (strs.Length > 1)
                    {
                        m_cityMap.Add(strs[0], strs);
                    }

                    line = sr.ReadLine();
                }
            }
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
            using (var sr = new StreamReader("multi_login.conf", System.Text.Encoding.Default))
            {
                string line = sr.ReadLine();
                while (line != null)
                {

                    string[] strs = line.Split('|');
                    if (strs.Length == 6)
                    {
                        LoginParam conf = new LoginParam()
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

        delegate void LoginAccountDeleget(string key);

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

        // private List<AttackTask> CreateAttackTasks()
        // {
        //     TimeSpan diff = this.dateTimePickerArrival.Value - m_SysTime;
        //     int maxDuration = diff.TotalSeconds <= 0 ? SortTeamList(0) : SortTeamList((int)diff.TotalSeconds);

        //     SyncTasksToTaskListView();
        //     if (m_teamlist.Count > 0)
        //     {
        //         StartUITimer();
        //         StartSendTroopTimer();
        //     }

        //     var task = new AttackTask();
        //     task.StartTime = DateTime.Now;
        //     task.EndTime = task.StartTime.AddSeconds(maxDuration);
        //     return task;
        // }

        #endregion

        #region UIActions
        private void FormMain_Load(object sender, EventArgs e)
        {
            LoadMultiLoginConf();

            LoadCityList();


            listViewTasks.Columns.Add("帐号名", 100);
            listViewTasks.Columns.Add("部队标识", 60);
            listViewTasks.Columns.Add("部队在出发城的顺序", 120);
            listViewTasks.Columns.Add("途中耗时", 100);
            listViewTasks.Columns.Add("出发倒计时", 100);

            listViewAccounts.Columns.Add("帐号名", 100);
            listViewAccounts.Columns.Add("状态", 60);

            checkBoxShowBrowers.Checked = false;
            btnLoginAll.Enabled = false;
            btnAutoAttack.Enabled = false;
            btnConfirmMainTeams.Enabled = false;
            rbtnSyncAttack.Checked = true;

            logStream.AutoFlush = true;

        }

        private void btnScanCity_Click(object sender, EventArgs e)
        {
            btnScanCity.Enabled = false;
            var bgThread = new Thread(new ThreadStart(LoadValidCityList));
            bgThread.Start();
        }

        private void btnLoginAll_Click(object sender, EventArgs e)
        {
            btnLoginAll.Enabled = false;
            btnLoginAll.Text = "登录中";
            Thread oThread = new Thread(new ThreadStart(BatchLoginProc));
            oThread.Start();
        }

        private void checkBoxShowBrowers_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxShowBrowers.Checked)
            {
                this.Height = 760;
            }
            else
            {
                this.Height = 570;
            }
        }

        private void btnQuickArmy_Click(object sender, EventArgs e)
        {
            var cityName = this.listBoxSrcCities.SelectedItem as string;
            if (string.IsNullOrEmpty(cityName))
            {
                return;
            }

            if (!cityList.ContainsKey(cityName))
            {
                return;
            }

            var cityId = cityList[cityName];
            this.btnQuickArmy.Enabled = false;

            new Thread(new ThreadStart(() =>
            {
                foreach (var account in this.accountTable)
                {
                    if (!account.Value.CityIDList.Contains(cityId))
                    {
                        continue;
                    }

                    this.Invoke(new DoSomething(() =>
                    {
                        this.txtInfo.Text = string.Format("Create Team for Account: {0} at {1}", account.Key, cityName);
                    }));

                    var heroList = OpenCreateTeamPage(cityId, account.Key);
                    foreach (var hero in heroList)
                    {
                        this.Invoke(new DoSomething(() =>
                        {
                            this.txtInfo.Text = string.Format("Create Team for Account: {0} at {1}, Hero={2}", account.Key, cityName, hero);
                        }));

                        CreateTeam(cityId, hero, "1", account.Key);
                    }
                }

                this.Invoke(new DoSomething(() =>
                {
                    this.txtInfo.Text = string.Format("Create Team Completed");
                    this.btnQuickArmy.Enabled = true;
                }));

                m_teamlist.Clear();
                foreach (var account in this.accountTable.Keys)
                {
                    var teamList = GetTeamsInfo(cityId, account);
                    m_teamlist.AddRange(teamList);
                }

                this.Invoke(new DoSomething(SyncTasksToTaskListView));
            })).Start();
        }

        private void listBoxDstCities_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listBoxSrcCities.SelectedItems.Count > 0 &&
                this.listBoxDstCities.SelectedItems.Count > 0 &&
                this.listViewTasks.CheckedItems.Count > 0)
            {
                btnAutoAttack.Enabled = true;
            }
        }

        private void listBoxSrcCities_SelectedIndexChanged(object sender, EventArgs e)
        {
            string cityname = this.listBoxSrcCities.SelectedItem as string;
            if (cityname == null)
            {
                return;
            }

            if (cityList.ContainsKey(cityname))
            {
                this.m_teamlist.Clear();
                this.listViewTasks.Items.Clear();
                this.listViewTasks.Enabled = false;
                this.listBoxDstCities.Enabled = false;
                this.listBoxSrcCities.Enabled = false;

                string cityId = cityList[cityname];
                new Thread(new ThreadStart(() =>
                {
                    var targetCityNameList = new List<string>();
                    foreach (var account in this.accountTable)
                    {
                        if (account.Value.CityIDList.Contains(cityId))
                        {
                            if (targetCityNameList.Count == 0)
                            {
                                var attackCityList = OpenAttackPage(cityId, account.Key);
                                targetCityNameList.AddRange(attackCityList);
                                var moveCityList = GetMoveTargetCities(cityId, account.Key);
                                targetCityNameList.AddRange(moveCityList);
                            }

                            var teamList = GetTeamsInfo(cityId, account.Key);
                            m_teamlist.AddRange(teamList);
                        }
                    }

                    this.Invoke(new DoSomething(SyncTasksToTaskListView));
                    this.Invoke(new DoSomething(() =>
                    {
                        this.listBoxDstCities.Items.Clear();
                        foreach (var name in targetCityNameList)
                        {
                            this.listBoxDstCities.Items.Add(name);
                        }

                        if (this.listBoxDstCities.SelectedItems.Count > 0)
                        {
                            btnAutoAttack.Enabled = true;
                        }

                        this.listViewTasks.Enabled = true;
                        this.listBoxDstCities.Enabled = true;
                        this.listBoxSrcCities.Enabled = true;
                    }));
                })).Start();
            }
        }

        private void btnDismissTeam_Click(object sender, EventArgs e)
        {
            if (this.listViewTasks.CheckedItems.Count <= 0)
            {
                return;
            }

            var targetTeams = new List<TeamInfo>();
            foreach (ListViewItem item in this.listViewTasks.CheckedItems)
            {
                var team = item.Tag as TeamInfo;
                if (team == null || team.is_troop_sent)
                {
                    continue;
                }

                targetTeams.Add(team);
            }

            if (targetTeams.Count == 0)
            {
                return;
            }

            this.btnDismissTeam.Enabled = false;
            this.listViewTasks.Enabled = false;

            new Thread(new ThreadStart(() =>
            {
                foreach (var team in targetTeams)
                {
                    DismissTeam(team.team_id, team.account);
                    this.Invoke(new DoSomething(() =>
                    {
                        foreach (ListViewItem item in this.listViewTasks.CheckedItems)
                        {
                            var teamInfo = item.Tag as TeamInfo;
                            if (teamInfo == team)
                            {
                                this.listViewTasks.Items.Remove(item);
                            }
                        }
                    }));
                }

                this.Invoke(new DoSomething(() =>
                {
                    btnDismissTeam.Enabled = true;
                    this.listViewTasks.Enabled = true;
                }));
            })).Start();
        }

        private void listViewTasks_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            int candidateTeamCount = 0;
            foreach (ListViewItem item in this.listViewTasks.CheckedItems)
            {
                var team = item.Tag as TeamInfo;
                if (team == null || team.is_troop_sent)
                {
                    continue;
                }

                ++candidateTeamCount;
            }

            this.btnDismissTeam.Enabled = candidateTeamCount > 0;
        }

        private void checkBoxSelectAllTasks_CheckedChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listViewTasks.Items)
            {
                item.Checked = this.checkBoxSelectAllTasks.Checked;
            }
        }

        private void AutoAttack_Click(object sender, EventArgs e)
        {
            if (listBoxSrcCities.SelectedItem == null ||
                listBoxDstCities.SelectedItem == null)
            {
                MessageBox.Show("Please select source city and destination city");
                return;
            }

            btnAutoAttack.Enabled = false;

            m_srcCityName = listBoxSrcCities.SelectedItem.ToString();
            m_srcCityID = cityList[m_srcCityName];
            m_dstCityName = listBoxDstCities.SelectedItem.ToString();

            Thread oThread = new Thread(new ThreadStart(AutoAttackProc));
            oThread.Start();
        }

        private void btnConfirmMainTeams_Click(object sender, EventArgs e)
        {
            if (btnConfirmMainTeams.Text != "取消")
            {
                TimeSpan diff = this.dateTimePickerArrival.Value - remoteTime;
                if (diff.TotalSeconds <= 0)
                {
                    SortTeamList(0);
                }
                else
                {
                    SortTeamList((int)diff.TotalSeconds);
                }

                SyncTasksToTaskListView();
                if (m_teamlist.Count > 0)
                {
                    StartUITimer();
                    StartSendTroopTimer();
                }

                btnConfirmMainTeams.Text = "取消";
                rbtnSeqAttack.Enabled = false;
                rbtnSyncAttack.Enabled = false;
            }
            else
            {
                StopUITimer();
                StopSendTroopTimer();

                btnConfirmMainTeams.Text = "确认攻击";
                rbtnSeqAttack.Enabled = true;
                rbtnSyncAttack.Enabled = true;
            }
        }

        ////////////////////////////////////////////////////////////////////////
        //load account list
        private void btnLoadProfile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            DialogResult ret = dlg.ShowDialog();
            if (ret == DialogResult.OK)
            {
                StreamReader sr = new StreamReader(dlg.FileName, System.Text.Encoding.Default);
                string line = sr.ReadLine();
                while (line != null)
                {
                    string[] strs = line.Split('|', ':');
                    if (strs.Length > 1)
                    {
                        AccountInfo accountinfo = new AccountInfo();
                        {
                            accountinfo.UserName = strs[0];
                            accountinfo.Password = strs[1];
                            if (strs.Length > 2)
                                accountinfo.AccountType = strs[2];
                            else
                                accountinfo.AccountType = "tianya";

                            accountinfo.CookieStr = "";
                            accountinfo.LoginStatus = "off-line";
                        }
                        this.accountTable.Add(strs[0], accountinfo);
                    }
                    line = sr.ReadLine();
                }

                SyncAccountsStatus();
                if (this.accountTable.Keys.Count > 0)
                {
                    btnLoadProfile.Enabled = false;
                    btnLoginAll.Enabled = true;
                }

            }
        }
        #endregion

    }

    class TeamInfo
    {
        public string name;
        public string account;
        public string team_id;
        public string leader;
        public string duration_str;
        public int time_left;
        public int duration;
        public bool is_mainstream;
        public bool is_troop_sent;
        public bool isGroupTeam;
    }

    class AccountInfo
    {
        public string UserName;
        public string Password;
        public string AccountType;
        public string CookieStr;
        public string LoginStatus;
        public List<string> CityIDList = new List<string>();
    }

    class LoginParam
    {
        public string Name;
        public string LoginURL;
        public string UsernameElmID;
        public string PasswordElmID;
        public string LoginTitle;
        public string HomeTitle;
        public string LogoutURL;
    }

    class AttackTask
    {
        public AccountInfo Account;
        public List<TeamInfo> TeamList = new List<TeamInfo>();
        public Thread Worker;
        public string FromCity;
        public string ToCity;
        public DateTime StartTime;
        public DateTime EndTime;
    }
}
