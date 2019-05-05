using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GameOfTHR3011.Entities;
using GameOfTHR3011.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using static GameOfTHR3011.AreaOfEffect;
using static GameOfTHR3011.Entities.Character;
using static GameOfTHR3011.Entities.Encounter;

namespace GameOfTHR3011
{

    public static class DemoHarness
    {
        const string DRAGON_ENCOUNTER = "DragonEncounter";

        [FunctionName("Demo_CreateEncounter")]
        public static async Task<HttpResponseMessage> CreateEncounter(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
            [OrchestrationClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await starter.StartNewAsync("DemoSetup", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }


        [FunctionName("DemoSetup")]
        public static void DemoSetup([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            List<CharacterState> characters = new List<CharacterState>();
            // Players
            characters.Add(new CharacterState("The Paladin", 90));
            characters.Add(new CharacterState("The Monk", 60));
            characters.Add(new CharacterState("The Wizard", 54));
            characters.Add(new CharacterState("The Bard", 70));
            // Dragon
            characters.Add(new CharacterState("The dragon", 320));


            foreach (CharacterState character in characters)
            {
                context.SignalEntity(character.Id, "SetStatistics", character);
            }
            log.LogInformation("Characters created");


            var encounter = new EntityId("Encounter", DRAGON_ENCOUNTER);
            context.SignalEntity(encounter, "Reset");

            foreach (CharacterState character in characters)
            {
                context.SignalEntity(encounter, "AddCharacter", character.Name);
            }
            log.LogInformation("Characters added to encounter");
        }


        [FunctionName("Demo_GetStatus")]
        public static async Task<HttpResponseMessage> GetStatus(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
    [OrchestrationClient] IDurableOrchestrationClient starter,
    ILogger log)
        {
            var encounter = await starter.ReadEntityStateAsync<EncounterState>(Encounter.ByKey(DRAGON_ENCOUNTER));

            return req.CreateResponse<IList<string>>(encounter.EntityState.Log);
        }


        [FunctionName("Demo_DragonAttack")]
        public static async Task<HttpResponseMessage> DragonAttack(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
            [OrchestrationClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var attack = new AoeAttack()
            {
                Encounter = Encounter.ByKey(DRAGON_ENCOUNTER),
                Source = Character.ByKey("The dragon"),
                Description = "The dragon breathes a cone of fire!",
                AreaDamage = new Damage()
                {
                    Type = DamageType.Fire,
                    Magnitude = 54
                }
            };

            string instanceId = await starter.StartNewAsync("ResolveAreaOfEffectDamage", attack);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }


    }

}