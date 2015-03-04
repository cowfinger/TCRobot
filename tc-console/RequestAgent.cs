using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TC
{
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
        user_prop_id,
    }

    class RequestAgent
    {
        public enum TCMod
        {
            depot,
            hero,
            influence,
            military,
            prop,
            world,
        }

        public enum TCSubMod
        {
            attack,
            depot,
            hero,
            influence,
            prop,
            world,
            world_war,
        }

        public enum TCOperation
        {
            Show,
            Do,
        }

        public enum TCFunc
        {
            allow_prop,
            attack,
            attack_confirm,
            city_build,
            create_group,
            create_team,
            disband_team,
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
            use_prop,
        }

        public class TCRequestArgument
        {
            public TCElement Name
            {
                get;
                set;
            }

            public int Value
            {
                get;
                set;
            }
        };

        private Random randGen = null;

        public TCAccount Account
        {
            get;
            private set;
        }

        public HttpClient WebClient
        {
            get;
            private set;
        }

        public RequestAgent(TCAccount account)
        {
            if (account == null)
            {
                throw new ArgumentNullException("Account cannot be null.");
            }

            this.Account = account;
            this.WebClient = new HttpClient(account.Cookie.CookieString);
            this.randGen = new Random(account.GetHashCode());
        }

        public string BuildUrl(string urlPathFormat, params object[] args)
        {
            string urlPath = string.Format(urlPathFormat, args);
            return string.Format("http://{0}/index.php?{1}&r={2}", this.Account.AccountType, urlPath, this.randGen.NextDouble());
        }

        public string BuildUrl(TCMod mod, TCSubMod subMode, TCOperation operatoin, TCFunc func, params TCRequestArgument[] args)
        {
            var url = string.Format(
                "mod={0}/{1}&op={2}&func={3}",
                mod.ToString(),
                subMode.ToString(),
                operatoin.ToString(),
                func.ToString());

            var argPairs = args.Select(arg => string.Format("{0}={1}", arg.Name.ToString(), arg.Value)).ToArray();
            if (argPairs.Any())
            {
                url += "&" + string.Join("&", argPairs);
            }
            return BuildUrl(url);
        }
    }
}
