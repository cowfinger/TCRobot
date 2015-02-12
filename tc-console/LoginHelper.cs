using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TC
{
    class LoginHelper
    {
        private AutoResetEvent loginLock = new AutoResetEvent(true);
        private WebBrowser webBrowser = null;
        private Dictionary<string, LoginParam> multiLoginConf = new Dictionary<string, LoginParam>();

        public LoginHelper(WebBrowser webBrowser)
        {
            this.webBrowser = webBrowser;
            this.webBrowser.ScriptErrorsSuppressed = true;
        }

        public void LoadProfile(string fileName)
        {
            using (var sr = new StreamReader(fileName, Encoding.Default))
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

                        if (this.multiLoginConf.ContainsKey(conf.Name))
                        {
                            this.multiLoginConf[conf.Name] = conf;
                        }
                        else
                        {
                            this.multiLoginConf.Add(conf.Name, conf);
                        }
                    }

                    line = sr.ReadLine();
                }
            }
        }

        public void LoginAccount(TCAccount account)
        {
            loginLock.WaitOne();
            {
                var loginPara = this.multiLoginConf[account.AccountType];

                this.webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(
                    (obj, args) =>
                    {
                        switch (account.LoginStatus)
                        {
                            case TCAccount.AccountLoginStatus.Logining:
                                if (!this.webBrowser.Document.Title.Contains(loginPara.LoginTitle))
                                {
                                    return;
                                }

                                account.LoginStatus = SubmitLoginRequest(loginPara, account) ?
                                    TCAccount.AccountLoginStatus.Submitting :
                                    TCAccount.AccountLoginStatus.LoginFailed;
                                break;

                            case TCAccount.AccountLoginStatus.Submitting:
                                if (this.webBrowser.Document.Title.Contains(loginPara.HomeTitle))
                                {
                                    account.Cookie.CookieString = this.webBrowser.Document.Cookie;
                                    account.LoginStatus = TCAccount.AccountLoginStatus.Online;
                                }
                                else
                                {
                                    account.LoginStatus = TCAccount.AccountLoginStatus.LoginFailed;
                                }
                                break;

                            case TCAccount.AccountLoginStatus.Online:
                                loginLock.Set();
                                break;
                        }
                    });
                this.webBrowser.Navigate(loginPara.LoginURL);
            }
        }

        private bool SubmitLoginRequest(LoginParam loginConf, TCAccount account)
        {
            var userNameElement = this.webBrowser.Document.GetElementById(loginConf.UsernameElmID);
            var passNameElement = this.webBrowser.Document.GetElementById(loginConf.PasswordElmID);

            if (userNameElement != null && passNameElement != null)
            {
                userNameElement.InnerText = account.UserName;
                passNameElement.InnerText = account.Password;

                Thread.Sleep(1000);

                foreach (HtmlElement he in this.webBrowser.Document.GetElementsByTagName("input"))
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
    }
}
