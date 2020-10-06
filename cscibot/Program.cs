using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace cscibot
{
    public class CSCIBot : IBot
    {
        public static CSCIBot Instance { get; internal set; }

        public async void StartBot() => await Start();

        public async void StopBot() => await Stop();

        public static DiscordSocketClient _c;
        public static bool _r, _l;

        public List<SocketMessage> sentMessages;

        public string Name { get => "CSCI 1133"; }
        public string Author { get => "Ben Nylund"; }
        public string Directory { get; set; }


        public static void Main(string[] args) => new CSCIBot().Start().GetAwaiter().GetResult(); // Only needed for testing


        public async Task Start()
        {
            Log("CSCI 1133 Bot, Version 0.2");
            Instance = this;

            string token = "";
            if (File.Exists("config"))
            {
                foreach (string s in File.ReadAllLines("config"))
                    if (s.StartsWith("token="))
                        token = s.Split('=')[1];
            } else
                File.WriteAllText("config", "token=");

            Log("Starting bot...");
            Log("Using token '" + token + "'");

            _c = new DiscordSocketClient();
            await _c.LoginAsync(Discord.TokenType.Bot, token);
            await _c.StartAsync();
            
            sentMessages = new List<SocketMessage>();

            _c.Ready += Ready;

            await BotThread();
        }

        private async Task BotThread()
        {
            while (!_r) { Thread.Sleep(100); } // waiting for ready

            Log("Bot started!");
            while (_r)
            {
                // Bot tasks here
                Thread.Sleep(50);
            }
        }

        private async Task Stop()
        {
            Log("Stopping bot...");
            await _c.StopAsync();
            await _c.LogoutAsync();
            Log("Bot deactivated!");
            _r = false;
        }

        private async Task Ready()
        {
            Log("Connected.");
            _r = true;
            _c.MessageReceived += MessageReceived;
        }

        private async Task MessageReceived(SocketMessage arg)
        {
            if(!sentMessages.Contains(arg) && arg.Author.Id != _c.CurrentUser.Id)
            {
                sentMessages.Add(arg);

                if (arg.Channel.Id == 759102103200989254)
                {
                    string content = arg.Content;
                    await arg.DeleteAsync();

                    int section;
                    if(int.TryParse(content, out section))
                    {
                        if(section <= 72 && section >= 2 && section % 10 != 0)
                        {
                            bool found = false;
                            foreach (SocketRole role in _c.GetGuild(759101634948890645).Roles)
                                if (role.Name == "Lab " + section)
                                    found = true;

                            if(!found)
                            {
                                Log("Role doesn't exist for section " + section + ", creating...");
                                RestRole role = await _c.GetGuild(759101634948890645).CreateRoleAsync("Lab " + section, null, null, false, null);
                                Log("Created role with id " + role.Id);
                                await (arg.Author as IGuildUser).AddRoleAsync(role);
                                Log("Adding " + role.Name + " to " + arg.Author.Username);

                                // Check for already-existing channel. If there isn't one, create one.
                                found = false;
                                foreach (ITextChannel channel in _c.GetGuild(759101634948890645).TextChannels)
                                    if (channel.Name == "lab-" + section)
                                        found = true;

                                if (!found)
                                {
                                    RestTextChannel tc = await _c.GetGuild(759101634948890645).CreateTextChannelAsync("lab-" + section, (a) => { a.CategoryId = 759102041309708370; });
                                    RestVoiceChannel vc = await _c.GetGuild(759101634948890645).CreateVoiceChannelAsync("- Lab " + section + " -", (a) => { a.CategoryId = 759102041309708370; });

                                    OverwritePermissions allow_perms = new OverwritePermissions(connect: PermValue.Allow, viewChannel: PermValue.Allow, sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow, speak: PermValue.Allow);
                                    OverwritePermissions deny_perms = new OverwritePermissions(connect: PermValue.Deny, viewChannel: PermValue.Deny, sendMessages: PermValue.Deny, readMessageHistory: PermValue.Deny, speak: PermValue.Deny);
                                    await vc.AddPermissionOverwriteAsync(role, allow_perms);
                                    await vc.AddPermissionOverwriteAsync(_c.GetGuild(759101634948890645).EveryoneRole, deny_perms);
                                    await tc.AddPermissionOverwriteAsync(role, allow_perms);
                                    await tc.AddPermissionOverwriteAsync(_c.GetGuild(759101634948890645).EveryoneRole, deny_perms);
                                }
                            }

                            foreach (SocketRole role in _c.GetGuild(759101634948890645).Roles)
                                if (role.Name.ToLower().Contains("lab ") && role.Name != "Lab " + section)
                                {
                                    await (arg.Author as IGuildUser).RemoveRoleAsync(role);
                                }
                                else if (role.Name.ToLower() == "student")
                                {
                                    await (arg.Author as IGuildUser).AddRoleAsync(role);
                                    Log("Adding " + role.Name + " to " + arg.Author.Username);
                                } else if(role.Name == "Lab " + section)
                                {
                                    await (arg.Author as IGuildUser).AddRoleAsync(role);
                                    Log("Adding " + role.Name + " to " + arg.Author.Username);

                                    // Check for already-existing channel. If there isn't one, create one.
                                    found = false;
                                    foreach (ITextChannel channel in _c.GetGuild(759101634948890645).TextChannels)
                                        if (channel.Name == "lab-" + section)
                                            found = true;

                                    if (!found)
                                    {
                                        RestTextChannel tc = await _c.GetGuild(759101634948890645).CreateTextChannelAsync("lab-" + section, (a) => { a.CategoryId = 759102041309708370; });
                                        RestVoiceChannel vc = await _c.GetGuild(759101634948890645).CreateVoiceChannelAsync("- Lab " + section + " -", (a) => { a.CategoryId = 759102041309708370; });

                                        OverwritePermissions allow_perms = new OverwritePermissions(connect: PermValue.Allow, viewChannel: PermValue.Allow, sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow, speak: PermValue.Allow);
                                        OverwritePermissions deny_perms = new OverwritePermissions(connect: PermValue.Deny, viewChannel: PermValue.Deny, sendMessages: PermValue.Deny, readMessageHistory: PermValue.Deny, speak: PermValue.Deny);
                                        await vc.AddPermissionOverwriteAsync(role, allow_perms);
                                        await vc.AddPermissionOverwriteAsync(_c.GetGuild(759101634948890645).EveryoneRole, deny_perms);
                                        await tc.AddPermissionOverwriteAsync(role, allow_perms);
                                        await tc.AddPermissionOverwriteAsync(_c.GetGuild(759101634948890645).EveryoneRole, deny_perms);
                                    }
                                }
                        }
                    }
                } else if(arg.Channel.Id == 762877768315830273)
                {
                    string content = arg.Content;
                    await arg.DeleteAsync();

                    int section;
                    if (int.TryParse(content, out section))
                    {
                        if (section >= 1 && section <= 70 && (section % 10 == 0 || section == 1))
                        {
                            bool found = false;
                            foreach (SocketRole role in _c.GetGuild(759101634948890645).Roles)
                                if (role.Name == "Lecture " + section)
                                    found = true;

                            if (!found)
                            {
                                Log("Role doesn't exist for section " + section + ", creating...");
                                RestRole role = await _c.GetGuild(759101634948890645).CreateRoleAsync("Lecture " + section, null, null, false, null);
                                Log("Created role with id " + role.Id);
                                await (arg.Author as IGuildUser).AddRoleAsync(role);
                                Log("Adding " + role.Name + " to " + arg.Author.Username);

                                // Check for already-existing channel. If there isn't one, create one.
                                found = false;
                                foreach (ITextChannel channel in _c.GetGuild(759101634948890645).TextChannels)
                                    if (channel.Topic == "Section " + (section == 1 ? "001" : "0" + section))
                                        found = true;

                                if (!found)
                                {
                                    RestTextChannel tc = await _c.GetGuild(759101634948890645).CreateTextChannelAsync("lecture-chat", (a) => { a.CategoryId = 759103059695239240; a.Topic = "Section " + (section == 1 ? "001" : "0" + section); });
                                    RestVoiceChannel vc = await _c.GetGuild(759101634948890645).CreateVoiceChannelAsync("- Lecture Chat (0" + (section == 1 ? "01" : section + "") + ")" + " -", (a) => { a.CategoryId = 759103059695239240; });

                                    OverwritePermissions allow_perms = new OverwritePermissions(connect: PermValue.Allow, viewChannel: PermValue.Allow, sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow, speak: PermValue.Allow);
                                    OverwritePermissions deny_perms = new OverwritePermissions(connect: PermValue.Deny, viewChannel: PermValue.Deny, sendMessages: PermValue.Deny, readMessageHistory: PermValue.Deny, speak: PermValue.Deny);
                                    await vc.AddPermissionOverwriteAsync(role, allow_perms);
                                    await vc.AddPermissionOverwriteAsync(_c.GetGuild(759101634948890645).EveryoneRole, deny_perms);
                                    await tc.AddPermissionOverwriteAsync(role, allow_perms);
                                    await tc.AddPermissionOverwriteAsync(_c.GetGuild(759101634948890645).EveryoneRole, deny_perms);
                                }
                            }

                            foreach (SocketRole role in _c.GetGuild(759101634948890645).Roles)
                                if (role.Name.ToLower().Contains("lecture ") && role.Name != "Lecture " + section)
                                {
                                    await (arg.Author as IGuildUser).RemoveRoleAsync(role);
                                }
                                else if (role.Name.ToLower() == "student")
                                {
                                    await (arg.Author as IGuildUser).AddRoleAsync(role);
                                    Log("Adding " + role.Name + " to " + arg.Author.Username);
                                }
                                else if (role.Name == "Lecture " + section)
                                {
                                    await (arg.Author as IGuildUser).AddRoleAsync(role);
                                    Log("Adding " + role.Name + " to " + arg.Author.Username);

                                    // Check for already-existing channel. If there isn't one, create one.
                                    found = false;
                                    foreach (ITextChannel channel in _c.GetGuild(759101634948890645).TextChannels)
                                        if (channel.Topic == "Section " + (section == 1 ? "001" : "0" + section))
                                            found = true;

                                    if (!found)
                                    {
                                        RestTextChannel tc = await _c.GetGuild(759101634948890645).CreateTextChannelAsync("lecture-chat", (a) => { a.CategoryId = 759103059695239240; a.Topic = "Section " + (section == 1 ? "001" : "0" + section); });
                                        RestVoiceChannel vc = await _c.GetGuild(759101634948890645).CreateVoiceChannelAsync("- Lecture Chat (0"+(section == 1 ? "01" : section + "")+")" + " -", (a) => { a.CategoryId = 759103059695239240; });

                                        OverwritePermissions allow_perms = new OverwritePermissions(connect: PermValue.Allow, viewChannel: PermValue.Allow, sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow, speak: PermValue.Allow);
                                        OverwritePermissions deny_perms = new OverwritePermissions(connect: PermValue.Deny, viewChannel: PermValue.Deny, sendMessages: PermValue.Deny, readMessageHistory: PermValue.Deny, speak: PermValue.Deny);
                                        await vc.AddPermissionOverwriteAsync(role, allow_perms);
                                        await vc.AddPermissionOverwriteAsync(_c.GetGuild(759101634948890645).EveryoneRole, deny_perms);
                                        await tc.AddPermissionOverwriteAsync(role, allow_perms);
                                        await tc.AddPermissionOverwriteAsync(_c.GetGuild(759101634948890645).EveryoneRole, deny_perms);
                                    }
                                }
                        }
                    }
                }
            }
        }

        public void Log(string text)
        {
            Console.WriteLine("[" + DateTime.Now.ToLocalTime().ToString("h:mm:ss tt") + " CSCIBot] " + text);
            File.AppendAllText(Directory + "log", "\r\n[" + DateTime.Now.ToLocalTime().ToString("MM/dd/yyyy h:mm:ss tt") + "] " + text);
        }
    }
}
