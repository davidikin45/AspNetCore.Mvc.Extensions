using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Notifications
{
    //https://medium.com/@mirceaoprea/sending-automatic-messages-to-slack-from-asp-net-core-with-hangfire-86b60d09b289
    //services.AddHttpClient<ISlackMessageService, SlackMessageSender>();
    public class SlackMessageSender : ISlackMessageService
    {
        private readonly HttpClient _client;

        public SlackMessageSender(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = new Uri("https://hooks.slack.com");
        }

        public async Task SendMessage(string channel, string message)
        {
            var contentObject = new { text = message };
            var contentObjectJson = JsonSerializer.Serialize(contentObject);
            var content = new StringContent(contentObjectJson, Encoding.UTF8, "application/json");

            var result = await _client.PostAsync(channel, content);
            var resultContent = await result.Content.ReadAsStringAsync();
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Task failed.");
            }
        }
    }
}
