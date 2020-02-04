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

        #endregion
        public MainDialog(BotStateService botStateService) : base(nameof(MainDialog))
        {
            _botStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
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
            if (Regex.Match(stepContext.Context.Activity.Text.ToLower(), "hi").Success)
            {
                return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.greeting", null, cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.bugReport", null, cancellationToken);
            }

        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}