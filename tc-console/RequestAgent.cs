namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    public enum TCElement
    {
        call_back,

        from,

        from_address,

        from_city_id,

        goods_type,

        group_id,

        hero_id,

        join_attack_type,

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

        user_nickname,

        user_prop_id
    }

    internal enum TCMod
    {
        depot,

        hero,

        influence,

        military,

        prop,

        world
    }

    internal enum TCSubMod
    {
        attack,

        depot,

        hero,

        influence,

        prop,

        science,

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

        move_army,

        military_event_list,

        my_depot,

        my_heros,

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

    internal class RequestAgent
    {
        private static readonly Random RandGen = new Random();

        public RequestAgent(TCAccount account)
        {
            if (account == null)
            {
                throw new ArgumentNullException("Account cannot be null.");
            }

            this.Account = account;
            this.WebClient = new HttpClient(account.Cookie.CookieString);
        }

        public TCAccount Account { get; private set; }

        public HttpClient WebClient { get; private set; }

        public static string BuildUrl(string hostName, string urlPathFormat, params object[] args)
        {
            var urlPath = string.Format(urlPathFormat, args);
            return string.Format(
                "http://{0}/index.php?{1}&r={2}",
                hostName,
                urlPath,
                RandGen.NextDouble());
        }

        public string BuildUrl(string urlPathFormat, params object[] args)
        {
            return BuildUrl(this.Account.UserName, urlPathFormat, args);
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

    }
}