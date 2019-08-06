using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace SafeSpaceSoundBoard
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private string _token;

        public async Task MainAsync()
        {
            _token = "NTY0MjE0NzQyNzgxMDAxNzUw.XKkpeQ.94fwXmHsK5YtjtLTc0GZp1Uwa74";
            _client = new DiscordSocketClient();

            var services = ConfigureServices();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync(services);

            //Below is used to read the token from a config file.
            //Using hardcoded string for development purposes.
            //await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                // Base
                .AddSingleton(_client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .BuildServiceProvider();
        }
    }
}
