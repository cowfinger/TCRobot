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
using System.Linq;
using System.Threading.Tasks;

namespace TC
{
    public partial class FormMain : Form
    {
        private static string UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.2; .NET4.0C; .NET4.0E)";

        private Dictionary<string, string> cityList = new Dictionary<string, string>();
        private List<TeamInfo> teamList = new List<TeamInfo>();

        private string hostname = "yw1.tc.9wee.com";
        private Dictionary<string, LoginParam> multiLoginConf = new Dictionary<string, LoginParam>();

        private string m_strCookies = string.Empty;
        private string m_srcCityID = "";
        private string m_srcCityName = "";
        private string destCityID = "";
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

        private void FormMain_Load(object sender, EventArgs e)
        {
            LoadMultiLoginConf();
            LoadCityList();

            checkBoxShowBrowers.Checked = false;
            btnLoginAll.Enabled = false;
            btnAutoAttack.Enabled = false;
            btnConfirmMainTeams.Enabled = false;
            rbtnSyncAttack.Checked = true;
            btnContribute.Enabled = false;
        }

        private void btnScanCity_Click(object sender, EventArgs e)
        {
            btnScanCity.Enabled = false;
            Task.Run(() =>
            {
                var validCityNameList = this.accountTable.Values.SelectMany(
                    account =>
                    {
                        this.Invoke(new DoSomething(() =>
                        {
                            this.txtInfo.Text = string.Format("Scan Account:{0}", account.UserName);
                        }));

                        var cityNameList = GetAccountInflunceCityNameListWithArmy(account.UserName);
                        account.CityIDList = cityNameList.Select(cityName => this.cityList[cityName]);

                        return cityNameList;
                    }).ToList().Distinct();

                this.Invoke(new DoSomething(() =>
                {
                    listBoxSrcCities.Items.Clear();
                    foreach (var city in validCityNameList)
                    {
                        listBoxSrcCities.Items.Add(city);
                    }

                    this.txtInfo.Text = string.Format("Scan Completed");
                    btnScanCity.Enabled = true;
                }));
            });
        }

        private void btnLoginAll_Click(object sender, EventArgs e)
        {
            btnLoginAll.Enabled = false;
            btnLoginAll.Text = "登录中";
            Task.Run(() => { BatchLoginProc(); });
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

        private void btnQuickAttack_Click(object sender, EventArgs e)
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
            this.btnQuickAttack.Enabled = false;

            Task.Run(() =>
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

                    var page = OpenCreateTeamPage(cityId, account.Key);
                    var heroList = ParseHerosInCreateTeamPage(page);
                    var soldierList = ParseSoldiersInCreateTeamPage(page).ToList();

                    soldierList.Sort((x, y) => { return x.SoldierNumber.CompareTo(y.SoldierNumber); });
                    soldierList.Reverse();

                    foreach (var hero in heroList)
                    {
                        string soldierString = BuildSoldierString(ref soldierList, 1000);
                        this.Invoke(new DoSomething(() =>
                        {
                            this.txtInfo.Text = string.Format(
                                "Create Team for Account: {0} at {1}, Hero={2}, Soldier={3}",
                                account.Key,
                                cityName,
                                hero,
                                soldierString
                                );
                        }));

                        CreateTeam(cityId, hero, soldierString, this.checkBoxDefend.Checked ? "2" : "1", account.Key);
                    }
                }

                this.Invoke(new DoSomething(() =>
                {
                    this.txtInfo.Text = string.Format("Create Team Completed");
                    this.btnQuickAttack.Enabled = true;
                }));

                this.teamList.Clear();
                foreach (var account in this.accountTable.Keys)
                {
                    this.teamList.AddRange(GetTeamsInfo(cityId, account));
                }

                this.Invoke(new DoSomething(SyncTasksToTaskListView));
            });
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

            if (!cityList.ContainsKey(cityname))
            {
                return;
            }

            this.teamList.Clear();
            this.listViewTasks.Items.Clear();
            this.listViewTasks.Enabled = false;
            this.listBoxDstCities.Enabled = false;
            this.listBoxSrcCities.Enabled = false;

            string cityId = cityList[cityname];
            Task.Run(() =>
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

                        this.teamList.AddRange(GetActiveTeamInfo(cityId, "1", account.Key));
                        this.teamList.AddRange(GetActiveTeamInfo(cityId, "2", account.Key));
                        this.teamList.AddRange(GetGroupTeamsInfo(cityId, account.Key));
                    }
                }

                targetCityNameList.Distinct();

                this.Invoke(new DoSomething(() =>
                {
                    SyncTasksToTaskListView();
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
            });
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
                if (team == null || team.IsTroopSent)
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
                    DismissTeam(team.TeamId, team.AccountName);
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
                if (team == null || team.IsTroopSent)
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
                if (teamList.Count > 0)
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

        private void btnContribute_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                var accountList = this.accountTable.Keys.ToList();

                for (int i = 0; i < 1000 && accountList.Any(); ++i)
                {
                    BatchDonate(i, ref accountList);
                }

                this.Invoke(new DoSomething(() =>
                {
                    this.txtInfo.Text = string.Format("Donate: Completed");
                }));
            });
        }

        private void btnGroupTeam_Click(object sender, EventArgs e)
        {
            var candidateTeams = new List<TeamInfo>();
            foreach (ListViewItem item in this.listViewTasks.CheckedItems)
            {
                candidateTeams.Add(item.Tag as TeamInfo);
            }

            string cityId = this.cityList[this.listBoxSrcCities.SelectedItem.ToString()];

            this.btnGroupTeam.Enabled = false;
            Task.Run(() =>
                {

                    var accountTeamTable = CategorizeTeams(candidateTeams);
                    if (accountTable.Count < 2)
                    {
                        return;
                    }

                    TeamInfo headTeam = null;
                    var teamGroup = new List<TeamInfo>();
                    foreach (var accountTeams in accountTeamTable.Values)
                    {
                        foreach (var team in accountTeams)
                        {
                            if (team.isGroupTeam && headTeam == null)
                            {
                                headTeam = team;
                                break;
                            }

                            if (!team.isGroupTeam && !team.isDefendTeam)
                            {
                                teamGroup.Add(team);
                                break;
                            }
                        }
                    }

                    if (teamGroup.Count() + (headTeam == null ? 0 : 1) < 2)
                    {
                        return;
                    }

                    teamGroup.Sort((x, y) => x.PowerIndex.CompareTo(y.PowerIndex));
                    teamGroup.Reverse();

                    if (headTeam == null)
                    {
                        headTeam = teamGroup.First();
                        teamGroup.RemoveAt(0);

                        string groupName = CreateGroupHead(cityId, headTeam.TeamId, headTeam.AccountName);
                        var groupTeams = GetGroupTeamsInfo(cityId, headTeam.AccountName);
                        headTeam = groupTeams.Where(team => team.Name == groupName).FirstOrDefault();
                        if (headTeam.Name != groupName)
                        {
                            return;
                        }
                    }

                    foreach (var team in teamGroup)
                    {
                        JoinGroup(cityId, headTeam.GroupId, team.TeamId, team.AccountName);
                    }

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

                            this.teamList.AddRange(GetActiveTeamInfo(cityId, "1", account.Key));
                            this.teamList.AddRange(GetActiveTeamInfo(cityId, "2", account.Key));
                            this.teamList.AddRange(GetGroupTeamsInfo(cityId, account.Key));
                        }
                    }

                    this.Invoke(new DoSomething(() =>
                    {
                        SyncTasksToTaskListView();
                    }));
                });
        }
    }
}
