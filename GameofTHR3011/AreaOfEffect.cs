using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GameOfTHR3011.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using static GameOfTHR3011.Entities.Character;
using static GameOfTHR3011.Models.Geometry;

namespace GameOfTHR3011
{
    public static class AreaOfEffect
    {

        public interface IAreaOfEffect
        {
            bool containsPoint(Point point);
        }


        public class AoeAttack : IAreaOfEffect
        {
            public EntityId Encounter { get; set; }
            public EntityId Source { get; set; }
            public string Description { get; set; }
            public Damage AreaDamage { get; set; }

            public bool containsPoint(Point point)
            {
                // TODO actual implementation
                return true;
            }
        }


        [FunctionName("ResolveAreaOfEffectDamage")]
        public static async Task ResolveAreaOfEffectDamage([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var attack = context.GetInput<AoeAttack>();

            var targets = await context.CallEntityAsync<List<EntityId>>(attack.Encounter, "GetParticipantEntities");



            using (await context.LockAsync(targets.ToArray()))
            {


                context.SignalEntity(attack.Encounter, "AppendStatementToLog", attack.Description);


                foreach (EntityId character in targets)
                {
                    if (character.Equals(attack.Source) || !attack.containsPoint(await context.CallEntityAsync<Point>(character, "GetLocation")))
                    {
                        continue;
                    }
                    else
                    {
                        CharacterEvent damageEvent = await context.CallEntityAsync<CharacterEvent>(character, "ApplyDamage", attack.AreaDamage);
                        context.SignalEntity(attack.Encounter, "AppendStatementToLog", damageEvent.EventMessage);
                    }
                }
            }
        }
    }
}
