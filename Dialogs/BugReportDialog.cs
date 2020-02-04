// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using MyEchoBot.Services;
using MyEchoBot.Models;

namespace MyEchoBot.Dialogs
{
    public class BugReportDialog : ComponentDialog
    {
        #region Variables
        private readonly BotStateService _botStateService;

        #endregion
        public BugReportDialog(string dialogId, BotStateService botStateService) : base(dialogId)
        {
            _botStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
            InitializeWaterfallDialog();
        }
        private void InitializeWaterfallDialog()
        {
            // Create waterfall steps
            var waterfallSteps = new WaterfallStep[]
            {
                DescriptionStepAsync,
                CallbackTimeStepAsync,
                PhoneNumberStepAsync,
                BugStepAsync,
                SummaryStepAsync
            };

            // Add named dialogs
            AddDialog(new WaterfallDialog($"{nameof(BugReportDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(BugReportDialog)}.description"));
            AddDialog(new DateTimePrompt($"{nameof(BugReportDialog)}.callbackTime", CallbackTimeValidatorAsync));
            AddDialog(new TextPrompt($"{nameof(BugReportDialog)}.phoneNumber", PhoneNumberValidatorAsync));
            AddDialog(new ChoicePrompt($"{nameof(BugReportDialog)}.bug"));

            // Set the starting dialog
            InitialDialogId = $"{nameof(BugReportDialog)}.mainFlow";
        }

        #region DialogSteps
        private async Task<DialogTurnResult> DescriptionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.description",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Enter a description for your report ")
                }, cancellationToken);

        }
        private async Task<DialogTurnResult> CallbackTimeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["description"] = (string)stepContext.Result;
            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.callbackTime",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter a time we can get back to you"),
                    RetryPrompt = MessageFactory.Text("The value must be between 9am and 5pm")
                }, cancellationToken);
        }
        private async Task<DialogTurnResult> PhoneNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["callbackTime"] = Convert.ToDateTime(((List<DateTimeResolution>)stepContext.Result).FirstOrDefault().Value);
            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.phoneNumber",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter a phone number that we can reach you on"),
                    RetryPrompt = MessageFactory.Text("The phone number entered is invalid, please check")
                }, cancellationToken);
        }
        private async Task<DialogTurnResult> BugStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["phoneNumber"] = (string)stepContext.Result;
            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.bug",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please select the type of bug"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Security", "Crash", "Power", "Performance", "Usability", "Other" })
                }, cancellationToken);
        }
        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["bug"] = ((FoundChoice)stepContext.Result).Value;

            // get the current profile object from the user state
            var userProfile = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            // save the data inside the user profile
            userProfile.Description = (string)stepContext.Values["description"];
            userProfile.CallbackTime = (DateTime)stepContext.Values["callbackTime"];
            userProfile.PhoneNumber = (string)stepContext.Values["phoneNumber"];
            userProfile.Bug = (string)stepContext.Values["bug"];

            // show the summary to the user
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Here is a summary of your bug report"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Description: {userProfile.Description}"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Callback Time: {userProfile.CallbackTime.ToString()}"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Phone Number: {userProfile.PhoneNumber}"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Bug: {userProfile.Bug}"), cancellationToken);

            // save data in user state
            await _botStateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
        #endregion

        #region Validators

        private Task<bool> CallbackTimeValidatorAsync(PromptValidatorContext<IList<DateTimeResolution>> promptValidatorContext, CancellationToken cancellationToken)
        {
            var valid = false;

            if (promptValidatorContext.Recognized.Succeeded)
            {
                var resolution = promptValidatorContext.Recognized.Value.First();
                DateTime selectedDate = Convert.ToDateTime(resolution.Value);
                TimeSpan start = new TimeSpan(9,0,0);
                TimeSpan end = new TimeSpan(17,0,0);
                if ((selectedDate.TimeOfDay >= start) && (selectedDate.TimeOfDay <= end)){
                    valid = true;
                }
            }
            return Task.FromResult(valid);

        }
        private Task<bool> PhoneNumberValidatorAsync(PromptValidatorContext<string> promptValidatorContext, CancellationToken cancellationToken)
        {
            var valid = false;

            if (promptValidatorContext.Recognized.Succeeded)
            {
                if (promptValidatorContext.Recognized.Value.Length > 5)
                {
                    valid = true;
                }
            }
            return Task.FromResult(valid);

        }
        #endregion
    }
}