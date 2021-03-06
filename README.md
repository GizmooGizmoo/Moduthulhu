[![Codacy Badge](https://api.codacy.com/project/badge/Grade/9107d84996d94ad388733b47047f5c7d)](https://www.codacy.com/manual/Lomztein/Moduthulhu?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=Lomztein/Moduthulhu&amp;utm_campaign=Badge_Grade) [![Discord Bots](https://top.gg/api/widget/status/192707822876622850.svg)](https://top.gg/bot/192707822876622850)


# Moduthulhu - Modular Discord Bot

The 'Moduthulhu - Modular Discord Bot' is a Discord bot core framework build on the [Discord.NET API Wrapper by RougeException](https://github.com/RogueException/Discord.Net). This project intends to create a foundation framework on which more front-end bot functionality can be added through the runtime loading of functionality-containing Plugins. The core framework, standard- and first party plugins, as well as the [command framework](https://github.com/Lomztein/Advanced-Discord-Commands) are under active dvelopment, so bugs, issues, or strange behaviour might occur. Any general feedback / bug reports / suggestions are welcome [here!](https://github.com/Lomztein/Moduthulhu/issues)

Everything is subject to change.

## Usage Guide

### Quickstart Guide
 * `!help` To view all available root commands.
 * `!plugins` To view all available plugins.
 * `!plugins enable/disable` To add or remove plugins.
 * `!config` To configure plugins.
 * `!config settrigger <trigger>` To change command trigger.
 * Suffix `?` to a command to get help for it. Example: `!plugins ?`

Have fun!

### Detailed Guide

To get started using this bot, you need to know of a few primary commands, as well as how to request command help, or <i>'autodocs'</i>.

The first and arguably most important command is `!help` or one of it's aliases such as `!commands` or `!clist`. This command displays a list of all available root commands.

In order to enable and disable plugins for your server, the commands `!plugins enable` `!plugins disable` area available. To view a list of all, available or active plugins, the commands `!plugins all`, `!plugins available`, and `!plugins active` are your friends.

Finally, `!config` is where you configure all of the active plugins to fit your need. Many servers have mulitple bots, need the bots to respond to specific individual triggers. To change which trigger this bot responds to, use `!config settrigger <trigger>`, and `!config resettrigger` to reset it to the default `!`.

To view the previously mentioned autodocs for a command, suffix the call with an `?`, for example `!help ?`. This shows you a bunch of information about the command, such as aliases, available variants, and which types of arguments they take, and what they return. Autodocs are also displayed if a command fails to execute due to mismatched arguments.

Some commands have the suffix `(set)` behind them. This means that they are what is known as a 'Command Set', which is in itself a list of nested commands. The previously mentioned `!plugins` is one such set, that contains commands such as `enable` or `disable`. `!config` is another such set. If executing a command was like climbing a tree and picking a leaf, think of command sets as moving up a specific branch. Sets may also provide command functionality on their own, like `plugins` that return a list of available plugins when executed.

### Standard plugins

The core framework includes some standard plugins which are all enabled by default, as well as some which are critical and cannot be disabled. These are as follows:

 * Command Root: Plugin for managing commands. Any plugin may add commands to it using the aforementioned messaging system.
 * Plugin Manager: Plugin for enabling and disabling individual plugins, as well as display data about them.
 * Configuration: Plugin that exposes registered configuration options through commands.
 * Consent: Simple plugin that allows users to toggle whether or not they consent to storage of their personal data, in accordance with GDPR. Also allows users to request or delete any data the bot has stored, that is linked to their user ID.
 * Standard Commands: Plugin that adds the standard commands from the command framework. May be disabled.
 * Administrator: Plugin that allows for the basic administration of the bot client, unneccesary for most users and may be disabled.

## Deployment Guide

This bot is written in C#, targets the .NET Core 2.1 LTS framework, and is fully dockerizable and may be pulled from [Docker Hub](https://hub.docker.com/repository/docker/lomztein/moduthulhu), however do notice that it cannot just run out of the box, as it additionally needs a PostgreSQL database to store information on, as well as a mounted volume to read client configuration data as well as store error logs on. Basic familiarity with Docker is recommended, as that's about what I had when I dockerized it. C:

I personally run the bot on a Ubuntu 16.04 server using Docker, and it works perfectly well!

To fully run it using Docker, you need to take into account the following:
 * You must supply it with a connection string for PostgreSQL database, through environment variable 'PostgreSQLConnectionString'
 * To configure the core, you must be able to edit the Data folder, such as through mounting a volume.
 
Currently I run the container with following command: `sudo docker run --env 'PostgreSQLConnectionString=Server=<DBAddress>;Port=5432;Username=<DBUsername>;Password=<DBPassword>;Database=<Database>;' --name moduthulhu --mount 'src=moduthulhu_data,dst=/build/Data' --restart on-failure lomztein/moduthulhu:latest`

Any vulnurable information has been omitted, of course. DB is short for database, and you must fill out those slots with the connection information for your own database.

I intend to replace this with a Docker-compose file later on, however this works for the time being. Additionally, I run [Watchtower](https://github.com/containrrr/watchtower) to automatically update the bot when a new build is build though Docker Hub Automated Builds. It is a very convenient and easy to set up Continuous Deployment solution that I can heartedly recommend you try. If you believe you know a better alternative solution, feel free to let me know! :D

## Development Guide

If you desire to contribute to the bots available functionality by creating your own plugin, you can quite easily do so. After all, the framework is designed specifically to allow for that. There are a few prerequisites before you can do so, such as:

* An IDE, such as Microsoft Visual Studio or Visual Studio Code. Anything should do as long as it can compile .NET Core.
* The .NET Core 2.1 LTS framework SDK, which is required for any .NET Core based applications to be compiled.
* Either a copy of the source code from here, or the [assemblies downloaded through NuGet](https://www.nuget.org/packages/Lomztein.Moduthulhu.Core.Assemblies) for reference.

If you download the assemblies via NuGet, you need to instantiate a BotCore and call it's `InitializeCore (args)` method, which will start up the bot and all of it's systems. To stop the program from shutting down, call `GetAwaiter ().GetResult ()` on the Task object given by `InitializeCore`.

As an example, this is the `Main` method of the bot:
    `static void Main(string [ ] args) => new BotCore ().InitializeCore (args).GetAwaiter ().GetResult ();`
    
Once the Bot Core is launched, it will generate an empty Configuration file in the `./Data/` folder, and then promptly throw an exception because the config file is invalid. You'll have to fill it up yourself. It needs a Bot Token that you need to create [here](https://discordapp.com/developers/applications/). Shards min/max/total should just be 0/1/1 if you only need the bot for testing. More information on sharding can be found [here](https://discordapp.com/developers/docs/topics/gateway#sharding) if you're curious.

The bot attempts to read plugins from `IncludedPlugins.dll` in the root folder, and from the `/Data/Plugins/` folder. You should build your plugins to either of these in order to test them.

By default, the bot reads data from local JSON files. In order to change this, you have to set Environment Variable `DATABASE_TYPE` to one of the following supperted options.
 * Path/Json *(default)*
 * SQL/PostgreSQL

### Primary Features of the Core Framework

 * Runtime loading of plugin functionality, with the ability to toggle them on and off on a per-server basis. 
 * A simple plugin interface that is easy to develop with, and a base class for easier writing of new plugins.
 * Inter-plugin communication through a message system, which allows any plugin to call a registered action or function from another plugin.
 * State change tracker system that keeps track of system state changes between plugin reloads.
 * Easy-to-use build-in configuration system that allows for the registration of configuration options, which may then be exposed through any means imaginable.
 * On-the-fly error handling that catches anything that goes wrong in a plugin and prints the full stacktrace to a file, while the bot keeps living.
 * PostgreSQL database support used for both data and config storage, as well as whatever you may need.
 * Consent Assertion that keeps track of whether or not users consent to the storage of personal data in the bots database.

The most basic element you should be aware of is the "IPlugin" interface, as this is the interface between plugin and bot framework. However, it isn't really neccesary to worry about it, as it is much easier to work with the "PluginBase" abstract class, the default implementation of IPlugin. Inheriting from PluginBase will provide you with an easy foundation to work with, as well as a bunch of build in utility methods.  Additionally, a Descriptor attribute is required to be added to the class, as it is used to define the plugins author, name, as well as optionally a description and a version. An optional Source attribute may also be added, which contains links to an authors website, a source repository, and a link to where the plugins .dll file may be downloaded for patching.

Now to create an actual plugin.

### Here is an empty class that inherits from PluginBase:

```cs
[Descriptor ("Alan Smithee", "Example Plugin")]
class ExamplePlugin : PluginBase {

    public override void Initialize() {
        throw new NotImplementedException ();
    }

    public override void Shutdown() {
        throw new NotImplementedException ();
    }
}
```
The default namespace for the core is Lomztein.Moduthulhu.Core.

There are a few more members you can use, including two other Initialize functions that calls at different time during setup. You can also add the Dependancy attribute to declare that your plugin requires a certain other plugin to be able to function.

There are in total four methods defined by IPlugin, all of which are called by PluginManager.

* `PreInitialize (GuildHandler handler)` - Executed before anything else is done. It is recommended that messages and config options are registered here, as well as any setup that may be neccesary for other plugins to this one. Additionally it takes in the plugins GuildHandler to be assigned.
* `Initialize ()` - The "Default" initialize function, as well as the only one neccesary to worry about if you don't need to interact with other plugins at all.
* `PostInitialize ()` - Executed lastly. It is recommended to use this to process data given by other plugins.
* `Shutdown ()` - Executed when the plugin must shutdown, perhaps to be disabled or reloaded. Use this to revert any changes done to other plugins or the core.

Initialize and Shutdown must be implemented in your plugin class.

You have access to a GuildHandler through IPlugin, which contains all Discord events defined by Discord.NET, however they only fire for the specific Discord server that the GuildHandler is tied to. Each individual server that the bot is connected to has its own instance of a GuildHandler, as well as any plugins that may be enabled on the server. Through the GuildHandler you have access to other tools. These tools are as following:

### PluginMessenger

Handles registering and calling of cross-plugin messages, so as to allow plugins to communicate without being strictly coupled together. Attempting to call an unregistered action/function returns displays a warning message in console and returns null/default.
 
Registering an action/function may look like this: `RegisterMessageAction ("Name", (x) => Method (x))`.
Calling a registered action/function would then look like: `SendMessage ("PluginAuthor-PluginName", "Name", value)`

Familiarity with delegates and lambda expressions in C# is recommended, but not required if you just need to do simple stuff.

As may be noticed here, calling a registered action/function requires specifiying the target plugin as well. Additionally, an alternative `RegisterMessageFunction` method may be used instead, which, unlike the previously mentioned, returns a value when the registered function is called.
 
### PluginManager

Handles the plugins enabled on the server, and contains methods for adding and removing plugins from the list of active plugins. There is little reason to worry about this, unless you wish to create your own plugin management plugin to replace the standard one.
 
### StateManager

Keeps track of a representation of plugin state between plugin reloads. Any plugin can track changes to it between reloads using this system, and any changes detected will be accessable using the `GetChanges ()` method of the StateManager. This is done by the default plugin management plugin when enabling/disabling plugins, and printed to the user.
 
In order to track a state you first need to register change headers using `SetChangeStateHeaders (identifier, additionHeader, removalHeader, mutationHeader)`. The last header is optional, and only needed if you expect *State Attributes* to change between reloads, instead of just being added/removed. These headers are used to display what kind of change has occured.

State Attributes are objects that represent a single "piece" of a state. This can for instance represent a specific command being present. A list of State Attributes represent a State. To add State Attributes, use `AddStateAttribute (identifier, name, description)`.

The `identifier` parameter is to keep track of different states for the same plugin.

The system will throw an exception if a doesn't have headers, so don't forget to add headers.

Additionally, an `AddGeneralFeaturesStateAttribute (name, description)` method is available, that adds a State Attribute to a general all-purpose State, and is meant to be used for notifying users of overall feature additions, such as automated functionality.

As an example, the included Command Root plugin keeps track of which root commands are added by new plugins. It firsts sets a header by calling `SetStateChangeHeaders ("Commands", "The following commands has been added", "The following commands has been removed")`, and calling `AddStateAttribute ("Commands", command.Name, prefix + command.Name);` (shortened). This results in this being printed out to the user when enabling a plugin that adds new commands, in this instance the included Utilities plugin

```
The following commands has been added
 > !rtd 
 > !flipcoin
 > !embolden
 > !fizzfyr13
 > !ping
```

### Persistant Storage

In order to save data between bot reboots, which can happen at any time due to updates or crashes, or some other unforseen catastrophe like the the earth going under in a beautiful cacophany of suffering, you need to use the `GetDataCache` method which returns a CachedValue where you can use `GetValue`, `SetValue` and `MutateValue` to get, set, or change the value it contains both locally and in a persistant database.

For storing configuration values, use `GetConfigCache` instead, which is completely identical except that it stores it's data elsewhere.
 
### PluginConfig

Handles plugin configuration by maintaining and exposing a list of config options, which are registered by individual plugins. Any outside functionality may access this list of config options and implement ways for users to configure plugins through it. By default, this is done by the standard Configuration plugin, however I wish to provide a web-based alternative later down the line.
 
Adding a configuration option looks something like this: `AddConfigInfo ("Name", "Description", new Action<T>(x => _config.SetValue (x))`.

Again, familiarity with delegates and lambda expressions in C# is recommended. Due to limitations of generic classes in C#, this is a bit more verbose than the previously mentioned RegisterMessageAction/Function methods.

Do note that the methods examplified here are shortcut methods from PluginBase.
