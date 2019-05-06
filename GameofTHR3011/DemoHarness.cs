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
        const string DRAGON_NAME = "The dragon";
        const string BARD_NAME = "The bard";
        const string MONK_NAME = "The monk";
        const string PALADIN_NAME = "The paladin";
        const string WIZARD_NAME = "The wizard";

        [FunctionName("Demo_CreateEncounter")]
        public static async Task<HttpResponseMessage> CreateEncounter(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
           [OrchestrationClient] IDurableOrchestrationClient starter,
           ILogger log)
        {
            string instanceId = await starter.StartNewAsync(nameof(Orchestrations.DemoSetup), null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("Demo_GetStatus")]
        public static async Task<HttpResponseMessage> GetStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
            [OrchestrationClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var encounter = await starter.ReadEntityStateAsync<EncounterState>(new EntityId(nameof(Encounter), DRAGON_ENCOUNTER));

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
                Encounter = new EntityId(nameof(Encounter), DRAGON_ENCOUNTER),
                Source = new EntityId(nameof(Character), DRAGON_NAME),
                Description = "The dragon breathes a cone of fire!",
                AreaDamage = new Damage()
                {
                    Type = DamageType.Fire,
                    Magnitude = 54
                }
            };

            string instanceId = await starter.StartNewAsync(nameof(AreaOfEffect.Orchestrations.ResolveAreaOfEffectDamage), attack);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }


        public enum Orchestrations
        {
            DemoSetup
        }

           
        [FunctionName(nameof(Orchestrations.DemoSetup))]
        public static void DemoSetup([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            List<CharacterState> characters = new List<CharacterState>();
            characters.Add(new CharacterState(PALADIN_NAME, 90));
            characters.Add(new CharacterState(MONK_NAME, 60));
            characters.Add(new CharacterState(WIZARD_NAME, 54));
            characters.Add(new CharacterState(BARD_NAME, 70));
            characters.Add(new CharacterState(DRAGON_NAME, 320));


            foreach (CharacterState character in characters)
            {
                context.SignalEntity(character.Id, nameof(Character.Ops.SetStatistics), character);
            }
            log.LogInformation("Characters created");


            var encounter = new EntityId(nameof(Encounter), DRAGON_ENCOUNTER);
            context.SignalEntity(encounter, nameof(Encounter.Ops.Reset));

            foreach (CharacterState character in characters)
            {
                context.SignalEntity(encounter, nameof(Encounter.Ops.AddCharacter), character.Name);
            }
            log.LogInformation("Characters added to encounter");
        }




    }

}