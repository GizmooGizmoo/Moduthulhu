﻿using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Linq;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.Plugins;

namespace Lomztein.Moduthulhu.Core.Bot.Client
{
    public class BotClient // TODO: Figure out how to implement horizontal scaleability, as in multiple shards over different processes.
    {
        public DateTime BootDate { get; private set; }
        public TimeSpan Uptime => DateTime.Now - BootDate;

        public BotCore Core { get; private set; }

        public ClientConfiguration Configuration;

        public static string DataDirectory => BotCore.DataDirectory;

        private BotShard[] _shards;
        private IEnumerable<SocketGuild> AllGuilds => _shards.SelectMany (x => x.Guilds);
        private DiscordSocketClient FirstClient => _shards.First ().Client;

        private BotStatus _status;
        private readonly Clock _statusClock = new Clock(1, "StatusClock");
        private UserList _botAdministrators;
        private int _consecutiveOfflineMinutes;
        private readonly int _automaticOfflineMinutesTreshold = 10;

        public event Func<Exception, Task> ExceptionCaught;

        internal BotClient (BotCore core) {

            BootDate = DateTime.Now;
            Core = core;

            Configuration = LoadConfiguration(DataDirectory + "/Configuration");

            Log.Write (Log.Type.BOT, "Creating bot client with token " + Configuration.Token);
        }

        internal async Task Initialize()
        {
            PluginLoader.ReloadPluginAssemblies();
            _botAdministrators = new UserList(DataDirectory + "/Administrators");

            InitializeShards();
            await AwaitAllConnected().ConfigureAwait (false);

            InitStatus();
            _statusClock.OnMinutePassed += StatusClock_OnMinutePassed;
            _statusClock.OnMinutePassed += _status.Cycle;
            _statusClock.Start();
        }

        internal void InitStatus()
        {
            int easterEggFraction = 1000;
            string universalHelp = "!help | !plugins | !config";

            _status = new BotStatus(x => FirstClient.SetActivityAsync(x), 10, new StatusMessage[] {
                new StatusMessage(ActivityType.Watching, () => new Random().Next(0, easterEggFraction) == 0 ? "Bible Black" : universalHelp),
                new StatusMessage(ActivityType.Watching, () => new Random().Next(0, easterEggFraction) == 0 ? $"{AllGuilds.Sum(x => x.MemberCount)} puny souls waste away their hilariously short lives." : $"{AllGuilds.Count()} servers!"),
                new StatusMessage(ActivityType.Watching, () => new Random().Next(0, easterEggFraction) == 0 ? "the sweet cries of the fresh virgin sacrifices." : universalHelp),
                new StatusMessage(ActivityType.Watching, () => new Random().Next(0, easterEggFraction) == 0 ? $"{AllGuilds.Count ()} realms lost to madness." : $"{AllGuilds.Sum(x => x.MemberCount)} people!"),
                new StatusMessage(ActivityType.Watching, () => new Random().Next(0, easterEggFraction) == 0 ? "the dice of the vast cosmos." : universalHelp),
                new StatusMessage(ActivityType.Watching, () => new Random().Next(0, easterEggFraction) == 0 ? "V̌̾͒̓͏̸̼͔̘͎̳̦̮̰̹̥Ǫ̪͎̜̝͙̅ͫ͊̃͗̾̍ͣ̔̾͊͆ͭ͗̏͆̀͘͟͠I̴͛͌ͦ͊̇̾ͮ͂̈̌͏̪̜̳͙̰̝̺̱͈̗̥D̡̳͈̠͔̲̳̤̱͚̤ͥͮͤͪ̄ͤ͐̆̿ͩ͐ͭ̋̂͗̔ͬͦ͊" : $"for {Uptime.Days} days!"),
                new StatusMessage(ActivityType.Watching, () => new Random().Next(0, easterEggFraction) == 0 ? "Cars 2" : universalHelp),
            });
        }

        private ClientConfiguration LoadConfiguration (string path)
        {
            Log.Write(Log.Type.BOT, "Loading configuration for bot client.");
            Configuration = ClientConfiguration.Load(path);
            if (Configuration == null)
            {
                // If no file exists, create a new one.
                Configuration = new ClientConfiguration();
                Configuration.Save(path);
            }

            Configuration.CheckValidity();
            
            return Configuration;
        }



        private async Task StatusClock_OnMinutePassed(DateTime currentTick, DateTime lastTick)
        {
            if (_shards.Any (x => x.IsConnected == false)) {
                _consecutiveOfflineMinutes++;
                Log.Write(Log.Type.WARNING, $"Disconnected shard detected, commencing auto-shutdown in {_consecutiveOfflineMinutes}/{_automaticOfflineMinutesTreshold} minutes..");

                foreach (var shard in _shards.Where (x => !x.IsConnected))
                {
                    Log.Bot($"Attempting reconnect of shard {shard.ShardId}.");
                    await shard.Connect();
                }
            }
            else
            {
                if (_consecutiveOfflineMinutes > 0)
                {
                    Log.Write(Log.Type.CONFIRM, $"All connections reestablished, auto-shutdown cancelled.");
                }

                _consecutiveOfflineMinutes = 0;
            }

            if (_consecutiveOfflineMinutes >= _automaticOfflineMinutesTreshold)
            {
                Log.Write(Log.Type.CRITICAL, $"Commencing automatic shutdown due to disconnected shard. :(");
                Shutdown();
            }
        }

        private void Shutdown()
        {
            Core.Shutdown();
        }

        internal void InitializeShards () {
            _shards = new BotShard[Configuration.TotalShards];
            for (int i = Configuration.ShardRange.Min; i < Configuration.ShardRange.Max; i++) {
                Log.Bot($"Creating shard {i + 1}/{Configuration.ShardRange.Max}, Id = 0.");
                _shards[i] = CreateShard (i, Configuration.TotalShards);
                _shards[i].Run();
            }
        }

        public bool IsBotAdministrator(ulong userId)
        {
            return _botAdministrators.Contains(userId);
        }

        internal BotShard CreateShard (int shardId, int totalShards) {
            BotShard shard = new BotShard (this, Configuration.Token, shardId, totalShards);
            shard.ExceptionCaught += OnExceptionCaught;
            return shard;
        }

        private Task OnExceptionCaught (Exception exception)
        {
            FirstClient.SetGameAsync(exception.Message + " from " + exception.Source + " in " + exception.TargetSite);
            return ExceptionCaught?.Invoke(exception);
        }

        private async Task AwaitAllConnected () {
            await Task.WhenAll (_shards.Select (x => x.AwaitConnected ()).ToArray ());
        }

        public BotShard[] GetShards () => _shards.Clone () as BotShard[];

        public override string ToString() => $"Shards: {Configuration.TotalShards}";
        public string[] GetShardsStatus() => _shards.Select (x => x == null ? $"Disconnected shard, client will auto-shutdown in {_consecutiveOfflineMinutes} / {_automaticOfflineMinutesTreshold} minutes." : x.ToString ()).ToArray ();
    }
}
