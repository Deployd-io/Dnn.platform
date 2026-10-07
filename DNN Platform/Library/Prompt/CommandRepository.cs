// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Prompt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Web.Caching;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Prompt;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The repository handles retrieving of commands from the entire DNN installation.</summary>
    public class CommandRepository : ServiceLocator<ICommandRepository, CommandRepository>, ICommandRepository
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CommandRepository));
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="CommandRepository"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public CommandRepository()
            : this(null)
        {
		Logger.LogWarning("<<< Exiting CommandRepository(serviceScopeFactory={ServiceScopeFactory},hostSettings={HostSettings})", serviceScopeFactory, hostSettings);
        }

        /// <summary>Initializes a new instance of the <see cref="CommandRepository"/> class.</summary>
        /// <param name="serviceScopeFactory">The service scope factory.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public CommandRepository(IServiceScopeFactory serviceScopeFactory)
        {
			Logger.LogError(">>> Entering CommandRepository(serviceScopeFactory={ServiceScopeFactory})", serviceScopeFactory);
            this.serviceScopeFactory = serviceScopeFactory ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServiceScopeFactory>();
			Logger.LogDebug("CommandRepository(serviceScopeFactory={ServiceScopeFactory}): serviceScopeFactory → {ServiceScopeFactory1}", serviceScopeFactory, serviceScopeFactory);
        Logger.LogWarning("<<< Exiting CommandRepository(serviceScopeFactory={ServiceScopeFactory})", serviceScopeFactory);
        }

        /// <summary>Initializes a new instance of the <see cref="CommandRepository"/> class.</summary>
        /// <param name="serviceScopeFactory">The service scope factory.</param>
        /// <param name="hostSettings">The host settings.</param>
        public CommandRepository(IServiceScopeFactory serviceScopeFactory, IHostSettings hostSettings)
        {
			Logger.LogError(">>> Entering CommandRepository(serviceScopeFactory={ServiceScopeFactory},hostSettings={HostSettings})", serviceScopeFactory, hostSettings);
			Logger.LogDebug("CommandRepository(serviceScopeFactory={ServiceScopeFactory},hostSettings={HostSettings}): hostSettings → {HostSettings1}", serviceScopeFactory, hostSettings, hostSettings);
            this.serviceScopeFactory = serviceScopeFactory ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServiceScopeFactory>();
			Logger.LogWarning("<<< Exiting CommandRepository(serviceScopeFactory={ServiceScopeFactory},hostSettings={HostSettings})", serviceScopeFactory, hostSettings);
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        }

        /// <inheritdoc />
        public IEnumerable<ICommand> GetCommands()
        {
			Logger.LogError(">>> Entering GetCommands()");
            Logger.LogWarning("<<< Exiting GetCommands()");
            return this.CommandList().Values;
        }

        /// <inheritdoc />
        public IConsoleCommand GetCommand(IServiceProvider serviceProvider, string commandName)
        {
			Logger.LogError(">>> Entering GetCommand(serviceProvider={ServiceProvider},commandName={CommandName})", serviceProvider, commandName);
            var allCommands = this.CommandList();
			Logger.LogError("GetCommand(serviceProvider={ServiceProvider},commandName={CommandName}): allCommands → {AllCommands}", serviceProvider, commandName, allCommands);
            if (allCommands.TryGetValue(commandName, out var command))
            {
                return (IConsoleCommand)ActivatorUtilities.CreateInstance(serviceProvider, Type.GetType(command.TypeFullName));
            }

			Logger.LogWarning("<<< Exiting GetCommand(serviceProvider={ServiceProvider},commandName={CommandName})", serviceProvider, commandName);
            return null;
        }

        /// <inheritdoc />
        public ICommandHelp GetCommandHelp(IConsoleCommand consoleCommand)
        {
			Logger.LogError(">>> Entering GetCommandHelp(consoleCommand={ConsoleCommand})", consoleCommand);
            var cacheKey = $"{consoleCommand.GetType().Name}-{System.Threading.Thread.CurrentThread.CurrentUICulture.Name}";
			Logger.LogError("GetCommandHelp(consoleCommand={ConsoleCommand}): cacheKey → {CacheKey}", consoleCommand, cacheKey);
			Logger.LogWarning("<<< Exiting GetCommandHelp(consoleCommand={ConsoleCommand})", consoleCommand);
            return DataCache.GetCachedData<ICommandHelp>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, CacheItemPriority.Low),
                _ => GetCommandHelpInternal(consoleCommand));
        }

        /// <inheritdoc />
        protected override Func<ICommandRepository> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return Globals.DependencyProvider.GetRequiredService<ICommandRepository>;
        }

        private static string LocalizeString(string key, string resourcesFile = Constants.DefaultPromptResourceFile)
        {
			Logger.LogError(">>> Entering LocalizeString(key={Key},resourcesFile={ResourcesFile})", key, resourcesFile);
            var localizedText = Localization.GetString(key, resourcesFile);
			Logger.LogError("LocalizeString(key={Key},resourcesFile={ResourcesFile}): localizedText → {LocalizedText}", key, resourcesFile, localizedText);
			Logger.LogWarning("<<< Exiting LocalizeString(key={Key},resourcesFile={ResourcesFile})", key, resourcesFile);
            return string.IsNullOrEmpty(localizedText) ? key : localizedText;
        }

        private static string CreateCommandFromClass(string className)
        {
			Logger.LogError(">>> Entering CreateCommandFromClass(className=<redacted>)");
            var camelCasedParts = SplitCamelCase(className);
			Logger.LogError("CreateCommandFromClass(className=<redacted>): camelCasedParts → {CamelCasedParts}", camelCasedParts);
			Logger.LogWarning("<<< Exiting CreateCommandFromClass(className=<redacted>)");
            return string.Join("-", camelCasedParts.Select(x => x.ToLowerInvariant()));
        }

        private static string[] SplitCamelCase(string source)
        {
			Logger.LogError(">>> Entering SplitCamelCase(source={Source})", source);
            Logger.LogWarning("<<< Exiting SplitCamelCase(source={Source})", source);
            return Regex.Split(source, @"(?<!^)(?=[A-Z])");
        }

        private static CommandHelp GetCommandHelpInternal(IConsoleCommand consoleCommand)
        {
			Logger.LogError(">>> Entering GetCommandHelpInternal(consoleCommand={ConsoleCommand})", consoleCommand);
            var commandHelp = new CommandHelp();
			Logger.LogError("GetCommandHelpInternal(consoleCommand={ConsoleCommand}): commandHelp → {CommandHelp}", consoleCommand, commandHelp);
            if (consoleCommand != null)
            {
                var cmd = consoleCommand.GetType();
				Logger.LogError("GetCommandHelpInternal(consoleCommand={ConsoleCommand}): cmd → {Cmd}", consoleCommand, cmd);
				Logger.LogDebug("GetCommandHelpInternal(consoleCommand={ConsoleCommand}): commandHelp.Name → {Arg1}", consoleCommand, commandHelp.Name);
                var attr = cmd.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).FirstOrDefault() as ConsoleCommandAttribute ?? new ConsoleCommandAttribute(CreateCommandFromClass(cmd.Name), Constants.CommandCategoryKeys.General, $"Prompt_{cmd.Name}_Description");
				Logger.LogError("GetCommandHelpInternal(consoleCommand={ConsoleCommand}): attr → {Attr}", consoleCommand, attr);
                commandHelp.Name = attr.Name;
                commandHelp.Description = LocalizeString(attr.DescriptionKey, consoleCommand.LocalResourceFile);
                var commandParameters = cmd.GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                    .Select(x => x.GetCustomAttributes(typeof(ConsoleCommandParameterAttribute), false).FirstOrDefault())
                    .Cast<ConsoleCommandParameterAttribute>().ToList();
                if (commandParameters.Count != 0)
                {
                    var options = commandParameters.Where(attribute => attribute != null).Select(attribute => new CommandOption
                    {
                        Name = attribute.Name,
                        Required = attribute.Required,
                        DefaultValue = attribute.DefaultValue,
                        Description =
                            LocalizeString(attribute.DescriptionKey, consoleCommand.LocalResourceFile),
                    }).ToList();
                    commandHelp.Options = options;
					Logger.LogDebug("GetCommandHelpInternal(consoleCommand={ConsoleCommand}): commandHelp.Options → {Arg1}", consoleCommand, commandHelp.Options);
                }

                commandHelp.ResultHtml = consoleCommand.ResultHtml;
				Logger.LogDebug("GetCommandHelpInternal(consoleCommand={ConsoleCommand}): commandHelp.ResultHtml → {Arg1}", consoleCommand, commandHelp.ResultHtml);
            }
            else
            {
                commandHelp.Error = LocalizeString("Prompt_CommandNotFound");
            }

			Logger.LogWarning("<<< Exiting GetCommandHelpInternal(consoleCommand={ConsoleCommand})", consoleCommand);
            return commandHelp;
        }

        private SortedDictionary<string, ICommand> GetCommandsInternal()
        {
            var commands = new SortedDictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase);
            var typeLocator = new TypeLocator();
            var allCommandTypes = typeLocator.GetAllMatchingTypes(
                t => t is { IsClass: true, IsAbstract: false, IsVisible: true, } &&
                     typeof(IConsoleCommand).IsAssignableFrom(t));

            using var serviceScope = this.serviceScopeFactory.CreateScope();
            foreach (var cmd in allCommandTypes)
            {
                var attr = cmd.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).FirstOrDefault() ?? new ConsoleCommandAttribute(CreateCommandFromClass(cmd.Name), Constants.CommandCategoryKeys.General, $"Prompt_{cmd.Name}_Description");
                var assemblyName = cmd.Assembly.GetName();
                var version = assemblyName.Version.ToString();
                var commandAttribute = (ConsoleCommandAttribute)attr;
                var key = commandAttribute.Name;

                var command = (IConsoleCommand)ActivatorUtilities.CreateInstance(serviceScope.ServiceProvider, cmd);
                var localResourceFile = command?.LocalResourceFile;

                commands.Add(key, new Command
                {
                    Category = LocalizeString(commandAttribute.CategoryKey, localResourceFile),
                    Description = LocalizeString(commandAttribute.DescriptionKey, localResourceFile),
                    Key = key,
                    Name = commandAttribute.Name,
                    Version = version,
                    TypeFullName = cmd.AssemblyQualifiedName,
                });
            }

            return commands;
        }

        private SortedDictionary<string, ICommand> CommandList()
        {
            return
                DataCache.GetCachedData<SortedDictionary<string, ICommand>>(
                    this.hostSettings,
                    new CacheItemArgs("DnnPromptCommandList", CacheItemPriority.Default),
                    _ => this.GetCommandsInternal());
        }
    }
}
