using System.IO;
using System.Net;

namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class RequestAgent
    {
        private static readonly Random RandGen = new Random();

        public RequestAgent(AccountInfo account)
        {
            if (account == null)
            {
                throw new ArgumentNullException("Account cannot be null.");
            }

            this.Account = account;
            if (account.WebAgent != null && account.WebAgent.WebClient != null)
            {
                this.WebClient = new HttpClient(account.WebAgent.WebClient.Cookies);
            }
            else
            {
                this.WebClient = new HttpClient();
            }
        }

        public AccountInfo Account { get; private set; }

        public HttpClient WebClient { get; private set; }

        public static string GetTimeUrl(string hostName)
        {
            return string.Format("http://{0}/get_time.php?&r={1}", hostName, RandGen.NextDouble());
        }

        public static string BuildUrl(string hostName, string urlPathFormat, params object[] args)
        {
            var urlPath = string.Format(urlPathFormat, args);
            return string.Format("http://{0}/index.php?{1}&r={2}", hostName, urlPath, RandGen.NextDouble());
        }

        public static string BuildUrl(
            string hostName,
            TCMod mod,
            TCSubMod subMode,
            TCOperation operatoin,
            TCFunc func,
            List<TCRequestArgument> args = null)
        {
            var url = string.Format("mod={0}/{1}&op={2}&func={3}", mod, subMode, operatoin.ToString().ToLower(), func);

            if (args != null)
            {
                var argPairs = args.Select(arg => string.Format("{0}={1}", arg.Name, arg.Value)).ToArray();
                if (argPairs.Any())
                {
                    url += "&" + string.Join("&", argPairs);
                }
            }

            return BuildUrl(hostName, url);
        }

        public static string BuildUrl(
            string hostName,
            TCMod mod,
            TCSubMod subMode,
            TCOperation operatoin,
            TCFunc func,
            params TCRequestArgument[] args)
        {
            return BuildUrl(hostName, mod, subMode, operatoin, func, args.ToList());
        }

        public string BuildUrl(string urlPathFormat, params object[] args)
        {
            return BuildUrl(this.Account.AccountType, urlPathFormat, args);
        }

        public string BuildUrl(
            TCMod mod,
            TCSubMod subMode,
            TCOperation operatoin,
            TCFunc func,
            params TCRequestArgument[] args)
        {
            var url = string.Format("mod={0}/{1}&op={2}&func={3}", mod, subMode, operatoin.ToString().ToLower(), func);

            var argPairs = args.Select(arg => string.Format("{0}={1}", arg.Name, arg.Value)).ToArray();
            if (argPairs.Any())
            {
                url += "&" + string.Join("&", argPairs);
            }
            return this.BuildUrl(url);
        }

        public bool Login()
        {
            const string redirectUrlPattern = "window.location = '(.*?)'";
            const string LoginUrl = "https://passport.9wee.com/login";
            const string MegeLoginUrl = "http://yw1.tc.9wee.com/index.php?mod=player/player&op=show&func=merge_login";

            var userPassPair = string.Format("username={0}&password={1}", this.Account.UserName, this.Account.Password);
            var userPassData = Uri.EscapeUriString(userPassPair);
            var loginBody = string.Format(
                "_REFERER=http%3A%2F%2Fyw1.tc.9wee.com%2Findex.php%3Fmod%3Dlogin%26refresh&{0}", userPassData);

            this.WebClient.Referer = "http://yw1.tc.9wee.com/index.php?mod=login";
            var loginResp = this.WebClient.OpenUrl(LoginUrl, loginBody);
            var redirectUrlMatch = Regex.Match(loginResp, redirectUrlPattern);
            if (!redirectUrlMatch.Success)
            {
                return false;
            }

            var url = redirectUrlMatch.Groups[1].Value;
            var mainPage = this.WebClient.OpenUrl(url);

            const string MergeLoginPattern = @"onclick=""set_id\((\d+)\)"">(.*?)</a></li>";
            var matches = Regex.Matches(mainPage, MergeLoginPattern);
            var subAccounts = (from Match subAccountMatch in matches
                              let subId = int.Parse(subAccountMatch.Groups[1].Value)
                              let name = subAccountMatch.Groups[2].Value
                              select new {subId, name}).ToList();
            if (subAccounts.Any())
            {
                var dlg = new FormChooseAccount();
                foreach (var item in subAccounts)
                {
                    dlg.AddAccount(item.subId, item.name);
                }
                dlg.ShowDialog();

                mainPage = this.WebClient.OpenUrl(
                    MegeLoginUrl, string.Format("g_now_id={0}", dlg.TargetAccountId));
            }

            const string MainCityPattern = @"game\.city_id = (\d+);";
            const string NickNamePattern = @"game_im\.nick_name = '(.*?)';";
            const string CityLevelPattern = @"game\.city_level = (\d+);";
            const string UnionIdPattern = @"game_im\.join_channel\( 'U', (\d+) \);";
            const string CountryIdPattern = @"game\.country = (\d);";
            var match = Regex.Match(mainPage, MainCityPattern);
            if (match.Success)
            {
                this.Account.Tid = int.Parse(match.Groups[1].Value);

                var nickNameMatch = Regex.Match(mainPage, NickNamePattern);
                this.Account.NickName = nickNameMatch.Success ? nickNameMatch.Groups[1].Value : "N/A";

                var cityLevelMatch = Regex.Match(mainPage, CityLevelPattern);
                this.Account.Level = cityLevelMatch.Success ? int.Parse(cityLevelMatch.Groups[1].Value) : -1;

                var unionIdMatch  = Regex.Match(mainPage, UnionIdPattern);
                this.Account.UnionId = unionIdMatch.Success ? int.Parse(unionIdMatch.Groups[1].Value) : -1;

                var countryIdMatch  = Regex.Match(mainPage, CountryIdPattern);
                this.Account.CountryId = countryIdMatch.Success ? int.Parse(countryIdMatch.Groups[1].Value) : -1;

                this.WebClient.Cookies.Add(
                    new Cookie("tmp_mid", this.Account.Tid.ToString())
                        {
                            Domain = this.Account.AccountType
                        });
            }

            return match.Success;
        }
    }
}