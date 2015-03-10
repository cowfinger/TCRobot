namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public enum TCElement
    {
        call_back,

        cid,

        from,

        from_address,

        from_city_id,

        goods_type,

        group_id,

        hero_id,

        join_attack_type,

        mt,

        node_id,

        page,

        pet_id,

        prop_id,

        prop_num,

        p_type,

        queue_id,

        res,

        tab_id,

        team_id,

        team_type,

        type,

        to_city_id,

        union_id,

        user_nickname,

        user_prop_id
    }

    internal enum TCMod
    {
        city,

        depot,

        hero,

        influence,

        military,

        prop,

        union,

        world
    }

    internal enum TCSubMod
    {
        attack,

        city,

        depot,

        hero,

        influence,

        prop,

        science,

        union,

        world,

        world_war
    }

    internal enum TCOperation
    {
        Show,

        Do
    }

    internal enum TCFunc
    {
        allow_prop,

        apply_union,

        attack,

        attack_confirm,

        city_build,

        create_group,

        create_team,

        disband_team,

        disband_group,

        get_node,

        group_detail,

        influence_city,

        influence_city_army,

        influence_city_detail,

        influence_donate,

        join_attack,

        join_attack_confirm,

        join_group,

        load_city,

        move_army,

        move_army_queue,

        military_event_list,

        my_depot,

        my_heros,

        out_union,

        relive_hero,

        science,

        team,

        team_detail,

        use_prop
    }

    public class TCRequestArgument
    {
        public TCElement Name { get; set; }

        public string Value { get; set; }

        public TCRequestArgument(TCElement name)
        {
            this.Name = name;
            this.Value = "";
        }

        public TCRequestArgument(TCElement name, int value)
        {
            this.Name = name;
            this.Value = value.ToString();
        }

        public TCRequestArgument(TCElement name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public TCRequestArgument(TCElement name, object value)
        {
            this.Name = name;
            this.Value = value.ToString();
        }
    };

    partial class RequestAgent
    {
        private static readonly Random RandGen = new Random();

        public RequestAgent(AccountInfo account)
        {
            if (account == null)
            {
                throw new ArgumentNullException("Account cannot be null.");
            }

            this.Account = account;
            this.WebClient = new HttpClient(account.CookieStr);
        }

        public AccountInfo Account { get; private set; }

        public HttpClient WebClient { get; private set; }

        public static string GetTimeUrl(string hostName)
        {
            return string.Format(
                "http://{0}/get_time.php?&r={1}",
                hostName,
                RandGen.NextDouble());
        }

        public static string BuildUrl(string hostName, string urlPathFormat, params object[] args)
        {
            var urlPath = string.Format(urlPathFormat, args);
            return string.Format(
                "http://{0}/index.php?{1}&r={2}",
                hostName,
                urlPath,
                RandGen.NextDouble());
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

        private IEnumerable<CityInfo> QueryInfluenceCityList()
        {
            this.OpenAccountFirstCity();
            var content = this.OpenMoveTroopPage();

            return this.ParseCityListFromMoveTroopPage(content);
        }
    }
}