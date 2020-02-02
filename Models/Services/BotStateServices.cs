using System;
using Microsoft.Bot.Builder;
using MyEchoBot.Models;

namespace MyEchoBot.Services
{
    public class BotStateService
    {
        #region Variables
        // State variables

        public ConversationState ConversationState { get; }
        public UserState UserState { get; }

        // ID
        public static string UserProfileId { get; } = $"{nameof(BotStateService)}.UserProfile";
        public static string ConversationDataId { get; } = $"{nameof(BotStateService)}.ConversationData";

        // Accessors
        public IStatePropertyAccessor<UserProfile> UserProfileAccessor { get; set; }
        public IStatePropertyAccessor<ConversationData> ConversationDataAccessor { get; set; }

        #endregion
        public BotStateService(ConversationState conversationState, UserState userState)
        {
            UserState = userState ?? throw new ArgumentNullException(nameof(UserState));
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(ConversationState));
            InitializeAccessors();
        }
        public void InitializeAccessors()
        {
            // Initialize user state
            UserProfileAccessor = UserState.CreateProperty<UserProfile>(UserProfileId);
            // init conversation state
            ConversationDataAccessor = ConversationState.CreateProperty<ConversationData>(ConversationDataId);
        }
    }
}