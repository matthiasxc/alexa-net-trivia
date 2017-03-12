using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Newtonsoft.Json;
using DbOptions.DynamoDb;
using Amazon.DynamoDBv2.DataModel;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SpaceGeek
{
    public class Function
    {
        public List<FactResource> GetResources()
        {
            List<FactResource> resources = new List<FactResource>();
            FactResource enUSResource = new FactResource("en-US");
            enUSResource.SkillName = "American Science Facts";
            enUSResource.GetFactMessage = "Here's your science fact: ";
            enUSResource.HelpMessage = "You can say tell me a science fact, or, you can say exit... What can I help you with?";
            enUSResource.HelpReprompt = "You can say tell me a science fact to start";
            enUSResource.StopMessage = "Goodbye!";
            enUSResource.Facts.Add("A year on Mercury is just 88 days long.");
            enUSResource.Facts.Add("Despite being farther from the Sun, Venus experiences higher temperatures than Mercury.");
            enUSResource.Facts.Add("Venus rotates counter-clockwise, possibly because of a collision in the past with an asteroid.");
            enUSResource.Facts.Add("On Mars, the Sun appears about half the size as it does on Earth.");
            enUSResource.Facts.Add("Earth is the only planet not named after a god.");
            enUSResource.Facts.Add("Jupiter has the shortest day of all the planets.");
            enUSResource.Facts.Add("The Milky Way galaxy will collide with the Andromeda Galaxy in about 5 billion years.");
            enUSResource.Facts.Add("The Sun contains 99.86% of the mass in the Solar System.");
            enUSResource.Facts.Add("The Sun is an almost perfect sphere.");
            enUSResource.Facts.Add("A total solar eclipse can happen once every 1 to 2 years. This makes them a rare event.");
            enUSResource.Facts.Add("Saturn radiates two and a half times more energy into space than it receives from the sun.");
            enUSResource.Facts.Add("The temperature inside the Sun can reach 15 million degrees Celsius.");
            enUSResource.Facts.Add("The Moon is moving approximately 3.8 cm away from our planet every year.");

            resources.Add(enUSResource);
            return resources;
        }

        string stateTable = "alexaStateTable";

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<SkillResponse> FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            SkillResponse response = new SkillResponse();
            response.Response.ShouldEndSession = false;
            IOutputSpeech innerResponse = null;
            var log = context.Logger;
            log.LogLine($"Skill Request Object:");
            log.LogLine(JsonConvert.SerializeObject(input));

            DynamoHelper dh = new DynamoHelper();
            var hasTable = await dh.VerifyTable(stateTable);

            if (!hasTable)
                hasTable = await dh.CreateTable(stateTable, "RequestId");


            var savedRequest = await SaveRequest(input, dh.GetContext());

            var allResources = GetResources();
            var resource = allResources.FirstOrDefault();

            if (input.GetRequestType() == typeof(LaunchRequest))
            {
                log.LogLine($"Default LaunchRequest made: 'Alexa, open Science Facts");
                innerResponse = new PlainTextOutputSpeech();
                (innerResponse as PlainTextOutputSpeech).Text = emitNewFact(resource, true);

            }
            else if (input.GetRequestType() == typeof(IntentRequest))
            {
                var intentRequest = (IntentRequest)input.Request;

                switch (intentRequest.Intent.Name)
                {
                    case "AMAZON.CancelIntent":
                        log.LogLine($"AMAZON.CancelIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.StopMessage;
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.StopIntent":
                        log.LogLine($"AMAZON.StopIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.StopMessage;
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.HelpIntent":
                        log.LogLine($"AMAZON.HelpIntent: send HelpMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.HelpMessage;
                        break;
                    case "GetFactIntent":
                        log.LogLine($"GetFactIntent sent: send new fact");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = emitNewFact(resource, false);
                        break;
                    case "GetNewFactIntent":
                        log.LogLine($"GetFactIntent sent: send new fact");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = emitNewFact(resource, false);
                        break;
                    default:
                        log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.HelpReprompt;
                        break;
                }
            }

            response.Response.OutputSpeech = innerResponse;
            response.Version = "1.0";
            log.LogLine($"Skill Response Object...");
            log.LogLine(JsonConvert.SerializeObject(response));
            return response;
        }

        public string emitNewFact(FactResource resource, bool withPreface)
        {
            Random r = new Random();
            if(withPreface)
                return resource.GetFactMessage + resource.Facts[r.Next(resource.Facts.Count)];
            return resource.Facts[r.Next(resource.Facts.Count)];
        }

        public async Task<bool> SaveRequest(SkillRequest request, DynamoDBContext context)
        {
            try
            {
                AlexaRequest cleanedRequest = new AlexaRequest();
                cleanedRequest.RequestId = request.Request.RequestId;
                cleanedRequest.RequestType = request.Request.Type;
                cleanedRequest.SessionId = request.Session.SessionId;
                cleanedRequest.ApplicationId = request.Session.Application.ApplicationId;
                cleanedRequest.UserId = request.Session.User.UserId;
                cleanedRequest.UserAccessToken = request.Session.User.AccessToken;
                cleanedRequest.Timestamp = request.Request.Timestamp;
                if (request.GetRequestType() == typeof(IntentRequest))
                {
                }
                else if (request.GetRequestType() == typeof(AudioPlayerRequest))
                {
                    var audioRequest = (AudioPlayerRequest)request.Request;
                    cleanedRequest.Intent = audioRequest.AudioRequestType.ToString();
                    cleanedRequest.EnqueuedAudioToken = audioRequest.EnqueuedToken;
                    cleanedRequest.OffsetInMilliseconds = audioRequest.OffsetInMilliseconds;
                }

                await context.SaveAsync<AlexaRequest>(cleanedRequest);

                return true;
            }
            catch
            {
                return false;
            }

        }

    }
        
    public class FactResource
    {
        public FactResource(string language)
        {
            this.Language = language;
            this.Facts = new List<string>();
        }

        public string Language { get; set; }
        public string SkillName { get; set; }
        public List<string> Facts { get; set; }
        public string GetFactMessage { get; set; }
        public string HelpMessage { get; set; }
        public string HelpReprompt { get; set; }
        public string StopMessage { get; set; }
    }


   
}
