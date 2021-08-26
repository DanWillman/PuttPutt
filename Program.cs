using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using PuttPutt.Commands;

namespace PuttPutt
{
    public class Program
    {
        private CancellationTokenSource cts {get;set;}
        private DiscordClient client;
        private CommandsNextExtension commands;

        static async Task Main(string[] args) => await new Program().InitBot(args);

        async Task InitBot(string[] args)
        {
            cts = new CancellationTokenSource();
            var json = "";
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            var configs = JsonConvert.DeserializeObject<ConfigJson>(json);
            var discordConfig = new DiscordConfiguration
            {
                Token = configs.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            client = new DiscordClient(discordConfig);

            client.Ready += Client_Ready;
            client.GuildAvailable += Client_GuildAvailable;
            client.ClientErrored += Client_ClientErrored;

            //client.MessageCreated += EventHandlers.

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new[] {"!"},
                EnableDms = true,
                EnableMentionPrefix = true
            };

            commands = client.UseCommandsNext(commandsConfig);
            commands.CommandExecuted += Commands_CommandExecuted;
            commands.CommandErrored += Commands_CommandErrored;

            commands.RegisterCommands<BasicCommands>();
            commands.RegisterCommands<AdminCommands>();

            await client.ConnectAsync();
            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

        private async Task Client_Ready(DiscordClient client, ReadyEventArgs e)
        {
            Console.WriteLine("[info] Client is ready");
            await Task.CompletedTask;
        }

        private async Task Client_GuildAvailable(DiscordClient client, GuildCreateEventArgs e)
        {
            Console.WriteLine($"Guild available: {e.Guild.Name}");
            await Task.CompletedTask;
        }

        private async Task Client_ClientErrored(DiscordClient client, ClientErrorEventArgs e)
        {
            Console.WriteLine($"Exception occured: {e.Exception.Message}");
            await Task.CompletedTask;
        }

        private async Task Commands_CommandExecuted(CommandsNextExtension cne, CommandExecutionEventArgs e)
        {
            Console.WriteLine($"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");
            await Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandsNextExtension cne, CommandErrorEventArgs e)
        {
            Console.WriteLine($"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);
            await Task.CompletedTask;
        }
    }

    public struct ConfigJson
    {
        [JsonProperty("bot_token")]
        public string Token { get; private set; }
    }
}
