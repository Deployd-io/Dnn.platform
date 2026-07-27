// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.EventQueue
{
    using System;
    using System.Data;
    using System.Globalization;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.EventQueue.Config;
    using DotNetNuke.Services.Log.EventLog;
    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;
    using Reflection = DotNetNuke.Framework.Reflection;

    /// <summary>EventQueueController provides business layer of event queue.</summary>
    /// <remarks>
    /// Sometimes when your module running in DotNetNuke,and contains some operations didn't want to execute immediately.
    /// e.g: after your module installed into system, and some component you want to registered when the application restart,
    /// you can send an 'Application_Start' message, so your specific operation will be executed when application has been restart.
    /// </remarks>
    /// <example>
    /// <code lang="C#">
    /// var oAppStartMessage = new EventMessage
    /// {
    ///     Sender = sender,
    ///     Priority = MessagePriority.High,
    ///     ExpirationDate = DateTime.Now.AddYears(-1),
    ///     SentDate = DateTime.Now,
    ///     Body = "",
    ///     ProcessorType = "DotNetNuke.Entities.Modules.EventMessageProcessor, DotNetNuke",
    ///     ProcessorCommand = "UpdateSupportedFeatures"
    /// };
    /// oAppStartMessage.Attributes.Add("BusinessControllerClass", desktopModuleInfo.BusinessControllerClass);
    /// oAppStartMessage.Attributes.Add("DesktopModuleId", desktopModuleInfo.DesktopModuleID.ToString());
    /// EventQueueController.SendMessage(oAppStartMessage, "Application_Start");
    /// if ((forceAppRestart))
    /// {
    ///     Config.Touch();
    /// }
    /// </code>
    /// </example>
    public partial class EventQueueController
    {
        /// <summary>Gets the messages.</summary>
        /// <param name="eventName">Name of the event.</param>
        /// <returns>event message collection.</returns>
        public static EventMessageCollection GetMessages(string eventName)
        {
			eventLogger.LogDebug(">>> Entering GetMessages(eventName={EventName})", eventName);
			eventLogger.LogDebug("<<< Exiting GetMessages(eventName={EventName})", eventName);
            return FillMessageCollection(DataProvider.Instance().GetEventMessages(eventName));
        }

        /// <summary>Gets the messages.</summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="subscriberId">The subscriber ID.</param>
        /// <returns>An <see cref="EventMessageCollection"/>.</returns>
        public static EventMessageCollection GetMessages(string eventName, string subscriberId)
        {
			eventLogger.LogDebug("<<< Exiting GetMessages(eventName={EventName},subscriberId={SubscriberId})", eventName, subscriberId);
            return FillMessageCollection(DataProvider.Instance().GetEventMessagesBySubscriber(eventName, subscriberId));
        }

        /// <summary>Processes the messages.</summary>
        /// <param name="eventName">Name of the event.</param>
        /// <returns><see langword="true"/> if any message is successfully sent, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(10, 0, 0, "Please use overload with IServiceProvider")]
        public static partial bool ProcessMessages(string eventName)
        {
			eventLogger.LogDebug(">>> Entering ProcessMessages(eventName={EventName})", eventName);
            using var scope = Globals.GetOrCreateServiceScope();
			eventLogger.LogDebug("ProcessMessages(eventName={EventName}): scope → {Scope}", eventName, scope);
			eventLogger.LogDebug("<<< Exiting ProcessMessages(eventName={EventName})", eventName);
            return ProcessMessages(scope.ServiceProvider, eventName);
        }

        /// <summary>Processes the messages.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="eventName">Name of the event.</param>
        /// <returns><see langword="true"/> if any message is successfully sent, otherwise <see langword="false"/>.</returns>
        public static bool ProcessMessages(IServiceProvider serviceProvider, string eventName)
        {
			eventLogger.LogDebug("<<< Exiting ProcessMessages(serviceProvider={ServiceProvider},eventName={EventName})", serviceProvider, eventName);
            return ProcessMessages(serviceProvider, GetMessages(eventName));
        }

        /// <summary>Processes the messages.</summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="subscriberId">The subscriber ID.</param>
        /// <returns><see langword="true"/> if any message is successfully sent, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(10, 0, 0, "Please use overload with IServiceProvider")]
        public static partial bool ProcessMessages(string eventName, string subscriberId)
        {
			eventLogger.LogDebug("<<< Exiting ProcessMessages(eventName={EventName},subscriberId={SubscriberId})", eventName, subscriberId);
            return ProcessMessages(GetMessages(eventName, subscriberId));
        }

        /// <summary>Processes the messages.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="subscriberId">The subscriber ID.</param>
        /// <returns><see langword="true"/> if any message is successfully sent, otherwise <see langword="false"/>.</returns>
        public static bool ProcessMessages(IServiceProvider serviceProvider, string eventName, string subscriberId)
        {
			eventLogger.LogDebug("<<< Exiting ProcessMessages(serviceProvider={ServiceProvider},eventName={EventName},subscriberId={SubscriberId})", serviceProvider, eventName, subscriberId);
            return ProcessMessages(serviceProvider, GetMessages(eventName, subscriberId));
        }

        /// <summary>Processes the messages.</summary>
        /// <param name="eventMessages">The event messages.</param>
        /// <returns><see langword="true"/> if any message is successfully sent, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(10, 0, 0, "Please use overload with IServiceProvider")]
        public static partial bool ProcessMessages(EventMessageCollection eventMessages)
        {
            using var scope = Globals.GetOrCreateServiceScope();
			eventLogger.LogDebug("ProcessMessages(eventMessages={EventMessages}): scope → {Scope}", eventMessages, scope);
			eventLogger.LogDebug("<<< Exiting ProcessMessages(eventMessages={EventMessages})", eventMessages);
            return ProcessMessages(scope.ServiceProvider, eventMessages);
        }

        /// <summary>Processes the messages.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="eventMessages">The event messages.</param>
        /// <returns><see langword="true"/> if any message is successfully sent, otherwise <see langword="false"/>.</returns>
        public static bool ProcessMessages(IServiceProvider serviceProvider, EventMessageCollection eventMessages)
        {
            bool success = Null.NullBoolean;
            for (int messageNo = 0; messageNo <= eventMessages.Count - 1; messageNo++)
            {
                var message = eventMessages[messageNo];
                try
                {
                    var messageProcessor = (EventMessageProcessorBase)Reflection.CreateObject(serviceProvider, message.ProcessorType, message.ProcessorType);
                    if (!messageProcessor.ProcessMessage(message))
                    {
                        throw new EventMessageException($"Event message of type {message.ProcessorType} returned false");
                    }

                    // Set Message complete so it is not run a second time
                    DataProvider.Instance().SetEventMessageComplete(message.EventMessageID);

                    success = true;
                }
                catch
                {
                    // log if message could not be processed
                    var eventLogger = serviceProvider.GetRequiredService<IEventLogger>();
                    var log = new LogInfo { LogTypeKey = nameof(EventLogType.HOST_ALERT) };
                    log.AddProperty("EventQueue.ProcessMessage", "Message Processing Failed");
                    log.AddProperty("ProcessorType", message.ProcessorType);
                    log.AddProperty("Body", message.Body);
                    log.AddProperty("Sender", message.Sender);
                    foreach (string key in message.Attributes.Keys)
                    {
                        log.AddProperty(key, message.Attributes[key]);
                    }

                    if (!string.IsNullOrEmpty(message.ExceptionMessage))
                    {
                        log.AddProperty("ExceptionMessage", message.ExceptionMessage);
                    }

                    eventLogger.AddLog(log);
                    if (message.ExpirationDate < DateTime.Now)
                    {
                        // Set Message complete so it is not run a second time
                        DataProvider.Instance().SetEventMessageComplete(message.EventMessageID);
                    }
                }
            }

			eventLogger.LogDebug("<<< Exiting ProcessMessages(serviceProvider={ServiceProvider},eventMessages={EventMessages})", serviceProvider, eventMessages);
            return success;
        }

        /// <summary>Sends the message.</summary>
        /// <param name="message">The message.</param>
        /// <param name="eventName">Name of the event.</param>
        /// <returns><see langword="true"/> if the message was successfully sent to all subscribers, otherwise <see langword="false"/>.</returns>
        public static bool SendMessage(EventMessage message, string eventName)
        {
            // set the sent date if it wasn't set by the sender
            if (message.SentDate != null)
            {
                message.SentDate = DateTime.Now;
            }

            // Get the subscribers to this event
            string[] subscribers = GetSubscribers(eventName);

            // send a message for each subscriber of the specified event
            int intMessageID = Null.NullInteger;
            bool success = true;
            try
            {
                for (int indx = 0; indx <= subscribers.Length - 1; indx++)
                {
                    intMessageID = DataProvider.Instance().AddEventMessage(
                        eventName,
                        (int)message.Priority,
                        message.ProcessorType,
                        message.ProcessorCommand,
                        message.Body,
                        message.Sender,
                        subscribers[indx],
                        message.AuthorizedRoles,
                        message.ExceptionMessage,
                        message.SentDate,
                        message.ExpirationDate,
                        message.SerializeAttributes());
                }
            }
            catch (Exception ex)
            {
                Exceptions.Exceptions.LogException(ex);
                success = Null.NullBoolean;
            }

            return success;
        }

        private static EventMessage FillMessage(IDataReader dr, bool checkForOpenDataReader)
        {
			eventLogger.LogDebug(">>> Entering FillMessage(dr={Dr},checkForOpenDataReader={CheckForOpenDataReader})", dr, checkForOpenDataReader);
            EventMessage message;

            // read datareader
            bool canContinue = true;
            if (checkForOpenDataReader)
			eventLogger.LogDebug("FillMessage(dr={Dr},checkForOpenDataReader={CheckForOpenDataReader}): canContinue → {CanContinue}", dr, checkForOpenDataReader, canContinue);
            {
                canContinue = false;
                if (dr.Read())
                {
                    canContinue = true;
                }
            }

            if (canContinue)
            {
                message = new EventMessage();
				eventLogger.LogDebug("FillMessage(dr={Dr},checkForOpenDataReader={CheckForOpenDataReader}): message → {Message}", dr, checkForOpenDataReader, message);
                message.EventMessageID = Convert.ToInt32(Null.SetNull(dr["EventMessageID"], message.EventMessageID), CultureInfo.InvariantCulture);
                message.Priority = (MessagePriority)Enum.Parse(typeof(MessagePriority), Convert.ToString(Null.SetNull(dr["Priority"], message.Priority), CultureInfo.InvariantCulture));
                message.ProcessorType = Convert.ToString(Null.SetNull(dr["ProcessorType"], message.ProcessorType), CultureInfo.InvariantCulture);
                message.ProcessorCommand = Convert.ToString(Null.SetNull(dr["ProcessorCommand"], message.ProcessorCommand), CultureInfo.InvariantCulture);
                message.Body = Convert.ToString(Null.SetNull(dr["Body"], message.Body), CultureInfo.InvariantCulture);
                message.Sender = Convert.ToString(Null.SetNull(dr["Sender"], message.Sender), CultureInfo.InvariantCulture);
                message.Subscribers = Convert.ToString(Null.SetNull(dr["Subscriber"], message.Subscribers), CultureInfo.InvariantCulture);
                message.AuthorizedRoles = Convert.ToString(Null.SetNull(dr["AuthorizedRoles"], message.AuthorizedRoles), CultureInfo.InvariantCulture);
                message.ExceptionMessage = Convert.ToString(Null.SetNull(dr["ExceptionMessage"], message.ExceptionMessage), CultureInfo.InvariantCulture);
                message.SentDate = Convert.ToDateTime(Null.SetNull(dr["SentDate"], message.SentDate), CultureInfo.InvariantCulture);
                message.ExpirationDate = Convert.ToDateTime(Null.SetNull(dr["ExpirationDate"], message.ExpirationDate), CultureInfo.InvariantCulture);

                // Deserialize Attributes
                string xmlAttributes = Null.NullString;
                xmlAttributes = Convert.ToString(Null.SetNull(dr["Attributes"], xmlAttributes), CultureInfo.InvariantCulture);
				eventLogger.LogDebug("FillMessage(dr={Dr},checkForOpenDataReader={CheckForOpenDataReader}): xmlAttributes → {XmlAttributes}", dr, checkForOpenDataReader, xmlAttributes);
                message.DeserializeAttributes(xmlAttributes);
            }
            else
            {
                message = null;
            }

			eventLogger.LogDebug("<<< Exiting FillMessage(dr={Dr},checkForOpenDataReader={CheckForOpenDataReader})", dr, checkForOpenDataReader);
            return message;
        }

        private static EventMessageCollection FillMessageCollection(IDataReader dr)
        {
			eventLogger.LogDebug(">>> Entering FillMessageCollection(dr={Dr})", dr);
            var arr = new EventMessageCollection();
            try
			eventLogger.LogDebug("FillMessageCollection(dr={Dr}): arr → {Arr}", dr, arr);
            {
                EventMessage obj;
                while (dr.Read())
                {
                    // fill business object
                    obj = FillMessage(dr, false);
					eventLogger.LogDebug("FillMessageCollection(dr={Dr}): obj → {Obj}", dr, obj);

                    // add to collection
                    arr.Add(obj);
                }
            }
            catch (Exception exc)
            {
				eventLogger.LogError("Exception in FillMessageCollection(dr={Dr}): {Arg1}", dr, ex.Message);
                Exceptions.Exceptions.LogException(exc);
            }
            finally
            {
                // close datareader
                CBO.CloseDataReader(dr, true);
            }

			eventLogger.LogDebug("<<< Exiting FillMessageCollection(dr={Dr})", dr);
            return arr;
        }

        private static string[] GetSubscribers(string eventName)
        {
			eventLogger.LogDebug(">>> Entering GetSubscribers(eventName={EventName})", eventName);
            // Get the subscribers to this event
            string[] subscribers;
            if (EventQueueConfiguration.GetConfig().PublishedEvents.TryGetValue(eventName, out var publishedEvent))
            {
                subscribers = publishedEvent.Subscribers.Split(";".ToCharArray());
            }
			eventLogger.LogDebug("GetSubscribers(eventName={EventName}): subscribers → {Subscribers}", eventName, subscribers);
            else
            {
                subscribers = [];
            }

			eventLogger.LogDebug("<<< Exiting GetSubscribers(eventName={EventName})", eventName);
            return subscribers;
        }
    }
}
