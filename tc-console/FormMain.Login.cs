namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;

    partial class FormMain
    {
        private const string CookieFolder = @".\Cookie";

        private void LoginAccount(string account)
        {
            this.loginLock.WaitOne();
            {
                this.activeAccount = account;
                var accountInfo = this.accountTable[this.activeAccount];

                if (!string.IsNullOrEmpty(accountInfo.CookieStr))
                {
                    if (!this.RefreshHomePage(account).Contains("登录超时"))
                    {
                        accountInfo.LoginStatus = "on-line";
                        this.OnLoginCompleted(accountInfo);
                        this.loginLock.Set();
                        return;
                    }
                }

                accountInfo.LoginStatus = "in-login";
                var loginurl = this.multiLoginConf[accountInfo.AccountType].LoginURL;

                this.webBrowserMain.Navigate(loginurl);

                this.webBrowserMain.DocumentCompleted += this.webBrowserMain_DocumentCompleted;
            }
        }

        private bool SubmitLoginRequest(LoginParam loginconf)
        {
            var webDoc = this.webBrowserMain.Document;
            if (webDoc.GetElementById(loginconf.UsernameElmID) == null)
            {
                return false;
            }

            webDoc.GetElementById(loginconf.UsernameElmID).InnerText = this.activeAccount;
            webDoc.GetElementById(loginconf.PasswordElmID).InnerText = this.accountTable[this.activeAccount].Password;

            Thread.Sleep(1000);
            foreach (HtmlElement he in webDoc.GetElementsByTagName("input"))
            {
                if (he.GetAttribute("type") == "submit")
                {
                    he.InvokeMember("Click");
                    return true;
                }
            }

            return false;
        }

        private void webBrowserMain_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var account = this.accountTable[this.activeAccount];

            if (!this.multiLoginConf.ContainsKey(account.AccountType))
            {
                return;
            }

            var loginpara = this.multiLoginConf[account.AccountType];
            switch (account.LoginStatus)
            {
                case "in-login":
                    if (!this.webBrowserMain.Document.Title.Contains(loginpara.LoginTitle))
                    {
                        return;
                    }

                    if (this.SubmitLoginRequest(loginpara))
                    {
                        account.LoginStatus = "submitting";
                    }
                    else
                    {
                        account.LoginStatus = "login-failed";
                    }
                    break;
                case "submitting":
                    if (this.webBrowserMain.Document.Title.Contains(loginpara.HomeTitle))
                    {
                        this.SetAccountCookie(this.activeAccount, this.webBrowserMain.Document.Cookie);
                        account.LoginStatus = "on-line";
                    }
                    else
                    {
                        account.LoginStatus = "login-failed";
                    }
                    break;
            }

            if (account.LoginStatus == "on-line")
            {
                this.webBrowserMain.DocumentCompleted -= this.webBrowserMain_DocumentCompleted;

                this.loginLock.Set();
            }

            this.OnLoginCompleted(account);
        }

        private void OnLoginCompleted(AccountInfo account)
        {
            var handledAccountNumber =
                this.accountTable.Values.Sum(a => a.LoginStatus == "on-line" || a.LoginStatus == "login-failed" ? 1 : 0);

            var mainPage = this.RefreshHomePage(account.UserName);
            account.UnionId = this.ParseUnionIdFromMainPage(mainPage);
            account.WebAgent = new RequestAgent(account);

            Task.Run(
                () =>
                    {
                        var cityNameList = this.GetAccountInflunceCityNameListWithArmy(account.UserName).ToList();
                        account.CityNameList = cityNameList;
                        account.CityIDList = cityNameList.Select(cityName => this.cityList[cityName]).ToList();

                        var accountCityList = this.QueryInfluenceCityList(account.UserName).ToList();
                        account.InfluenceCityList = accountCityList.ToDictionary(city => city.Name);
                        account.InfluenceMap = this.BuildInfluenceCityMap(accountCityList, account.UserName);

                        if (accountCityList.Any())
                        {
                            account.MainCity = accountCityList.Single(cityInfo => cityInfo.CityId == 0);
                            account.Level = this.GetAccountLevel(account);
                        }
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
                this.StartOnlineTaskCheckTimer();
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

        private void SetAccountCookie(string account, string val)
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
                var oldcookiestr = accountInfo.CookieStr;
                if (oldcookiestr.Length == 0)
                {
                    accountInfo.CookieStr = val;
                }
                else
                {
                    var setcookies = ParseCookieStr(val);
                    var oldcookies = ParseCookieStr(oldcookiestr);

                    foreach (var key in setcookies.Keys)
                    {
                        oldcookies[key] = setcookies[key];
                    }

                    accountInfo.CookieStr = ComposeCookieStr(oldcookies);
                }

                TrySaveAccountCookie(accountInfo);
            }
        }

        private string GetAccountCookie(string account)
        {
            lock (this.accountTable)
            {
                return this.accountTable[account].CookieStr;
            }
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
            if (!Directory.Exists(CookieFolder))
            {
                Directory.CreateDirectory(CookieFolder);
            }

            var accountCookieFileName = Path.Combine(CookieFolder, account.UserName);
            if (File.Exists(accountCookieFileName))
            {
                File.Delete(accountCookieFileName);
            }

            using (var streamWriter = new StreamWriter(accountCookieFileName))
            {
                streamWriter.Write(account.CookieStr);
                streamWriter.Flush();
            }
        }

        private static void TryLoadAccountCookie(AccountInfo account)
        {
            var accountCookieFileName = Path.Combine(CookieFolder, account.UserName);
            if (!File.Exists(accountCookieFileName))
            {
                return;
            }

            using (var streamReader = new StreamReader(accountCookieFileName))
            {
                account.CookieStr = streamReader.ReadToEnd();
            }
        }
    }
}