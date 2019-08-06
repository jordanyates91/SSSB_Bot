using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System.Timers;
using Discord;
using Discord.Commands;
using Discord.Audio;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SafeSpaceSoundBoard
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task Help()
        {

            var embed = new EmbedBuilder();
            string message =
            "The following commands are supported and explained below:\n\n" +

            "!play <@user> <time-out>: This command is used to initiate a game.\n" +
            "If an opponent is not specified with <@user> then the game will default to you playing against the bot.\n" +
            "If a <time-out> of no greater than 24 hours is specied that will be the time before the game completes, " +
            "if not specified it will default to 15 seconds.\n" +
            "Timeout expects values in this format: HH:MM:SS\n\n" +

            "!record <@user1> <@user2>: This command will return a record of wins/losses/ties for <@user1>.\n" +
            "If <@user2> is specified it will show the wins/losses/ties between <@user1> and <@user2>.\n" +
            "If @user2 is not specified it will show wins/losses/ties between <@user1> and the bot.\n\n" +

            "!time-left: This command will display how much time is remaining in the game before it ends.\n\n" +

            "!forfiet: This command will cause you to forfiet an ongoing game you are playing.\n\n";

            embed.WithTitle("Help Message");
            embed.WithDescription(message);
            embed.WithColor(new Color(0, 255, 0));
            var output = embed.Build();
            await Context.Channel.SendMessageAsync("", false, output);
        }


        // The command's Run Mode MUST be set to RunMode.Async, otherwise, being connected to a voice channel will block the gateway thread.
        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinChannel(IVoiceChannel channel = null)
        {
            // Get the audio channel
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null) { await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

            // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
            var audioClient = await channel.ConnectAsync();

            string path = Path.Combine(Environment.CurrentDirectory, @"Audio\", "finalfantasyvictory.mp3");

            await SendAsync(audioClient, path);
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }


        private async Task SendAsync(IAudioClient client, string path)
        {
            // Create FFmpeg using the previous example
            using (var ffmpeg = CreateStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }
            }
        }

    }
}

