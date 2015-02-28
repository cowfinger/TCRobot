namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Windows.Forms;

    partial class FormMain
    {
        private Dictionary<string, CityInfo> cachedCityMap = new Dictionary<string, CityInfo>();

        private string HTTPRequest(string url, string account, string body = null)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = UserAgent;
                request.Headers.Add("Cookie", this.GetAccountCookie(account));

                if (!string.IsNullOrEmpty(body))
                {
                    var codedBytes = new ASCIIEncoding().GetBytes(body);
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = codedBytes.Length;
                    request.GetRequestStream().Write(codedBytes, 0, codedBytes.Length);
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    this.SetAccountCookie(account, response.Headers["Set-Cookie"]);

                    var content = string.Empty;
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        content = reader.ReadToEnd();
                    }

                    response.Close();
                    return content;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        private string Time2Str(int timeval)
        {
            var secs = timeval % 60;
            var mins = (timeval / 60) % 60;
            var hours = timeval / 3600;
            var fmt = "{0:D2}:{1:D2}:{2:D2}";
            return string.Format(fmt, hours, mins, secs);
        }

        private string CalcGroupType(TroopInfo team)
        {
            if (team.isGroupTroop)
            {
                return "群组";
            }

            if (team.isDefendTroop)
            {
                return "防御";
            }

            return "攻击";
        }

        private void SyncTroopInfoToUI(IEnumerable<TroopInfo> troopList)
        {
            foreach (var team in troopList)
            {
                this.TrySyncTroopInfoToUI(team);
            }
        }

        private void TrySyncTroopInfoToUI(TroopInfo team)
        {
            ListViewItem lvItemTroop = null;
            foreach (ListViewItem tempLvItem in this.listViewTroops.Items)
            {
                var troop = tempLvItem.Tag as TroopInfo;
                if (troop != null && troop == team)
                {
                    lvItemTroop = tempLvItem;
                    break;
                }
            }

            if (lvItemTroop == null)
            {
                lvItemTroop = new ListViewItem();
                lvItemTroop.Tag = team;
                lvItemTroop.SubItems[0].Text = team.AccountName;
                lvItemTroop.SubItems.Add(team.TroopId);
                lvItemTroop.SubItems.Add(team.PowerIndex.ToString());
                lvItemTroop.SubItems.Add(this.Time2Str(team.Duration));
                lvItemTroop.SubItems.Add(this.Time2Str(0));
                lvItemTroop.SubItems.Add(team.GroupId);
                lvItemTroop.SubItems.Add(this.CalcGroupType(team));

                this.listViewTroops.Items.Add(lvItemTroop);
            }
            else
            {
                lvItemTroop.SubItems[3].Text = this.Time2Str(team.Duration);
                lvItemTroop.SubItems[4].Text = this.Time2Str(0);
            }
        }

        private void RefreshTroopInfoToUI(IEnumerable<TroopInfo> troopList)
        {
            this.listViewTroops.Items.Clear();
            foreach (var team in troopList)
            {
                var newli = new ListViewItem();
                newli.SubItems[0].Text = team.AccountName;
                newli.SubItems.Add(team.TroopId);
                newli.SubItems.Add(team.PowerIndex.ToString());
                newli.SubItems.Add(this.Time2Str(team.Duration));
                newli.SubItems.Add(this.Time2Str(0));
                newli.SubItems.Add(team.GroupId);
                newli.SubItems.Add(this.CalcGroupType(team));
                newli.Tag = team;
                this.listViewTroops.Items.Add(newli);
            }
        }

        private string ConvertStatusStr(string status)
        {
            if (status == "on-line")
            {
                return "已登录";
            }
            if (status == "in-login")
            {
                return "登录中";
            }
            if (status == "login-failed")
            {
                return "登录失败";
            }
            if (status == "submitting")
            {
                return "提交中";
            }
            if (status == "sync-time")
            {
                return "同步系统时间中";
            }
            return "未登录";
        }

        private void SyncAccountsStatus()
        {
            this.listViewAccounts.Items.Clear();
            var loginnum = 0;
            foreach (var accountkey in this.accountTable.Keys)
            {
                var account = this.accountTable[accountkey];

                var newli = new ListViewItem();
                {
                    newli.SubItems[0].Text = account.UserName;
                    newli.SubItems.Add(this.ConvertStatusStr(account.LoginStatus));
                }
                newli.Tag = account;
                this.listViewAccounts.Items.Add(newli);

                if (account.LoginStatus == "on-line")
                {
                    loginnum++;
                    this.hostname = account.AccountType;
                }
            }

            if (loginnum >= this.accountTable.Keys.Count)
            {
                this.ToolStripMenuItemFunctions.Enabled = true;
                this.btnQuickCreateTroop.Enabled = true;
            }
        }

        private IEnumerable<TroopInfo> QueryCityTroops(string cityId)
        {
            return this.accountTable.Values.Where(account => account.CityIDList.Contains(cityId)).Select(
                account =>
                    {
                        var singleAttackTeams = this.GetActiveTroopInfo(cityId, "1", account.UserName);
                        var singleDefendTeams = this.GetActiveTroopInfo(cityId, "2", account.UserName);
                        var groupAttackteams = this.GetGroupTeamList(cityId, account.UserName);
                        return singleAttackTeams.Concat(singleDefendTeams).Concat(groupAttackteams);
                    }).SelectMany(teams => teams);
        }

        private IEnumerable<string> QueryTargetCityList(string cityId)
        {
            var relatedAccountList = this.accountTable.Values.Where(account => account.CityIDList.Contains(cityId));
            if (relatedAccountList.Any())
            {
                var account = relatedAccountList.First();
                var attackCityList = this.OpenAttackPage(cityId, account.UserName);
                var greoupAttackCityList = this.GetGroupAttackTargetCity(cityId, account.UserName);
                // var moveCityList = GetMoveTargetCities(cityId, account.UserName);
                return attackCityList.Concat(greoupAttackCityList).Distinct();
            }
            return new List<string>();
        }

        private void LoadCheckpoint()
        {
        }

        private void LoadCityList()
        {
            using (var sr = new StreamReader("cities.txt", Encoding.Default))
            {
                var line = sr.ReadLine();
                while (line != null)
                {
                    var strs = line.Split('|', ':');
                    if (strs.Length > 1)
                    {
                        this.cityList.Add(strs[1], strs[0]);
                    }

                    line = sr.ReadLine();
                }
            }
        }

        private void LoadMultiLoginConf()
        {
            using (var sr = new StreamReader("multi_login.conf", Encoding.Default))
            {
                var line = sr.ReadLine();
                while (line != null)
                {
                    var strs = line.Split('|');
                    if (strs.Length == 6)
                    {
                        var conf = new LoginParam
                                       {
                                           Name = strs[0].Trim(' '),
                                           LoginURL = strs[1].Trim(' '),
                                           UsernameElmID = strs[2].Trim(' '),
                                           PasswordElmID = strs[3].Trim(' '),
                                           LoginTitle = strs[4].Trim(' '),
                                           HomeTitle = strs[5].Trim(' ')
                                       };

                        this.multiLoginConf.Add(conf.Name, conf);
                    }

                    line = sr.ReadLine();
                }
            }
        }

        private void BatchLoginProc()
        {
            foreach (var key in this.accountTable.Keys)
            {
                this.LoginAccount(key);
            }
        }

        private bool IsOwnCity(string name)
        {
            var cityId = this.cityList[name];
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
            for (var i = 0; i < 4; ++i)
            {
                var toDonate = resNeeds[i] > resHave[i] ? resHave[i] : resNeeds[i];
                yield return toDonate > 10000 ? toDonate : 0;
            }
        }

        private void BatchDonate(int i, ref List<string> accountList)
        {
            foreach (var account in accountList)
            {
                var influenceSciencePage = this.OpenInfluenceSciencePage(account);
                var influenceRes = this.ParseInfluenceResource(influenceSciencePage).ToList();
                var resNeeds = influenceRes.Select(resPair => resPair.Value - resPair.Key).ToList();

                if (resNeeds[3] < 1000000)
                {
                    accountList.Remove(account);
                    continue;
                }

                var accountRes = this.GetAccountResources(account).ToList();
                while (accountRes[3] < 10000)
                {
                    if (!this.OpenResourceBox(account))
                    {
                        accountList.Remove(account);
                        break;
                    }

                    accountRes = this.GetAccountResources(account).ToList();
                }

                var resToContribute = this.CalculateDonations(resNeeds, accountRes).ToList();

                this.DonateResource(
                    account,
                    resToContribute[0],
                    resToContribute[1],
                    resToContribute[2],
                    resToContribute[3]);
            }
        }

        private void UpdateHeroTable(IEnumerable<HeroInfo> heroList)
        {
            foreach (var hero in heroList)
            {
                this.Invoke(
                    new DoSomething(
                        () =>
                            {
                                foreach (ListViewItem lvItem in this.listViewAccountHero.Items)
                                {
                                    var tabHero = lvItem.Tag as HeroInfo;
                                    if (tabHero != null && tabHero.HeroId == hero.HeroId
                                        && tabHero.IsDead != hero.IsDead)
                                    {
                                        lvItem.Tag = hero;
                                        break;
                                    }
                                }
                            }));
            }
        }

        private void TryBuildInfluenceMaps()
        {
            Parallel.Dispatch(
                this.accountTable.Values,
                account =>
                    {
                        var accountCityList = this.QueryInfluenceCityList(account.UserName).ToList();
                        account.InfluenceMap = this.BuildInfluenceCityMap(accountCityList, account.UserName);
                    }).Then(() => { this.LoadAccountListToMoveArmyTab(); });
        }

        private void LoadAccountListToMoveArmyTab()
        {
            this.Invoke(
                new DoSomething(
                    () =>
                        {
                            this.comboBoxAccount.Items.Clear();
                            foreach (var account in this.accountTable.Keys)
                            {
                                this.comboBoxAccount.Items.Add(account);
                            }
                        }));
        }

        private delegate void DoSomething();
    }
}