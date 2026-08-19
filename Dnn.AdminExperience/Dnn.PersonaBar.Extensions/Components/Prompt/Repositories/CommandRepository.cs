// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Prompt.Components.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Web.Caching;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Prompt.Components.Models;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc cref="DotNetNuke.Prompt.CommandRepository"/>
    [DnnDeprecated(9, 7, 0, "Moved to DotNetNuke.Prompt in the core library project.")]
    public partial class CommandRepository : ServiceLocator<ICommandRepository, CommandRepository>, ICommandRepository
    {
        private readonly IServiceScopeFactory serviceScopeFactory;

        public CommandRepository()
            : this(null)
        {
		eventLogger.LogDebug(">>> Entering CommandRepository()");
		eventLogger.LogDebug("<<< Exiting CommandRepository()");
		eventLogger.LogDebug(">>> Entering CommandRepository(serviceScopeFactory={ServiceScopeFactory})", serviceScopeFactory);
		eventLogger.LogDebug("CommandRepository(serviceScopeFactory={ServiceScopeFactory}): serviceScopeFactory → {ServiceScopeFactory1}", serviceScopeFactory, serviceScopeFactory);
        }

        public CommandRepository(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServiceScopeFactory>();
			eventLogger.LogDebug("<<< Exiting CommandRepository(serviceScopeFactory={ServiceScopeFactory})", serviceScopeFactory);
        }

        /// <inheritdoc />
        public SortedDictionary<string, Command> GetCommands()
        {
            return
                DataCache.GetCachedData<SortedDictionary<string, Command>>(
                    new CacheItemArgs("DnnPromptCommands", CacheItemPriority.Default),
                    c => this.GetCommandsInternal());
        }

        /// <inheritdoc />
        public CommandHelp GetCommandHelp(string[] args, IConsoleCommand consoleCommand)
        {
			eventLogger.LogDebug(">>> Entering GetCommandHelp(args={Args},consoleCommand={ConsoleCommand})", args, consoleCommand);
            var cacheKey = (string.Join("_", args) + "_" + PortalController.Instance.GetCurrentSettings()?.DefaultLanguage).Replace("-", "_");
			eventLogger.LogDebug("GetCommandHelp(args={Args},consoleCommand={ConsoleCommand}): cacheKey → {CacheKey}", args, consoleCommand, cacheKey);
			eventLogger.LogDebug("<<< Exiting GetCommandHelp(args={Args},consoleCommand={ConsoleCommand})", args, consoleCommand);
            return DataCache.GetCachedData<CommandHelp>(
                new CacheItemArgs(cacheKey, CacheItemPriority.Low),
                c => this.GetCommandHelpInternal(consoleCommand));
        }

        /// <inheritdoc />
        protected override Func<ICommandRepository> GetFactory()
        {
			eventLogger.LogDebug(">>> Entering GetFactory()");
			eventLogger.LogDebug("<<< Exiting GetFactory()");
            return Globals.DependencyProvider.GetRequiredService<ICommandRepository>;
        }

        private static string LocalizeString(string key, string resourcesFile = Constants.LocalResourcesFile)
        {
			eventLogger.LogDebug(">>> Entering LocalizeString(key={Key},resourcesFile={ResourcesFile})", key, resourcesFile);
            var localizedText = Localization.GetString(key, resourcesFile);
			eventLogger.LogDebug("LocalizeString(key={Key},resourcesFile={ResourcesFile}): localizedText → {LocalizedText}", key, resourcesFile, localizedText);
			eventLogger.LogDebug("<<< Exiting LocalizeString(key={Key},resourcesFile={ResourcesFile})", key, resourcesFile);
            return string.IsNullOrEmpty(localizedText) ? key : localizedText;
        }

        private static string CreateCommandFromClass(string className)
        {
			eventLogger.LogDebug(">>> Entering CreateCommandFromClass(className=<redacted>)");
            var camelCasedParts = SplitCamelCase(className);
			eventLogger.LogDebug("CreateCommandFromClass(className=<redacted>): camelCasedParts → {CamelCasedParts}", camelCasedParts);
			eventLogger.LogDebug("<<< Exiting CreateCommandFromClass(className=<redacted>)");
            return string.Join("-", camelCasedParts.Select(x => x.ToLowerInvariant()));
        }

        private static string[] SplitCamelCase(string source)
        {
			eventLogger.LogDebug(">>> Entering SplitCamelCase(source={Source})", source);
			eventLogger.LogDebug("<<< Exiting SplitCamelCase(source={Source})", source);
            return Regex.Split(source, @"(?<!^)(?=[A-Z])");
        }

        private SortedDictionary<string, Command> GetCommandsInternal()
        {
            var commands = new SortedDictionary<string, Command>(StringComparer.OrdinalIgnoreCase);
            var typeLocator = new TypeLocator();
            var allCommandTypes = typeLocator.GetAllMatchingTypes(
                t => t is { IsClass: true, IsAbstract: false, IsVisible: true, } &&
                     typeof(IConsoleCommand).IsAssignableFrom(t));

            using var serviceScope = this.serviceScopeFactory.CreateScope();
            foreach (var commandType in allCommandTypes)
            {
                var attr = commandType.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).FirstOrDefault() ?? new ConsoleCommandAttribute(CreateCommandFromClass(commandType.Name), Constants.GeneralCategory, $"Prompt_{commandType.Name}_Description");
                var assemblyName = commandType.Assembly.GetName();
                var version = assemblyName.Version.ToString();
                var commandAttribute = (ConsoleCommandAttribute)attr;
                var key = commandAttribute.Name;

                var command = (IConsoleCommand)ActivatorUtilities.GetServiceOrCreateInstance(serviceScope.ServiceProvider, commandType);
                var localResourceFile = command?.LocalResourceFile;
                commands.Add(key, new Command
                {
                    Category = LocalizeString(commandAttribute.Category, localResourceFile),
                    Description = LocalizeString(commandAttribute.Description, localResourceFile),
                    Key = key,
                    Name = commandAttribute.Name,
                    Version = version,
                    CommandType = commandType,
                });
            }

            return commands;
        }

        private CommandHelp GetCommandHelpInternal(IConsoleCommand consoleCommand)
        {
			eventLogger.LogDebug(">>> Entering GetCommandHelpInternal(consoleCommand={ConsoleCommand})", consoleCommand);
            var commandHelp = new CommandHelp();
            if (consoleCommand != null)
            {
                var commandType = consoleCommand.GetType();
				eventLogger.LogDebug("GetCommandHelpInternal(consoleCommand={ConsoleCommand}): commandType → {CommandType}", consoleCommand, commandType);
				eventLogger.LogDebug("GetCommandHelpInternal(consoleCommand={ConsoleCommand}): commandHelp.Name → {Arg1}", consoleCommand, commandHelp.Name);
                var attr = commandType.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).FirstOrDefault() as ConsoleCommandAttribute ?? new ConsoleCommandAttribute(CreateCommandFromClass(commandType.Name), Constants.GeneralCategory, $"Prompt_{commandType.Name}_Description");
				eventLogger.LogDebug("GetCommandHelpInternal(consoleCommand={ConsoleCommand}): attr → {Attr}", consoleCommand, attr);
                commandHelp.Name = attr.Name;
                commandHelp.Description = LocalizeString(attr.Description, consoleCommand.LocalResourceFile);
                var flagAttributes = commandType.GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                    .Select(x => x.GetCustomAttributes(typeof(FlagParameterAttribute), false).FirstOrDefault())
                    .Cast<FlagParameterAttribute>().ToList();
                if (flagAttributes.Count != 0)
                {
                    var options = flagAttributes.Where(attribute => attribute != null).Select(attribute => new CommandOption
                    {
                        Flag = attribute.Flag,
                        Type = attribute.Type,
                        Required = attribute.Required,
                        DefaultValue = attribute.DefaultValue,
                        Description =
                               LocalizeString(attribute.Description, consoleCommand.LocalResourceFile),
                    }).ToList();
                    commandHelp.Options = options;
					eventLogger.LogDebug("GetCommandHelpInternal(consoleCommand={ConsoleCommand}): commandHelp.Options → {Arg1}", consoleCommand, commandHelp.Options);
                }

                commandHelp.ResultHtml = consoleCommand.ResultHtml;
				eventLogger.LogDebug("GetCommandHelpInternal(consoleCommand={ConsoleCommand}): commandHelp.ResultHtml → {Arg1}", consoleCommand, commandHelp.ResultHtml);
            }
            else
            {
                commandHelp.Error = LocalizeString("Prompt_CommandNotFound");
            }

			eventLogger.LogDebug("<<< Exiting GetCommandHelpInternal(consoleCommand={ConsoleCommand})", consoleCommand);
            return commandHelp;
        }
    }
}
