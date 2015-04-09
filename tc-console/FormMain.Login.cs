namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Policy;
    using System.Threading;
    using System.Windows.Forms;

    partial class FormMain
    {
        private const string CookieFolder = @".\Cookie";

        private void LoginAccount(string account)
        {
            //this.loginLock.WaitOne();
            {
                this.activeAccount = account;
                var accountInfo = this.accountTable[this.activeAccount];

                var fileName = Path.Combine(CookieFolder, account);
                if (File.Exists(fileName))
                {
                    var cc = HttpClient.LoadCookies(fileName);
                    accountInfo.WebAgent = new RequestAgent(accountInfo)
                    {
                        WebClient = { Cookies = cc }
                    };

                    var homePage = this.RefreshHomePage(account);
                    if (homePage.Contains("wee.timer.set_time"))
                    {
                        accountInfo.LoginStatus = "on-line";
                        this.OnLoginCompleted(accountInfo);
                        // this.loginLock.Set();
                        return;
                    }
                }

                accountInfo.WebAgent = new RequestAgent(accountInfo);
                if (accountInfo.WebAgent.Login())
                {
                    accountInfo.LoginStatus = "on-line";
                    this.OnLoginCompleted(accountInfo);
                    HttpClient.SaveCookies(accountInfo.WebAgent.WebClient.Cookies, fileName);
                }
                else
                {
                    accountInfo.LoginStatus = "failed";
                }

                // this.loginLock.Set();
            }
        }

        private void OnLoginCompleted(AccountInfo account)
        {
            var handledAccountNumber =
                this.accountTable.Values.Sum(a => a.LoginStatus == "on-line" || a.LoginStatus == "login-failed" ? 1 : 0);

            var mainPage = this.RefreshHomePage(account.UserName);
            account.UnionId = ParseUnionIdFromMainPage(mainPage);

            if (account.WebAgent == null)
            {
                account.WebAgent = new RequestAgent(account);
            }

            Task.Run(
                () =>
                {
                    var cityArmyPage = TCPage.Influence.ShowInfluenceCityArmy.Open(account.WebAgent);
                    account.CityNameList = cityArmyPage.Cities.ToList();
                    account.CityIdList = account.CityNameList.Select(cityName => this.cityList[cityName]).ToList();

                    var accountCityList = TCDataType.InfluenceMap.QueryCityList(account).ToList();
                    account.InfluenceCityList = accountCityList.ToDictionary(city => city.Name);
                    account.InfluenceMap = TCDataType.InfluenceMap.BuildMap(accountCityList, account);
                    account.Level = this.GetAccountLevel(account);
                    account.MainCity = accountCityList.Any() ?
                        accountCityList.Single(cityInfo => cityInfo.CityId == 0) : null;
                }).Then(
                        () =>
                        {
                            this.Invoke(
                                new DoSomething(
                                    () =>
                                    {
                                        foreach (ListViewItem lvItem in this.listViewAccounts.Items)
                                        {
                                            var tagAccount = lvItem.Tag as AccountInfo;
                                            if (tagAccount == account)
                                            {
                                                lvItem.SubItems[3].Text = account.Level.ToString();
                                                break;
                                            }
                                        }
                                    }));
                        });
            ;

            if (handledAccountNumber >= this.accountTable.Keys.Count)
            {
                this.remoteTimeLastSync = DateTime.Now;
                RemoteTime = this.QueryRemoteSysTime(this.accountTable.Keys.First()).ToLocalTime();
                this.StartUITimeSyncTimer();
                this.StartTaskTimer();
                // this.StartOnlineTaskCheckTimer();
                // this.StartAuthTimer();
            }

            this.Invoke(
                new DoSomething(
                    () =>
                    {
                        foreach (ListViewItem lvItem in this.listViewAccounts.Items)
                        {
                            var tagAccount = lvItem.Tag as AccountInfo;
                            if (tagAccount == account)
                            {
                                lvItem.SubItems[1].Text = ConvertStatusStr(account.LoginStatus);
                                lvItem.SubItems[2].Text = account.UnionId.ToString();
                                lvItem.SubItems[3].Text = account.Level.ToString();
                                break;
                            }
                        }

                        if (handledAccountNumber >= this.accountTable.Keys.Count)
                        {
                            this.btnAutoAttack.Enabled = true;
                            this.btnQuickCreateTroop.Enabled = true;
                            this.ToolStripMenuItemFunctions.Enabled = true;
                            this.ToolStripMenuItemScan.Enabled = true;
                        }
                    }));
        }

        private void SetAccountCookie(string account, CookieContainer cookie)
        {
            AccountInfo accountInfo;
            lock (this.accountTable)
            {
                if (!this.accountTable.TryGetValue(account, out accountInfo))
                {
                    return;
                }
            }

            lock (accountInfo)
            {
                accountInfo.WebAgent.WebClient.Cookies = cookie;
                TrySaveAccountCookie(accountInfo);
            }
        }

        private CookieContainer GetAccountCookies(string account)
        {
            AccountInfo accountInfo;
            lock (this.accountTable)
            {
                accountInfo = this.accountTable[account];
            }

            if (accountInfo.WebAgent != null)
            {
                return accountInfo.WebAgent.WebClient.Cookies;
            }

            return new CookieContainer();
        }

        private static Dictionary<string, string> ParseCookieStr(string cookiestr)
        {
            var output = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(cookiestr))
            {
                return output;
            }

            var strs = cookiestr.Split(';');
            foreach (var i in strs)
            {
                var key = "";
                var val = "";

                var seppos = i.IndexOf('=');
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
                {
                    continue;
                }

                if (output.ContainsKey(key))
                {
                    output[key] = val;
                }
                else
                {
                    output.Add(key, val);
                }
            }
            return output;
        }

        private static string ComposeCookieStr(Dictionary<string, string> input)
        {
            var output = "";
            foreach (var i in input.Keys.Where(i => i != "path"))
            {
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

        private static void TrySaveAccountCookie(AccountInfo account)
        {
            // if (!Directory.Exists(CookieFolder))
            // {
            //     Directory.CreateDirectory(CookieFolder);
            // }

            // var accountCookieFileName = Path.Combine(CookieFolder, account.UserName);
            // if (File.Exists(accountCookieFileName))
            // {
            //     File.Delete(accountCookieFileName);
            // }

            // using (var streamWriter = new StreamWriter(accountCookieFileName))
            // {
            //     streamWriter.Write(account.CookieStr);
            //     streamWriter.Flush();
            // }
        }

        private static void TryLoadAccountCookie(AccountInfo account)
        {
            // var accountCookieFileName = Path.Combine(CookieFolder, account.UserName);
            // if (!File.Exists(accountCookieFileName))
            // {
            //     return;
            // }

            // using (var streamReader = new StreamReader(accountCookieFileName))
            // {
            //     account.CookieStr = streamReader.ReadToEnd();
            // }
        }
    }
}