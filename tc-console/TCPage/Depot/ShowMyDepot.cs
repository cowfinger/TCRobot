namespace TC.TCPage.Depot
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal class ShowMyDepot : TCPage
    {
        public const string PageIndicatorPattern = @"<span class=""page_on"">(?<page>\d+)/(?<maxPage>\d+)</span>";

        public const string UsablePropPattern = @"prop\.use_prop\((\d+), (\d+), (\d+), (\d+)\)";

        protected ShowMyDepot(string page)
            : base(page)
        {
        }

        public int Page
        {
            get
            {
                var match = Regex.Match(this.RawPage, PageIndicatorPattern);
                return match.Success ? int.Parse(match.Groups["page"].Value) : 0;
            }
        }

        public int MaxPage
        {
            get
            {
                var match = Regex.Match(this.RawPage, PageIndicatorPattern);
                return match.Success ? int.Parse(match.Groups["maxPage"].Value) : 0;
            }
        }

        public IEnumerable<DepotItem> PropItems
        {
            get
            {
                var matches = Regex.Matches(this.RawPage, UsablePropPattern);
                return from Match match in matches
                       select
                           new DepotItem
                               {
                                   PropertyId = int.Parse(match.Groups[1].Value),
                                   UserPropertyId = int.Parse(match.Groups[3].Value),
                                   GoodsType = int.Parse(match.Groups[2].Value)
                               };
            }
        }

        public static ShowMyDepot Open(RequestAgent agent, int goodsType = 1, int page = 1)
        {
            var url = agent.BuildUrl(
                TCMod.depot,
                TCSubMod.depot,
                TCOperation.Show,
                TCFunc.my_depot,
                new TCRequestArgument(TCElement.goods_type, goodsType),
                new TCRequestArgument(TCElement.page, page));
            return new ShowMyDepot(agent.WebClient.OpenUrl(url));
        }

        public static IEnumerable<DepotItem> EnumDepotItems(RequestAgent agent, int goodsType = 1)
        {
            var pageNum = 1;
            var maxPage = 1;
            do
            {
                var page = Open(agent, goodsType, pageNum);
                foreach (var item in page.PropItems)
                {
                    yield return item;
                }

                maxPage = page.MaxPage;
                ++pageNum;
            }
            while (pageNum <= maxPage);
        }
    }
}