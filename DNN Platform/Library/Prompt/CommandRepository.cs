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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="CommandRepository"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public CommandRepository()
            : this(null)
        {
		Logger.LogDebug(">>> Entering CommandRepository()");
		Logger.LogDebug("<<< Exiting CommandRepository()");
		Logger.LogDebug(">>> Entering CommandRepository(serviceScopeFactory={ServiceScopeFactory})", serviceScopeFactory);
		Logger.LogDebug("<<< Exiting CommandRepository(serviceScopeFactory={ServiceScopeFactory})", serviceScopeFactory);
        }

        /// <summary>Initializes a new instance of the <see cref="CommandRepository"/> class.</summary>
        /// <param name="serviceScopeFactory">The service scope factory.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public CommandRepository(IServiceScopeFactory serviceScopeFactory)
        {
			Logger.LogDebug(">>> Entering CommandRepository(serviceScopeFactory={ServiceScopeFactory})", serviceScopeFactory);
            this.serviceScopeFactory = serviceScopeFactory ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServiceScopeFactory>();
			Logger.LogDebug("<<< Exiting CommandRepository(serviceScopeFactory={ServiceScopeFactory})", serviceScopeFactory);
			Logger.LogDebug("CommandRepository(serviceScopeFactory={ServiceScopeFactory}): serviceScopeFactory → {ServiceScopeFactory1}", serviceScopeFactory, serviceScopeFactory);
        }

        /// <summary>Initializes a new instance of the <see cref="CommandRepository"/> class.</summary>
        /// <param name="serviceScopeFactory">The service scope factory.</param>
        /// <param name="hostSettings">The host settings.</param>
        public CommandRepository(IServiceScopeFactory serviceScopeFactory, IHostSettings hostSettings)
        {
            this.serviceScopeFactory = serviceScopeFactory ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServiceScopeFactory>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("CommandRepository(serviceScopeFactory={ServiceScopeFactory},hostSettings={HostSettings}): hostSettings → {HostSettings1}", serviceScopeFactory, hostSettings, hostSettings);
			Logger.LogDebug("CommandRepository(serviceScopeFactory={ServiceScopeFactory},hostSettings={HostSettings}): serviceScopeFactory → {ServiceScopeFactory1}", serviceScopeFactory, hostSettings, serviceScopeFactory);
        }

        /// <inheritdoc />
        public IEnumerable<ICommand> GetCommands()
        {
			Logger.LogDebug(">>> Entering GetCommands()");
			Logger.LogDebug("<<< Exiting GetCommands()");
            return this.CommandList().Values;
        }

        /// <inheritdoc />
        public IConsoleCommand GetCommand(IServiceProvider serviceProvider, string commandName)
        {
			Logger.LogDebug(">>> Entering GetCommand(serviceProvider={ServiceProvider},commandName={CommandName})", serviceProvider, commandName);
            var allCommands = this.CommandList();
            if (allCommands.TryGetValue(commandName, out var command))
			Logger.LogDebug("GetCommand(serviceProvider={ServiceProvider},commandName={CommandName}): allCommands → {AllCommands}", serviceProvider, commandName, allCommands);
            {
                return (IConsoleCommand)ActivatorUtilities.CreateInstance(serviceProvider, Type.GetType(command.TypeFullName));
            }

			Logger.LogDebug("<<< Exiting GetCommand(serviceProvider={ServiceProvider},commandName={CommandName})", serviceProvider, commandName);
            return null;
        }

        /// <inheritdoc />
        public ICommandHelp GetCommandHelp(IConsoleCommand consoleCommand)
        {
			Logger.LogDebug(">>> Entering GetCommandHelp(consoleCommand={ConsoleCommand})", consoleCommand);
            var cacheKey = $"{consoleCommand.GetType().Name}-{System.Threading.Thread.CurrentThread.CurrentUICulture.Name}";
			Logger.LogDebug("GetCommandHelp(consoleCommand={ConsoleCommand}): cacheKey → {CacheKey}", consoleCommand, cacheKey);
			Logger.LogDebug("<<< Exiting GetCommandHelp(consoleCommand={ConsoleCommand})", consoleCommand);
            return DataCache.GetCachedData<ICommandHelp>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, CacheItemPriority.Low),
                _ => GetCommandHelpInternal(consoleCommand));
        }

        /// <inheritdoc />
        protected override Func<ICommandRepository> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return Globals.DependencyProvider.GetRequiredService<ICommandRepository>;
        }

        private static string LocalizeString(string key, string resourcesFile = Constants.DefaultPromptResourceFile)
        {
            var localizedText = Localization.GetString(key, resourcesFile);
            return string.IsNullOrEmpty(localizedText) ? key : localizedText;
        }

        private static string CreateCommandFromClass(string className)
        {
			Logger.LogDebug(">>> Entering CreateCommandFromClass(className={ClassName})", className);
            var camelCasedParts = SplitCamelCase(className);
			Logger.LogDebug("CreateCommandFromClass(className={ClassName}): camelCasedParts → {CamelCasedParts}", className, camelCasedParts);
			Logger.LogDebug("<<< Exiting CreateCommandFromClass(className={ClassName})", className);
            return string.Join("-", camelCasedParts.Select(x => x.ToLowerInvariant()));
        }

        private static string[] SplitCamelCase(string source)
        {
			Logger.LogDebug(">>> Entering SplitCamelCase(source={Source})", source);
			Logger.LogDebug("<<< Exiting SplitCamelCase(source={Source})", source);
            return Regex.Split(source, @"(?<!^)(?=[A-Z])");
        }

        private static CommandHelp GetCommandHelpInternal(IConsoleCommand consoleCommand)
        {
			Logger.LogDebug(">>> Entering GetCommandHelpInternal(consoleCommand={ConsoleCommand})", consoleCommand);
            var commandHelp = new CommandHelp();
            if (consoleCommand != null)
			Logger.LogDebug("GetCommandHelpInternal(consoleCommand={ConsoleCommand}): commandHelp → {CommandHelp}", consoleCommand, commandHelp);
            {
                var cmd = consoleCommand.GetType();
                var attr = cmd.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).FirstOrDefault() as ConsoleCommandAttribute ?? new ConsoleCommandAttribute(CreateCommandFromClass(cmd.Name), Constants.CommandCategoryKeys.General, $"Prompt_{cmd.Name}_Description");
				Logger.LogDebug("GetCommandHelpInternal(consoleCommand={ConsoleCommand}): cmd → {Cmd}", consoleCommand, cmd);
				Logger.LogDebug("GetCommandHelpInternal(consoleCommand={ConsoleCommand}): attr → {Attr}", consoleCommand, attr);
				Logger.LogDebug("GetCommandHelpInternal(consoleCommand={ConsoleCommand}): commandHelp.Name → {Arg1}", consoleCommand, commandHelp.Name);
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

			Logger.LogDebug("<<< Exiting GetCommandHelpInternal(consoleCommand={ConsoleCommand})", consoleCommand);
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
