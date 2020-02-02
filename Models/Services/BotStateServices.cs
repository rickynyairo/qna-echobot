using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using MyEchoBot.Models;

namespace MyEchoBot.Services
{
    public class BotStateService
    {
        #region Variables
        // State variables

        public ConversationState ConversationState { get; }
        public UserState UserState { get; }
        public DialogState DialogState { get; }

        // ID
        public static string UserProfileId { get; } = $"{nameof(BotStateService)}.UserProfile";
        public static string ConversationDataId { get; } = $"{nameof(BotStateService)}.ConversationData";
        public static string DialogStateId { get; } = $"{nameof(BotStateService)}.DialogState";
        // Accessors
        public IStatePropertyAccessor<UserProfile> UserProfileAccessor { get; set; }
        public IStatePropertyAccessor<ConversationData> ConversationDataAccessor { get; set; }
        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

        #endregion
        public BotStateService(ConversationState conversationState, UserState userState)
        {
            UserState = userState ?? throw new ArgumentNullException(nameof(UserState));
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(ConversationState));
            InitializeAccessors();
        }
        public void InitializeAccessors()
        {
            // init conversation state
            ConversationDataAccessor = ConversationState.CreateProperty<ConversationData>(ConversationDataId);
            // init dialog state accessor
            DialogStateAccessor = ConversationState.CreateProperty<DialogState>(DialogStateId);
            // Initialize user state
            UserProfileAccessor = UserState.CreateProperty<UserProfile>(UserProfileId);
        }
    }
}