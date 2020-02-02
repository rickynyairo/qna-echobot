using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using MyEchoBot.Services;
using MyEchoBot.Models;

namespace MyGreetingBot.Bots
{
    public class GreetingBot : ActivityHandler
    {
        #region Variables
            private readonly BotStateService _botStateService;
        #endregion

        public GreetingBot(BotStateService botStateService)
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await GetName(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await GetName(turnContext, cancellationToken);
                }
            }
        }
        private async Task GetName(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _botStateService.UserProfileAccessor.GetAsync(turnContext, () => new UserProfile());
            ConversationData conversationData = await _botStateService.ConversationDataAccessor.GetAsync(turnContext, () => new ConversationData());
            if (!string.IsNullOrEmpty(userProfile.Name))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Hi {userProfile.Name}. How can I help you today?"), cancellationToken);
            }
            else
            {
                if(conversationData.PromptedUserForName)
                {
                    // set the name to what the user provides
                    userProfile.Name = turnContext.Activity.Text?.Trim();

                    // acknowledge that we got their name
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Thanks {userProfile.Name}. How can I help you today?"), cancellationToken);

                    // Reset the flag to allow the bot to go through the cycle again
                    conversationData.PromptedUserForName = true;
                }
                else
                {
                    // prompt the user for their name
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Welcome, what is your name"), cancellationToken);
                    
                    // update the flag
                    conversationData.PromptedUserForName = true;
                }

                // save any state changes 
                await _botStateService.UserProfileAccessor.SetAsync(turnContext, userProfile);
                await _botStateService.ConversationDataAccessor.SetAsync(turnContext, conversationData);

                await _botStateService.UserState.SaveChangesAsync(turnContext);
                await _botStateService.ConversationState.SaveChangesAsync(turnContext);

            }
        }

    }
}
