using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace GameOfTHR3011.Entities
{
    public static class Encounter
    {

        public class EncounterState
        {
            public IList<string> Participants { get; set; }
            public IList<string> Log { get; set; }

            public IList<EntityId> getParticipantEntities()
            {
                var entities = new List<EntityId>();
                foreach (string characterKey in Participants)
                {
                    entities.Add(new EntityId(nameof(Character), characterKey));
                }
                return entities;
            }

            public EncounterState()
            {
                Participants = new List<string>();
                Log = new List<string>();
            }
        }

        public enum Ops
        {
            AddCharacter,
            GetParticipanEntities,
            AppendStatementToLog,
            GetLog,
            Reset
        }


        [FunctionName(nameof(Encounter))]
        public static async Task HandleOperation([EntityTrigger(EntityName = nameof(Encounter))] IDurableEntityContext context)
        {
            var currentValue = context.GetState<EncounterState>();
            if (context.IsNewlyConstructed)
            {
                currentValue = new EncounterState();
            }

            switch (Enum.Parse<Ops>(context.OperationName))
            {
                case Ops.AddCharacter:
                    string character = context.GetInput<string>();
                    currentValue.Participants.Add(character);
                    currentValue.Log.Add($"{character} joined the fray!");
                    break;
                case Ops.GetParticipanEntities:
                    context.Return(currentValue.getParticipantEntities());
                    break;
                case Ops.AppendStatementToLog:
                    string statement = context.GetInput<string>();
                    currentValue.Log.Add(statement);
                    break;
                case Ops.GetLog:
                    context.Return(currentValue.Log);
                    break;
                case Ops.Reset:
                    currentValue = new EncounterState();
                    break;
            }

            context.SetState(currentValue);
        }
    }

    
}
