using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WebCreekBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        public MainDialog() : base(nameof(MainDialog))
        {
            // Define the main dialog and its related components.
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ChoiceCardStepAsync,
                ShowCardStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        // 1. Prompts the user if the user is not in the middle of a dialog.
        // 2. Re-prompts the user when an invalid input is received.
        private async Task<DialogTurnResult> ChoiceCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Create the PromptOptions which contain the prompt and re-prompt messages.
            // PromptOptions also contains the list of choices available to the user.
            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Que tipo de tarjeta te gustaría ver?" + Environment.NewLine + "Selecciona o ingresa el nombre de la tarjeta."),
                RetryPrompt = MessageFactory.Text("Esa no es una opción valida, porfavor selecciona una tarjeta, ingresa su nombre o un número del 1 al 9."),
                Choices = GetChoices(),
            };

            // Prompt the user with the configured PromptOptions.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        // Send a Rich Card response to the user based on their choice.
        // This method is only called when a valid prompt response is parsed from the user's response to the ChoicePrompt.
        private async Task<DialogTurnResult> ShowCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Cards are sent as Attachments in the Bot Framework.
            // So we need to create a list of attachments for the reply activity.
            var attachments = new List<Attachment>();

            // Reply to the activity we received with an activity.
            var reply = MessageFactory.Attachment(attachments);

            // Decide which type of card(s) we are going to show the user
            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Adaptive Card":
                    // Display an Adaptive Card
                    reply.Attachments.Add(Cards.CreateAdaptiveCardAttachment());
                    break;

                case "Animation Card":
                    // Display an AnimationCard.
                    reply.Attachments.Add(Cards.GetAnimationCard().ToAttachment());
                    break;

                case "Audio Card":
                    // Display an AudioCard
                    reply.Attachments.Add(Cards.GetAudioCard().ToAttachment());
                    break;

                case "Hero Card":
                    // Display a HeroCard.
                    reply.Attachments.Add(Cards.GetHeroCard().ToAttachment());
                    break;

                case "Receipt Card":
                    // Display a ReceiptCard.
                    reply.Attachments.Add(Cards.GetReceiptCard().ToAttachment());
                    break;

                case "Signin Card":
                    // Display a SignInCard.
                    reply.Attachments.Add(Cards.GetSigninCard().ToAttachment());
                    break;

                case "Thumbnail Card":
                    // Display a ThumbnailCard.
                    reply.Attachments.Add(Cards.GetThumbnailCard().ToAttachment());
                    break;

                case "Video Card":
                    // Display a VideoCard
                    reply.Attachments.Add(Cards.GetVideoCard().ToAttachment());
                    break;

                default:
                    // Display a carousel of all the rich card types.
                    reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    reply.Attachments.Add(Cards.CreateAdaptiveCardAttachment());
                    reply.Attachments.Add(Cards.GetAnimationCard().ToAttachment());
                    reply.Attachments.Add(Cards.GetAudioCard().ToAttachment());
                    reply.Attachments.Add(Cards.GetHeroCard().ToAttachment());
                    reply.Attachments.Add(Cards.GetReceiptCard().ToAttachment());
                    reply.Attachments.Add(Cards.GetSigninCard().ToAttachment());
                    reply.Attachments.Add(Cards.GetThumbnailCard().ToAttachment());
                    reply.Attachments.Add(Cards.GetVideoCard().ToAttachment());
                    break;
            }

            // Send the card(s) to the user as an attachment to the activity
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            // Give the user instructions about what to do next
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Ingresa cualquier cosa para ver otra tarjeta."), cancellationToken);

            return await stepContext.EndDialogAsync();
        }

        private IList<Choice> GetChoices()
        {
            var cardOptions = new List<Choice>()
            {
                new Choice() { Value = "Adaptive Card", Synonyms = new List<string>() { "adaptive" } },
                new Choice() { Value = "Animation Card", Synonyms = new List<string>() { "animation" } },
                new Choice() { Value = "Audio Card", Synonyms = new List<string>() { "audio" } },
                new Choice() { Value = "Hero Card", Synonyms = new List<string>() { "hero" } },
                new Choice() { Value = "Receipt Card", Synonyms = new List<string>() { "receipt" } },
                new Choice() { Value = "Signin Card", Synonyms = new List<string>() { "signin" } },
                new Choice() { Value = "Thumbnail Card", Synonyms = new List<string>() { "thumbnail", "thumb" } },
                new Choice() { Value = "Video Card", Synonyms = new List<string>() { "video" } },
                new Choice() { Value = "All cards", Synonyms = new List<string>() { "all" } },
            };

            return cardOptions;
        }
    }

    public class Cards
    {
        public static Attachment CreateAdaptiveCardAttachment()
        {
            // combine path for cross platform support
            string[] paths = { ".", "Resources", "adaptiveCard.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

        public static HeroCard GetHeroCard()
        {
            var heroCard = new HeroCard
            {
                Title = "BotFramework Hero Card",
                Subtitle = "Webcreek Bot",
                Text = "Una tarjeta que normalmente contiene una sola imagen grande, uno o varios botones y texto. Se utiliza normalmente para resaltar visualmente una posible selección del usuario.",
                Images = new List<CardImage> { new CardImage("https://i.ebayimg.com/images/g/qnQAAOSw~Nta4we4/s-l1600.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Avengers", value: "https://www.marvel.com/movies/avengers-endgame") },
            };

            return heroCard;
        }

        public static ThumbnailCard GetThumbnailCard()
        {
            var heroCard = new ThumbnailCard
            {
                Title = "BotFramework Thumbnail Card",
                Subtitle = "Webcreek Bot",
                Text = "Una tarjeta que normalmente contiene una sola imagen de miniatura, uno o varios botones y texto. Se utiliza normalmente para resaltar visualmente los botones de una posible selección del usuario.",
                Images = new List<CardImage> { new CardImage("https://media.cylex.us.com/companies/2533/4392/logo/logo.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Get Started", value: "https://webcreek.com/es/") },
            };

            return heroCard;
        }

        public static ReceiptCard GetReceiptCard()
        {
            var receiptCard = new ReceiptCard
            {
                Title = "Webcreek",
                Facts = new List<Fact> { new Fact("Número Orden", "1234"), new Fact("Payment Method", "VISA 5555 - ****") },
                Items = new List<ReceiptItem>
                {
                    new ReceiptItem(
                        "Pizza",
                        price: "$ 15.00",
                        quantity: "2",
                        image: new CardImage(url: "https://www.lavanguardia.com/r/GODO/LV/p6/WebSite/2019/04/03/Recortada/img_rocarceller_20190204-120844_imagenes_lv_getty_istock-938742222-kuKF-U4614441024200t-992x558@LaVanguardia-Web.jpg")),
                    new ReceiptItem(
                        "Hot Wings x 6",
                        price: "$ 10.00",
                        quantity: "3",
                        image: new CardImage(url: "https://www.lakegenevacountrymeats.com/wp-content/uploads/ChickenWings.jpg")),
                },
                Tax = "$ 9.00",
                Total = "$ 69.00",
                Buttons = new List<CardAction>
                {
                    new CardAction(
                        ActionTypes.OpenUrl,
                        "Mas información",
                        "https://account.windowsazure.com/content/6.10.1.38-.8225.160809-1618/aux-pre/images/offer-icon-freetrial.png",
                        value: "https://azure.microsoft.com/en-us/pricing/"),
                },
            };

            return receiptCard;
        }

        public static SigninCard GetSigninCard()
        {
            var signinCard = new SigninCard
            {
                Text = "Una tarjeta que permite al bot solicitar el inicio de sesión del usuario. Normalmente contiene texto y uno o más botones en los cuales el usuario puede hacer clic para comenzar el proceso de inicio de sesión",
                Buttons = new List<CardAction> { new CardAction(ActionTypes.Signin, "Sign-in", value: "https://login.microsoftonline.com/") },
            };

            return signinCard;
        }

        public static AnimationCard GetAnimationCard()
        {
            var animationCard = new AnimationCard
            {
                Title = "Microsoft Bot Framework",
                Subtitle = "Animation Card",
                Image = new ThumbnailUrl
                {
                    Url = "https://docs.microsoft.com/en-us/bot-framework/media/how-it-works/architecture-resize.png",
                },
                Media = new List<MediaUrl>
                {
                    new MediaUrl()
                    {
                        Url = "http://i.giphy.com/Ki55RUbOV5njy.gif",
                    },
                },
            };

            return animationCard;
        }

        public static VideoCard GetVideoCard()
        {
            var videoCard = new VideoCard
            {
                Title = "Scarface",
                Subtitle = "Cara Cortada - El precio del poder",
                Text = "es una película estadounidense de 1983, dirigida por Brian De Palma.1​ Protagonizada por Al Pacino, Steven Bauer, Michelle Pfeiffer, Mary Elizabeth Mastrantonio, Robert Loggia, Harris Yulin, Paul Shenar y F. Murray Abraham. El guion fue escrito por Oliver Stone y está inspirada por el filme del mismo nombre de 1932, dirigido por Howard Hawks. La cinta relata el ascenso y caída de Tony Montana (Al Pacino), un refugiado cubano que logra formar un imperio basado en el tráfico de drogas.",
                Image = new ThumbnailUrl
                {
                    Url = "https://images2.minutemediacdn.com/image/upload/c_crop,h_1010,w_1800,x_0,y_120/f_auto,q_auto,w_1100/v1554993278/shape/mentalfloss/62429-scarface-featured.jpg",
                },
                Media = new List<MediaUrl>
                {
                    new MediaUrl()
                    {
                        Url = "https://www.youtube.com/watch?v=a_z4IuxAqpE",
                    },
                },
                Buttons = new List<CardAction>
                {
                    new CardAction()
                    {
                        Title = "Ver en IMDb",
                        Type = ActionTypes.OpenUrl,
                        Value = "https://www.imdb.com/title/tt0086250/",
                    },
                },
            };

            return videoCard;
        }

        public static AudioCard GetAudioCard()
        {
            var audioCard = new AudioCard
            {
                Title = "I am your father",
                Subtitle = "Star Wars: Episode V - The Empire Strikes Back",
                Text = "The Empire Strikes Back (also known as Star Wars: Episode V – The Empire Strikes Back)" +
                       " is a 1980 American epic space opera film directed by Irvin Kershner. Leigh Brackett and" +
                       " Lawrence Kasdan wrote the screenplay, with George Lucas writing the film's story and serving" +
                       " as executive producer. The second installment in the original Star Wars trilogy, it was produced" +
                       " by Gary Kurtz for Lucasfilm Ltd. and stars Mark Hamill, Harrison Ford, Carrie Fisher, Billy Dee Williams," +
                       " Anthony Daniels, David Prowse, Kenny Baker, Peter Mayhew and Frank Oz.",
                Image = new ThumbnailUrl
                {
                    Url = "https://upload.wikimedia.org/wikipedia/en/3/3c/SW_-_Empire_Strikes_Back.jpg",
                },
                Media = new List<MediaUrl>
                {
                    new MediaUrl()
                    {
                        Url = "https://wavlist.com/wav/father.wav",
                    },
                },
                Buttons = new List<CardAction>
                {
                    new CardAction()
                    {
                        Title = "Read More",
                        Type = ActionTypes.OpenUrl,
                        Value = "https://en.wikipedia.org/wiki/The_Empire_Strikes_Back",
                    },
                },
            };

            return audioCard;
        }
    }
}