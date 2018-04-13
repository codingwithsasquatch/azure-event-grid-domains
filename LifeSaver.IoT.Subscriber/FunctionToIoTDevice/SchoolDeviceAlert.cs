using Microsoft.Azure.Devices;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace LifeSaver.IoT.Subscriber
{
    public static class SchoolDeviceAlert
    {
        static RegistryManager registryManager;
        static string connectionString = Environment.GetEnvironmentVariable("iotHubConnectionString");
        static string iotDeviceId = Environment.GetEnvironmentVariable("iotDeviceId");
        static string alertActive = "LifeSaver.Alerts.ActiveShooterDetected";
        static string alertInactive = "LifeSaver.Alerts.ActiveShooterResolved";

        [FunctionName("SchoolDeviceAlert")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage request, TraceWriter log)
        {
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);

            var content = request.Content;
            string jsonContent = await content.ReadAsStringAsync();
            log.Info($"Received Event with payload: {jsonContent}");

            IEnumerable<string> headerValues;
            if (request.Headers.TryGetValues("Aeg-Event-Type", out headerValues))
            {
                var eventTypeHeaderValue = headerValues.FirstOrDefault();
                if (eventTypeHeaderValue == "SubscriptionValidation")
                {
                    var events = JsonConvert.DeserializeObject<GridEvent[]>(jsonContent);
                    var code = events[0].Data["validationCode"];
                    return new HttpResponseMessage(HttpStatusCode.OK) {
                        Content = new StringContent(@"{ 'validationResponse': '" + code + "'}")
                    };
                }
                else if (eventTypeHeaderValue == "Notification")
                {
                    var events = JsonConvert.DeserializeObject<GridEvent[]>(jsonContent);
                    var eventType = events[0].EventType;
                    if (eventType == alertActive)
                    {
                        await SetStateActive();
                    }
                    else if (eventType == alertInactive)
                    {
                        await SetStateInactive();
                    }
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        public static async Task SetStateActive()
        {
            await SetState(1, 255, 0, 0);
        }

        public static async Task SetStateInactive()
        {
            await SetState(1, 0, 255, 0);
        }

        public static async Task SetState(int _rgbLEDState, int _rgbLEDR, int _rgbLEDG, int _rgbLEDB)
        {
            var twin = await registryManager.GetTwinAsync(iotDeviceId);
            var statePatch = new
            {
                properties = new
                {
                    desired = new
                    {
                        rgbLEDState = _rgbLEDState,
                        rgbLEDR = _rgbLEDR,
                        rgbLEDG = _rgbLEDG,
                        rgbLEDB = _rgbLEDB
                    }
                }
            };
            await registryManager.UpdateTwinAsync(twin.DeviceId, JsonConvert.SerializeObject(statePatch), twin.ETag);
        }
    }

    public class GridEvent
    {
        public string Id { get; set; }
        public string EventType { get; set; }
        public string Subject { get; set; }
        public DateTime EventTime { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public string Topic { get; set; }

    }
}
