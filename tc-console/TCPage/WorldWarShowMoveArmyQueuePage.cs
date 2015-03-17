using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TC.TCPage
{
    class WorldWarShowMoveArmyQueuePage
    {
        const string EventPattern = "id=\"event_(?<taskId>\\d+)\"";
        const string EtaPattern = "预计于(?<eta>.+?)到达";

        public class MoveArmyQueueItem
        {
            public int TaskId { get; set; }

            public DateTime Eta { get; set; }
        }

        public IEnumerable<MoveArmyQueueItem> Items;

        public static WorldWarShowMoveArmyQueuePage Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Show,
                TCFunc.move_army_queue);
            var rawPage = agent.WebClient.OpenUrl(url);
            return new WorldWarShowMoveArmyQueuePage(rawPage);
        }

        public WorldWarShowMoveArmyQueuePage(string page)
        {
            var eventMatches = Regex.Matches(page, EventPattern);
            var etaMatches = Regex.Matches(page, EtaPattern);

            var eventIdList = (from Match eventId in eventMatches
                               select eventId.Groups["taskId"].Value).ToList();
            var etaList = (from Match eta in etaMatches
                           select DateTime.Parse(eta.Groups["eta"].Value)).ToList();

            this.Items = eventIdList.Zip(
                etaList,
                (eventId, eta) => new MoveArmyQueueItem()
                {
                    TaskId = int.Parse(eventId),
                    Eta = eta
                });
        }
    }
}
