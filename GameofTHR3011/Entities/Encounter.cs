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

        public static EntityId ByKey(string key)
        {
            return new EntityId("Encounter", key);
        }

        public class EncounterState
        {
            public IList<string> Participants { get; set; }
            public IList<string> Log { get; set; }

            public IList<EntityId> getParticipantEntities()
            {
                var entities = new List<EntityId>();
                foreach (string name in Participants)
                {
                    entities.Add(new EntityId("Character", name));
                }
                return entities;
            }

            public EncounterState()
            {
                Participants = new List<string>();
                Log = new List<string>();
            }
        }


        [FunctionName("Encounter")]
        public static async Task Run([EntityTrigger(EntityName = "Encounter")] IDurableEntityContext context)
        {
            var currentValue = context.GetState<EncounterState>();
            if (context.IsNewlyConstructed)
            {
                currentValue = new EncounterState();
                currentValue.Log.Add("--- Encounter! ---");
            }

            switch (context.OperationName)
            {
                case "AddCharacter":
                    string character = context.GetInput<string>();
                    currentValue.Participants.Add(character);
                    currentValue.Log.Add($"{character} joined the fray!");
                    break;
                case "GetParticipantEntities":
                    context.Return(currentValue.getParticipantEntities());
                    break;
                case "AppendStatementToLog":
                    string statement = context.GetInput<string>();
                    currentValue.Log.Add(statement);
                    break;
                case "GetLog":
                    context.Return(currentValue.Log);
                    break;
                case "Reset":
                    currentValue = new EncounterState();
                    break;
            }

            context.SetState(currentValue);
        }
    }

    
}
