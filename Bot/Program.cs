using System;
using Discord;
using System.IO;
using System.Threading.Tasks;
using Discord.Net;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Bot
{
    class Program
    {
        private protected string Token = File.ReadAllText(@"X:\_DiscordBot\1.txt");

        private const string PREFIX = "!";

        private int PREFIX_POS = 0;

        private DiscordSocketClient bot;

        private CommandService commands;

        private IServiceProvider services;

        static void Main() => new Program().RunBot().GetAwaiter().GetResult();

        public async Task RunBot()
        {
            bot = new DiscordSocketClient();

            commands = new CommandService();

            services = new ServiceCollection().AddSingleton(bot).AddSingleton(commands).BuildServiceProvider();

            await RegisterCommands();

            bot.Log += BotLog;

            await bot.LoginAsync(TokenType.Bot, Token);

            Token = null;                                                   // This is so noone can read the token from the memory

            await bot.StartAsync();

            await Task.Delay(-1);
        }

        private Task BotLog(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommands()
        {
            bot.MessageReceived += HandleCommand;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }

        private async Task HandleCommand(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;

            var context = new SocketCommandContext(bot, message);

            if (message.Author.IsBot) return;

            if(message.HasStringPrefix(PREFIX, ref PREFIX_POS))
            {
                var result = await commands.ExecuteAsync(context, PREFIX_POS, services);
                if (!result.IsSuccess) Console.WriteLine(result.Error + ", " + result.ErrorReason);
            }
        }
    }
}
