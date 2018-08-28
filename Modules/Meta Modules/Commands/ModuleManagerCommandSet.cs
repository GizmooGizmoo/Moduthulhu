﻿using Discord;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Core.Configuration;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.Module.Framework;
using Lomztein.Moduthulhu.Modules.CommandRoot;
using Lomztein.Moduthulhu.Modules.Meta.Extensions;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Modules.CustomCommands.Categories;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;

namespace Lomztein.Moduthulhu.Modules.Meta.Commands
{
    public class ModuleManagerCommandSet : ModuleCommandSet<ModuleManagerModule>
    {
        public ModuleManagerCommandSet () {
            Name = "modules";
            Description = "Get information about modules.";
            Category = AdditionalCategories.Management;

            commandsInSet = new List<ICommand> () {
                new List (), new Get (), new Info (), new Reload (),
            };
        }

        public class Get : ModuleCommand<ModuleManagerModule> {

            public Get () {
                Name = "get";
                Description = "Get a module object.";
                Category = AdditionalCategories.Management;
            }

            [Overload (typeof (IModule), "Get a module from the parent manager by name and author.")]
            public Task<Result> Execute (CommandMetadata data, string name, string author) {
                IModule result = ParentModule.ParentModuleHandler.GetModule (name, author);
                return TaskResult (result, result.CompactizeName ());
            }

            [Overload (typeof (IModule), "Get a module from the parent manager by seach string.")]
            public Task<Result> Execute(CommandMetadata data, string search) {
                IModule result = ParentModule.ParentModuleHandler.FuzzySearchModule (search);
                return TaskResult (result, result.CompactizeName ());
            }

        }

        public class List : ModuleCommand<ModuleManagerModule> {

            public List () {
                Name = "list";
                Description = "Display a list of modules.";
                Category = AdditionalCategories.Management;
            }

            [Overload (typeof (Embed), "Display a list of all currently active modules.")]
            public Task<Result> Execute (CommandMetadata data) {
                return TaskResult (ParentModule.ParentModuleHandler.GetModuleListEmbed (), "");
            }

        }

        public class Info : ModuleCommand<ModuleManagerModule> {

            public Info () {
                Name = "info";
                Description = "Display information about a module.";
                Category = AdditionalCategories.Management;
            }

            [Overload (typeof (Embed), "Display information about a specific module.")]
            public Task<Result> Execute (CommandMetadata data, IModule module) {
                return TaskResult (module?.GetModuleEmbed (), "");
            }

            [Overload (typeof (Embed), "Display information about a specific module found by name.")]
            public Task<Result> Execute(CommandMetadata data, string search) {
                IModule module = ParentModule.ParentModuleHandler.FuzzySearchModule (search);
                return TaskResult (module?.GetModuleEmbed (), "");
            }
        }

        public class Reload : ModuleCommand<ModuleManagerModule> {

            public Reload () {
                Name = "reload";
                Description = "Reload modules.";
                Category = AdditionalCategories.Management;

                RequiredPermissions.Add (GuildPermission.Administrator);
            }

            [Overload (typeof (void), "Reload modules from the module folder.")]
            public Task<Result> Execute (CommandMetadata data) {
                ParentModule.ParentModuleHandler.ReloadModules ();
                return TaskResult (null, "Modules have been reloaded.");
            }

        }
    }
}