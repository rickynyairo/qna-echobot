using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using MyEchoBot.Config;

namespace MyEchoBot.Services
{
    public class BotServices
    {
        public BotServices()
        {
            // Read the setting for cognitive services from appsettings.json
            LuisPredictionOptions luisPrediction = new LuisPredictionOptions()
            {
                IncludeAllIntents = true,
                IncludeInstanceData = true
            };
            LuisApplication luisApp = new LuisApplication(
                Envars.LuisAppId,
                Envars.LuisAPIKey,
                $"https://{Envars.LuisAPIHostName}.api.cognitive.microsoft.com"
            );
            Dispatch = new LuisRecognizer(luisApp, luisPrediction, true);
        }

        public LuisRecognizer Dispatch { get; private set; }
    }
}