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
        private static IVoiceChannel voiceChannel;

        [Command("help")]
        public async Task Help()
        {

            var embed = new EmbedBuilder();
            string message =
            "The following commands are supported and explained below:\n\n" +
            "!join: This command will cause the bot to join the audio channel you're currently connected to.\n" +
            "!play sound: This command will cause the bot to play a sound in the channel it's connected to.\n";

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
            if (voiceChannel == null)
            {
                voiceChannel = channel;
            }

            // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
            var audioClient = await channel.ConnectAsync();
            //string path = Path.Combine(Environment.CurrentDirectory, @"Audio\", "finalfantasyvictory.mp3");
            //await SendAsync(audioClient, path);
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

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlaySound([Remainder] string args = "")
        {

            if (voiceChannel == null) { await Context.Channel.SendMessageAsync("Bot must be in a voice channel, try !join"); return; }

            // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
            var audioClient = await voiceChannel.ConnectAsync();
            
            var arguments = args.Split(' ');
            var soundbite = arguments.ElementAt(0);

            //TODO set up dictionary of keywords and soundfiles.
            //until then just take the name of the sound as the input.
            //soundbite should be "finalfantasyvictory.mp3"
            string path = Path.Combine(Environment.CurrentDirectory, @"Audio\", soundbite);            

            await SendAsync(audioClient, path);
        }
    }
}

