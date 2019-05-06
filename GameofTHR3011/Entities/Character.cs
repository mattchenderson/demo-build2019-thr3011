using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GameOfTHR3011.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using static GameOfTHR3011.Models.Geometry;

namespace GameOfTHR3011.Entities
{
    public static class Character
    {
        public class CharacterState
        {
            public EntityId Id { get; }
            public string Name { get; }
            public int HitPoints { get; set; }
            public Point Location { get; }

            // TODO other statistics

            public CharacterState(string name, int hp)
            {
                Name = name;
                HitPoints = hp;
                Id = new EntityId(nameof(Character), name);
            }
        }

        public class CharacterEvent
        {
            public CharacterState NewState { get; set; }
            public string EventMessage { get; set;  }
        }

        public enum Ops
        {
            SetStatistics,
            GetLocation,
            ApplyDamage
        }


        [FunctionName(nameof(Character))]
        public static async Task HandleOperation([EntityTrigger(EntityName = nameof(Character))] IDurableEntityContext context)
        {
            CharacterState currentState = context.GetState<CharacterState>();

            switch (Enum.Parse<Ops>(context.OperationName))
            {
                case Ops.SetStatistics:
                    currentState = context.GetInput<CharacterState>();
                    break;
                case Ops.GetLocation:
                    context.Return(currentState.Location);
                    break;
                case Ops.ApplyDamage:
                    Damage damage = context.GetInput<Damage>();
                    // TODO check for type resistances/immunities/weaknesses
                    currentState.HitPoints -= damage.Magnitude;
                    context.Return(new CharacterEvent()
                    {
                        EventMessage = $"{currentState.Name} took {damage.Magnitude} points of {damage.Type} damage! {currentState.Name} has {currentState.HitPoints} HP!",
                        NewState = currentState
                    });
                    break;
            }

            context.SetState(currentState);
        }
    }
}
