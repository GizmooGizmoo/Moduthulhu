﻿using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.Module;
using System.IO;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using Lomztein.Moduthulhu.Cross;
using Lomztein.Moduthulhu.Core.Bot.Client;

namespace Lomztein.Moduthulhu.Core.Bot {

    /// <summary>
    /// A wrapper for the Discord.NET DiscordClient.
    /// </summary>
    public class Core {

        // TODO: Allow for sending bot-wide messages directly in console.

        public DateTime BootDate { get; private set; }

        internal ClientManager ClientManager { get; private set; }
        internal ModuleLoader ModuleLoader { get; private set; }
        internal Clock.Clock Clock { get; private set; }
        internal ErrorReporter ErrorReporter { get; private set; }
        public UserList BotAdministrators { get; private set; }

        public string BaseDirectory { get => AppContext.BaseDirectory; }

        private DiscordSocketConfig socketConfig = new DiscordSocketConfig () {
            DefaultRetryMode = RetryMode.AlwaysRetry,
        };

        internal async Task InitializeCore () {
            BootDate = DateTime.Now;

            ClientManager = new ClientManager (this);
            ModuleLoader = new ModuleLoader (this);
            Clock = new Clock.Clock (1);

            ClientManager.InitializeClients ();

            ErrorReporter = new ErrorReporter (this);
            ClientManager.OnExceptionCaught += ErrorReporter.ReportError;
            BotAdministrators = new UserList (Path.Combine (BaseDirectory, "AdministratorIDs"));

            await Task.Delay (-1);
            Log.Write (Log.Type.BOT, "Shutting down..");
        }
    }
}