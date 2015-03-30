namespace TC.TCPage.WorldWar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal class ShowMoveArmyQueue
    {
        private const string EventPattern = "id=\"event_(?<taskId>\\d+)\"";

        private const string EtaPattern = "预计于(?<eta>.+?)到达";

        public IEnumerable<MoveArmyQueueItem> Items;

        public ShowMoveArmyQueue(string page)
        {
            var eventMatches = Regex.Matches(page, EventPattern);
            var etaMatches = Regex.Matches(page, EtaPattern);

            var eventIdList = (from Match eventId in eventMatches select eventId.Groups["taskId"].Value).ToList();
            var etaList = (from Match eta in etaMatches select DateTime.Parse(eta.Groups["eta"].Value)).ToList();

            this.Items = eventIdList.Zip(
                etaList,
                (eventId, eta) => new MoveArmyQueueItem { TaskId = int.Parse(eventId), Eta = eta });
        }

        public static ShowMoveArmyQueue Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(TCMod.military, TCSubMod.world_war, TCOperation.Show, TCFunc.move_army_queue);
            var rawPage = agent.WebClient.OpenUrl(url);
            return new ShowMoveArmyQueue(rawPage);
        }

        public class MoveArmyQueueItem
        {
            public int TaskId { get; set; }

            public DateTime Eta { get; set; }
        }
    }
}