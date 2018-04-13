using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Collections.Generic;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json.Linq;
using System;

namespace SchoolAlert
{
    public static class SchoolAlert
    {
        const string SubscriptionValidationEvent = "Microsoft.EventGrid.SubscriptionValidationEvent";

        [FunctionName("SchoolAlert")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var content = req.Content;

            string jsonContent = await content.ReadAsStringAsync();
            log.Info($"Received Event with payload: {jsonContent}");

            var subject = "";
            var plainTextContent = "";
            var htmlContent = "";
            bool sendEmail = false;

                    var events = JsonConvert.DeserializeObject<EventGridEvent[]>(jsonContent);
                    foreach (EventGridEvent eventGridEvent in events)
                    {
                        JObject dataObject = eventGridEvent.Data as JObject;

                        // Deserialize the event data into the appropriate type based on event type
                        if (string.Equals(eventGridEvent.EventType, SubscriptionValidationEvent, StringComparison.OrdinalIgnoreCase))
                        {
                            var eventData = dataObject.ToObject<SubscriptionValidationEventData>();
                            log.Info($"Got SubscriptionValidation event data, validation code: {eventData.ValidationCode}, topic: {eventGridEvent.Topic}");
                            // Do any additional validation (as required) and then return back the below response
                            var responseData = new SubscriptionValidationResponseData();
                            responseData.ValidationResponse = eventData.ValidationCode;
                            return req.CreateResponse(HttpStatusCode.OK, responseData);
                        }
                        else if (string.Equals(eventGridEvent.EventType, "LifeSaver.Alerts.ActiveShooterDetected", StringComparison.OrdinalIgnoreCase))
                        {
                            subject = "School Alert";
                            plainTextContent = "Umbrella happened.  Be aware.";
                            htmlContent = "Umbrella happened.  Be aware.";
                            sendEmail = true;

                        }
                        else if (string.Equals(eventGridEvent.EventType, "LifeSaver.Alerts.ActiveShooterResolved", StringComparison.OrdinalIgnoreCase))
                        {
                            subject = "School Alert Resolved";
                            plainTextContent = "Umbrella was captured.";
                            htmlContent = "Umbrella was captured.";
                            sendEmail = true;

                        }
                    }

            if (sendEmail)
            {
                log.Info($"Ready to send alert email");

                var apiKey = "<enter send grid api key>";
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("<from email>");
                var to = new EmailAddress("<to email addresses");


                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                log.Info($"Email sent");
            }

            return req.CreateResponse(HttpStatusCode.OK);

        }
    }

    class SubscriptionValidationEventData
    {
        public string ValidationCode { get; set; }
    }

    class SubscriptionValidationResponseData
    {
        public string ValidationResponse { get; set; }
    }
}
