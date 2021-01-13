using System;
using System.Collections.Generic;
using System.Text;
using Kaiheila.Client;
using Kaiheila.Client.WebHook;
using Kaiheila.Events;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace KaiheilaInspector
{
    /// <summary>
    /// Bot Host.
    /// </summary>
    public class BotHost : IDisposable
    {
        public BotHost(
            ConfigHost configHost,
            ILogger<BotHost> logger)
        {
            _configHost = configHost;
            _logger = logger;

            _bot = WebHookClient.CreateWebHookClient()
                .Configure(options =>
                {
                    options
                        .Listen(_configHost.Config.Port)
                        .UseEncryptKey(_configHost.Config.Auth.EncryptKey)
                        .UseVerifyToken(_configHost.Config.Auth.VerifyToken)
                        .UseBotAuthorization(_configHost.Config.Auth.Token);
                }).Build();

            _bot.Start();

            _bot.Event.Subscribe(ProcessEvent);
        }

        private void ProcessEvent(KhEventBase eventBase)
        {
            if (!(eventBase is KhEventTextMessage msg)) return;

            string content = msg.Content;
            string patten = "#" + _configHost.Config.SelfId;

            // Now, reject any event that not @ the bot.
            if (!content.Contains(patten)) return;

            _logger.LogDebug("Received content: {content}", content);

            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"发送者ID：{msg.User.Id}");
            builder.AppendLine($"消息ID：{msg.Id}");

            // Parse content
            JObject raw = JObject.Parse(msg.Raw);

            try
            {
                string quote = raw["extra"]["quote"]["id"].ToObject<string>();
                builder.AppendLine($"引用消息ID：{quote}");
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Ejected when parsing quote.");
            }

            try
            {
                string guild = raw["extra"]["guild_id"].ToObject<string>();
                builder.AppendLine($"频道ID：{guild}");

                List<string> mention = raw["extra"]["mention"].ToObject<List<string>>();
                mention.ForEach(x => builder.AppendLine($"提及用户ID：{x}"));
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Ejected when parsing extra.");
            }

            msg.Content = builder.ToString();

            try
            {
                _bot.Send(msg);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Ejected when sending.");
            }
        }

        public void Dispose()
        {
            _bot?.Dispose();
        }

        private Bot _bot;

        private readonly ConfigHost _configHost;

        private readonly ILogger<BotHost> _logger;
    }
}
