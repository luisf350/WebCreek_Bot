using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebCreekBot.Dialogs;

namespace WebCreekBot.Bots
{
    public class RichCardsBot : DialogBot<MainDialog>
    {
        public RichCardsBot(ConversationState conversationState, UserState userState, MainDialog dialog)
            : base(conversationState, userState, dialog)
        {
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var reply = MessageFactory.Text($"Hola.{Environment.NewLine}Bienvenido al Bot de RichCards.");

                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }
        }
    }
}