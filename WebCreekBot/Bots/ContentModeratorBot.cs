using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebCreekBot.Dto;

namespace WebCreekBot.Bots
{
    public class ContentModeratorBot : ActivityHandler
    {
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await SendWelcomeMessageAsync(turnContext, cancellationToken);
        }

        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(
                        $"Bienvenido al Bot de Análisis de Contenido." +
                        Environment.NewLine +
                        $"Ingrese un texto para ser analizado...",
                        cancellationToken: cancellationToken);
                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var reply = ProcessInput(turnContext);
            // Respond to the user.
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        // Given the input from the message, create the response.
        private static IMessageActivity ProcessInput(ITurnContext turnContext)
        {
            var activity = turnContext.Activity;
            return HandleIncomingAttachment(activity); ;
        }

        private static IMessageActivity HandleIncomingAttachment(IMessageActivity activity)
        {
            string textToAnalize = activity.Text;
            if (!string.IsNullOrEmpty(textToAnalize))
            {
                var contentResponse = CallModerateApi(textToAnalize).Result;

                StringBuilder sb = new StringBuilder();

                sb.AppendLine($"El texto ingresado esta en {contentResponse.Language}");

                if (contentResponse.Terms != null && contentResponse.Terms.Any())
                {
                    foreach (var item in contentResponse.Terms)
                    {
                        sb.AppendLine($" - La palabra {item.Term} es una mala palabra.");
                    }
                    sb.AppendLine("Su comentario no puede ser almacenado.");
                }
                else
                {
                    sb.AppendLine("Se a registrado su comentario correctamente.");
                }

                return MessageFactory.Text(sb.ToString());
            }

            return MessageFactory.Text("Nada para procesar...");
        }

        private static async Task<ContentModeratorDto> CallModerateApi(string userInput)
        {
            string Uri = "https://webcreekcontentmoderator.cognitiveservices.azure.com/contentmoderator/moderate/v1.0/ProcessText/Screen";

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(Uri);

                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("text/plain"));

                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "bbf386cc70674166adfbc08ee4d3f085");

                var response = client.PostAsync(Uri, new StringContent(userInput, Encoding.UTF8, "text/plain")).Result;

                // Get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();

                // Display the JSON response.
                return JsonConvert.DeserializeObject<ContentModeratorDto>(contentString);
            }
        }
    }
}