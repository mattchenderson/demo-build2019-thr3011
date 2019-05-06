using System.Collections.Generic;
using System.Threading.Tasks;
using GameOfTHR3011.Entities;
using GameOfTHR3011.Models;
using Microsoft.Azure.WebJobs;
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
            public EntityId Encounter { get; set; } // Encounter Entity
            public EntityId Source { get; set; } // Character Entity
            public string Description { get; set; }
            public Damage AreaDamage { get; set; }

            public bool containsPoint(Point point)
            {
                // TODO actual implementation
                return true;
            }
        }

        public enum Orchestrations
        {
            ResolveAreaOfEffectDamage
        }

        [FunctionName(nameof(Orchestrations.ResolveAreaOfEffectDamage))]
        public static async Task ResolveAreaOfEffectDamage([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var attack = context.GetInput<AoeAttack>();

            var targets = await context.CallEntityAsync<List<EntityId>>(attack.Encounter, nameof(Encounter.Ops.GetParticipanEntities));



            using (await context.LockAsync(targets.ToArray()))
            {


                context.SignalEntity(attack.Encounter, nameof(Encounter.Ops.AppendStatementToLog), attack.Description);


                foreach (EntityId character in targets)
                {
                    if (character.Equals(attack.Source) || !attack.containsPoint(await context.CallEntityAsync<Point>(character, nameof(Character.Ops.GetLocation))))
                    {
                        continue;
                    }
                    else
                    {
                        CharacterEvent damageEvent = await context.CallEntityAsync<CharacterEvent>(character, nameof(Character.Ops.ApplyDamage), attack.AreaDamage);
                        context.SignalEntity(attack.Encounter, nameof(Encounter.Ops.AppendStatementToLog), damageEvent.EventMessage);
                    }
                }
            }
        }
    }
}
