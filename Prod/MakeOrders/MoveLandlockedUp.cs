﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Prod.MakeOrders
{
    static class MoveLandlockedUp
    {

        /// <summary>
        /// Any armies that are surrounded by our own territories (or our teammates) should move towards the nearest enemy.
        /// </summary>
        public static void Go(BotMain bot)
        {
            var start = Environment.TickCount;

            foreach (var landlocked in bot.Standing.Territories.Values.Where(o => o.OwnerPlayerID == bot.PlayerID
                && bot.Map.Territories[o.ID].ConnectedTo.Keys.All(c => bot.IsTeammateOrUs(bot.Standing.Territories[c].OwnerPlayerID))
                && bot.MakeOrders.GetArmiesAvailable(o.ID) > 0))
            {
                if (Environment.TickCount - start > 1000)
                {
                    //Extreme cases (i.e. where one player controls all of a big map), this algorithm can take forever.  We don't care about these extreme cases since they've already won.  Stop processing after too long (max 1 second per AI)
                    AILog.Log("MoveLandlockedUp", "Giving up due to time");
                    break; 
                }

                var moveTowards = bot.MoveTowardsNearestBorder(landlocked.ID);

                if (moveTowards.HasValue)
                {
                    var armies = bot.MakeOrders.GetArmiesAvailable(landlocked.ID);
                    AILog.Log("MoveLandlockedUp", "Ordering " + armies + " armies from " + bot.TerrString(landlocked.ID) + " to " + bot.TerrString(moveTowards.Value));
                    bot.Orders.AddAttack(landlocked.ID, moveTowards.Value, bot.Settings.AllowTransferOnly ? AttackTransferEnum.Transfer : AttackTransferEnum.AttackTransfer, armies, false);
                }
            }
        }
    }
}
