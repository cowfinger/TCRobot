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
        private const string cookieFolder = @".\Cookie";

        private void LoginAccount(string account)
        {
            this.loginLock.WaitOne();
            {
                this.activeAccount = account;
                var account_info = this.accountTable[this.activeAccount];

                if (!string.IsNullOrEmpty(account_info.CookieStr))
                {
                    if (this.QueryRemoteSysTime(account) != DateTime.MinValue)
                    {
                        account_info.LoginStatus = "on-line";
                        this.OnLoginCompleted(account_info);
                        this.loginLock.Set();
                        return;
                    }
                }

                account_info.LoginStatus = "in-login";
                var loginurl = this.multiLoginConf[account_info.AccountType].LoginURL;

                this.webBrowserMain.Navigate(loginurl);

                this.webBrowserMain.DocumentCompleted += this.webBrowserMain_DocumentCompleted;
            }
        }

        private bool SubmitLoginRequest(LoginParam loginconf)
        {
            if (this.webBrowserMain.Document.GetElementById(loginconf.UsernameElmID) != null)
            {
                this.webBrowserMain.Document.GetElementById(loginconf.UsernameElmID).InnerText = this.activeAccount;
                this.webBrowserMain.Document.GetElementById(loginconf.PasswordElmID).InnerText =
                    this.accountTable[this.activeAccount].Password;

                Thread.Sleep(1000);
                foreach (HtmlElement he in this.webBrowserMain.Document.GetElementsByTagName("input"))
                {
                    if (he.GetAttribute("type") == "submit")
                    {
                        he.InvokeMember("Click");
                        return true;
                    }
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
            if (account.LoginStatus == "in-login")
            {
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
            }
            else if (account.LoginStatus == "submitting")
            {
                if (this.webBrowserMain.Document.Title.Contains(loginpara.HomeTitle))
                {
                    this.SetAccountCookie(this.activeAccount, this.webBrowserMain.Document.Cookie);
                    account.LoginStatus = "on-line";
                }
                else
                {
                    account.LoginStatus = "login-failed";
                }
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

            string mainPage = this.RefreshHomePage(account.UserName);
            account.UnionId = this.ParseUnionIdFromMainPage(mainPage);

            Task.Run(
                () =>
                    {
                        var cityNameList = this.GetAccountInflunceCityNameListWithArmy(account.UserName);
                        account.CityNameList = cityNameList;
                        account.CityIDList = cityNameList.Select(cityName => this.cityList[cityName]);
                    });

            if (handledAccountNumber >= this.accountTable.Keys.Count)
            {
                this.remoteTimeLastSync = DateTime.Now;
                this.RemoteTime = this.QueryRemoteSysTime(this.accountTable.Keys.First());
                this.StartUITimeSyncTimer();
                this.StartTaskTimer();
                this.StartOnlineTaskCheckTimer();
                this.StartAuthTimer();
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
                                    lvItem.SubItems[1].Text = this.ConvertStatusStr(account.LoginStatus);
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
            AccountInfo accountInfo = null;
            lock (this.accountTable)
            {
                accountInfo = this.accountTable[account];
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
                    var setcookies = this.ParseCookieStr(val);
                    var oldcookies = this.ParseCookieStr(oldcookiestr);

                    foreach (var key in setcookies.Keys)
                    {
                        oldcookies[key] = setcookies[key];
                    }

                    accountInfo.CookieStr = this.ComposeCookieStr(oldcookies);
                }

                this.TrySaveAccountCookie(accountInfo);
            }
        }

        private string GetAccountCookie(string account)
        {
            lock (this.accountTable)
            {
                return this.accountTable[account].CookieStr;
            }
        }

        private Dictionary<string, string> ParseCookieStr(string cookiestr)
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

        private string ComposeCookieStr(Dictionary<string, string> input)
        {
            var output = "";
            foreach (var i in input.Keys)
            {
                if (i == "path")
                {
                    continue;
                }

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

        private void TrySaveAccountCookie(AccountInfo account)
        {
            if (!Directory.Exists(cookieFolder))
            {
                Directory.CreateDirectory(cookieFolder);
            }

            var accountCookieFileName = Path.Combine(cookieFolder, account.UserName);
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

        private void TryLoadAccountCookie(AccountInfo account)
        {
            var accountCookieFileName = Path.Combine(cookieFolder, account.UserName);
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