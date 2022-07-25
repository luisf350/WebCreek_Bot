using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebCreekBot.Dto;

namespace WebCreekBot.Bots
{
    public class ImagenBot : ActivityHandler
    {
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await SendWelcomeMessageAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var reply = ProcessInput(turnContext);
            // Respond to the user.
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(
                        $"Bienvenido al Bot de Análisis de Imágenes." +
                        Environment.NewLine +
                        $"Seleccione una imagen para ser analizada...",
                        cancellationToken: cancellationToken);
                }
            }
        }

        // Given the input from the message, create the response.
        private static IMessageActivity ProcessInput(ITurnContext turnContext)
        {
            var activity = turnContext.Activity;
            return HandleIncomingAttachment(activity); ;
        }

        // Handle attachments uploaded by users. The bot receives an <see cref="Attachment"/> in an <see cref="Activity"/>.
        // The activity has a "IList{T}" of attachments.
        // Not all channels allow users to upload files. Some channels have restrictions
        // on file type, size, and other attributes. Consult the documentation for the channel for
        // more information. For example Skype's limits are here
        // <see ref="https://support.skype.com/en/faq/FA34644/skype-file-sharing-file-types-size-and-time-limits"/>.
        private static IMessageActivity HandleIncomingAttachment(IMessageActivity activity)
        {
            string replyText = string.Empty;
            foreach (var file in activity.Attachments)
            {
                // Determine where the file is hosted.
                var remoteFileUrl = file.ContentUrl;

                // Save the attachment to the system temp directory.
                var localFileName = Path.Combine(Path.GetTempPath(), file.Name);

                // Download the actual attachment
                using (var webClient = new WebClient())
                {
                    webClient.DownloadFile(remoteFileUrl, localFileName);
                }

                var apiResult = CallFaceApi(localFileName).Result;

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < apiResult.Count; i++)
                {
                    FaceApiAttributesDto item = apiResult[i].faceAttributes;
                    sb.AppendLine($"--- Persona {i + 1} ---");

                    sb.AppendLine($"Es {(item.gender == "male" ? "un hombre" : "una mujer")}.");
                    sb.AppendLine($"Tiene unos {item.age} años.");
                    sb.AppendLine($"Sus emociones son:");
                    sb.AppendLine($"- Enojado: {item.emotion.anger * 100} %");
                    sb.AppendLine($"- Rebeldia: {item.emotion.contempt * 100} %");
                    sb.AppendLine($"- Disgustado: {item.emotion.disgust * 100} %");
                    sb.AppendLine($"- Miedo: {item.emotion.fear * 100} %");
                    sb.AppendLine($"- Felicidad: {item.emotion.happiness * 100} %");
                    sb.AppendLine($"- Neutro: {item.emotion.neutral * 100} %");
                    sb.AppendLine($"- Triste: {item.emotion.sadness * 100} %");
                    sb.AppendLine($"- Sorprendido: {item.emotion.surprise * 100} %");

                    sb.AppendLine($"-----------------");
                }

                return MessageFactory.Text(sb.ToString());
            }

            return MessageFactory.Text("Nada para procesar...");
        }

        private static async Task<List<FaceApiResponseDto>> CallFaceApi(string filePath)
        {
            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", "85e56e2a63034b06a206794d445aac44");

            // Request parameters. A third optional parameter is "details".
            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false" +
                "&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses," +
                "emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

            // Assemble the URI for the REST API Call.
            string uri = "https://webcreekdetectfaces.cognitiveservices.azure.com/face/v1.0/detect" + "?" + requestParameters;

            HttpResponseMessage response;

            // Request body. Posts a locally stored JPEG image.
            byte[] byteData = GetImageAsByteArray(filePath);

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json"
                // and "multipart/form-data".
                content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call.
                response = await client.PostAsync(uri, content);

                // Get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();

                // Display the JSON response.
                return JsonConvert.DeserializeObject<List<FaceApiResponseDto>>(contentString);
            }
        }

        private static byte[] GetImageAsByteArray(string imageFilePath)
        {
            using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
    }
}