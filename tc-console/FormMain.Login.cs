using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace TC
{
    partial class FormMain
    {
        const string cookieFolder = @".\Cookie";

        private void LoginAccount(string account)
        {
            loginLock.WaitOne();
            {
                activeAccount = account;
                AccountInfo account_info = this.accountTable[activeAccount];

                if (!string.IsNullOrEmpty(account_info.CookieStr))
                {
                    if (QueryRemoteSysTime(account) != DateTime.MinValue)
                    {
                        account_info.LoginStatus = "on-line";
                        OnLoginCompleted(account_info);
                        loginLock.Set();
                        return;
                    }
                }

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
                        return true;
                    }
                }
            }

            return false;
        }

        private void webBrowserMain_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var account = this.accountTable[activeAccount];

            if (!this.multiLoginConf.ContainsKey(account.AccountType))
            {
                return;
            }

            var loginpara = this.multiLoginConf[account.AccountType];
            if (account.LoginStatus == "in-login")
            {
                if (!webBrowserMain.Document.Title.Contains(loginpara.LoginTitle))
                {
                    return;
                }

                if (SubmitLoginRequest(loginpara))
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
                if (webBrowserMain.Document.Title.Contains(loginpara.HomeTitle))
                {
                    SetAccountCookie(activeAccount, webBrowserMain.Document.Cookie);
                    account.LoginStatus = "on-line";
                }
                else
                {
                    account.LoginStatus = "login-failed";
                }
            }

            if (account.LoginStatus == "on-line")
            {
                webBrowserMain.DocumentCompleted -=
                    new WebBrowserDocumentCompletedEventHandler(webBrowserMain_DocumentCompleted);

                loginLock.Set();
            }

            OnLoginCompleted(account);
        }

        private void OnLoginCompleted(AccountInfo account)
        {
            int handledAccountNumber = this.accountTable.Values.Sum(
                a => a.LoginStatus == "on-line" || a.LoginStatus == "login-failed" ? 1 : 0
                );

            if (handledAccountNumber >= this.accountTable.Keys.Count)
            {
                this.remoteTimeLastSync = DateTime.Now;
                this.RemoteTime = QueryRemoteSysTime(this.accountTable.Keys.First());
                // StartRemoteTimeSyncTimer();
                StartUITimeSyncTimer();
                StartTaskTimer();
                StartOnlineTaskCheckTimer();
            }

            this.Invoke(new DoSomething(() =>
            {
                foreach (ListViewItem lvItem in this.listViewAccounts.Items)
                {
                    var tagAccount = lvItem.Tag as AccountInfo;
                    if (tagAccount == account)
                    {
                        lvItem.SubItems[1].Text = ConvertStatusStr(account.LoginStatus);
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

        private Dictionary<string, string> ParseCookieStr(string cookiestr)
        {
            var output = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(cookiestr))
            {
                return output;
            }

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

        private void TrySaveAccountCookie(AccountInfo account)
        {
            if (!Directory.Exists(cookieFolder))
            {
                Directory.CreateDirectory(cookieFolder);
            }

            string accountCookieFileName = Path.Combine(cookieFolder, account.UserName);
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
            string accountCookieFileName = Path.Combine(cookieFolder, account.UserName);
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
