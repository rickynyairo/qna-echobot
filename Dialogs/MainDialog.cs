// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using MyEchoBot.Services;
using MyEchoBot.Models;

namespace MyEchoBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        #region Variables
        private readonly BotStateService _botStateService;
        private readonly BotServices _botServices;

        #endregion
        public MainDialog(BotStateService botStateService, BotServices botServices) : base(nameof(MainDialog))
        {
            _botStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
            _botServices = botServices ?? throw new ArgumentNullException(nameof(botServices));
            InitializeWaterfallDialog();
        }
        private void InitializeWaterfallDialog()
        {
            // Create waterfall steps
            var waterfallSteps = new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync
            };

            // Add named dialogs
            AddDialog(new GreetingDialog($"{nameof(MainDialog)}.greeting", _botStateService));
            AddDialog(new BugReportDialog($"{nameof(MainDialog)}.bugReport", _botStateService));
            AddDialog(new WaterfallDialog($"{nameof(MainDialog)}.mainFlow", waterfallSteps));
            // Set the starting dialog
            InitialDialogId = $"{nameof(MainDialog)}.mainFlow";
        }
        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // use the dispatch model to determine which cognitive service to use
            var recogniserResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);

            // top intent tells us which cognitive service to use
            var topIntent = recogniserResult.GetTopScoringIntent();
            switch(topIntent.intent)
            {
                case "GreetingIntent":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.greeting", null, cancellationToken);
                case "NewBugReportIntent":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.bugReport", null, cancellationToken);
                case "QueryBugTypeIntent":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.bugType", null, cancellationToken);
                default:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Sorry, I dont understand what you mean"));
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);

        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}