// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
// 
// Licensed to the Apache Software Foundation (ASF) under one or more
// contributor license agreements. See the NOTICE file distributed with
// this work for additional information regarding copyright ownership.
// The ASF licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with
// the License. You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

using System;

using log4net.Core;
using log4net.Appender;

namespace log4net.Util
{
    /// <summary>
    /// A straightforward implementation of the <see cref="IAppenderAttachable"/> interface.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the default implementation of the <see cref="IAppenderAttachable"/>
    /// interface. Implementors of the <see cref="IAppenderAttachable"/> interface
    /// should aggregate an instance of this type.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell</author>
    /// <author>Gert Driesen</author>
    public class AppenderAttachedImpl : IAppenderAttachable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// <para>
        /// Initializes a new instance of the <see cref="AppenderAttachedImpl"/> class.
        /// </para>
        /// </remarks>
        public AppenderAttachedImpl()
        {
		Logger.LogDebug(">>> Entering AppenderAttachedImpl()");
		Logger.LogDebug("<<< Exiting AppenderAttachedImpl()");
        }

        /// <summary>
        /// Append on on all attached appenders.
        /// </summary>
        /// <param name="loggingEvent">The event being logged.</param>
        /// <returns>The number of appenders called.</returns>
        /// <remarks>
        /// <para>
        /// Calls the <see cref="IAppender.DoAppend" /> method on all 
        /// attached appenders.
        /// </para>
        /// </remarks>
        public int AppendLoopOnAppenders(LoggingEvent loggingEvent) 
        {
			Logger.LogDebug(">>> Entering AppendLoopOnAppenders(loggingEvent={LoggingEvent})", loggingEvent);
            if (loggingEvent == null)
            {
                throw new ArgumentNullException("loggingEvent");
            }

            // m_appenderList is null when empty
            if (this.m_appenderList == null) 
            {
                return 0;
            }

            if (this.m_appenderArray == null)
            {
                this.m_appenderArray = this.m_appenderList.ToArray();
            }

            foreach(IAppender appender in this.m_appenderArray)
            {
                try
                {
                    appender.DoAppend(loggingEvent);
                }
                catch(Exception ex)
                {
                    LogLog.Error(declaringType, "Failed to append to appender [" + appender.Name + "]", ex);
                }
            }
			Logger.LogDebug("<<< Exiting AppendLoopOnAppenders(loggingEvent={LoggingEvent})", loggingEvent);
            return this.m_appenderList.Count;
        }

        /// <summary>
        /// Append on on all attached appenders.
        /// </summary>
        /// <param name="loggingEvents">The array of events being logged.</param>
        /// <returns>The number of appenders called.</returns>
        /// <remarks>
        /// <para>
        /// Calls the <see cref="IAppender.DoAppend" /> method on all 
        /// attached appenders.
        /// </para>
        /// </remarks>
        public int AppendLoopOnAppenders(LoggingEvent[] loggingEvents) 
        {
            if (loggingEvents == null)
            {
                throw new ArgumentNullException("loggingEvents");
            }
            if (loggingEvents.Length == 0)
            {
                throw new ArgumentException("loggingEvents array must not be empty", "loggingEvents");
            }
            if (loggingEvents.Length == 1)
            {
                // Fall back to single event path
                return this.AppendLoopOnAppenders(loggingEvents[0]);
            }

            // m_appenderList is null when empty
            if (this.m_appenderList == null) 
            {
                return 0;
            }

            if (this.m_appenderArray == null)
            {
                this.m_appenderArray = this.m_appenderList.ToArray();
            }

            foreach(IAppender appender in this.m_appenderArray)
            {
                try
                {
                    CallAppend(appender, loggingEvents);
                }
                catch(Exception ex)
                {
                    LogLog.Error(declaringType, "Failed to append to appender [" + appender.Name + "]", ex);
                }
            }
			Logger.LogDebug("<<< Exiting AppendLoopOnAppenders(loggingEvents={LoggingEvents})", loggingEvents);
            return this.m_appenderList.Count;
        }

        /// <summary>
        /// Calls the DoAppende method on the <see cref="IAppender"/> with 
        /// the <see cref="LoggingEvent"/> objects supplied.
        /// </summary>
        /// <param name="appender">The appender</param>
        /// <param name="loggingEvents">The events</param>
        /// <remarks>
        /// <para>
        /// If the <paramref name="appender" /> supports the <see cref="IBulkAppender"/>
        /// interface then the <paramref name="loggingEvents" /> will be passed 
        /// through using that interface. Otherwise the <see cref="LoggingEvent"/>
        /// objects in the array will be passed one at a time.
        /// </para>
        /// </remarks>
        private static void CallAppend(IAppender appender, LoggingEvent[] loggingEvents)
        {
			Logger.LogDebug(">>> Entering CallAppend(appender={Appender},loggingEvents={LoggingEvents})", appender, loggingEvents);
            IBulkAppender bulkAppender = appender as IBulkAppender;
            if (bulkAppender != null)
            {
                bulkAppender.DoAppend(loggingEvents);
            }
            else
            {
                foreach(LoggingEvent loggingEvent in loggingEvents)
                {
                    appender.DoAppend(loggingEvent);
                }
            }
			Logger.LogDebug("<<< Exiting CallAppend(appender={Appender},loggingEvents={LoggingEvents})", appender, loggingEvents);
        }

        /// <summary>
        /// Attaches an appender.
        /// </summary>
        /// <param name="newAppender">The appender to add.</param>
        /// <remarks>
        /// <para>
        /// If the appender is already in the list it won't be added again.
        /// </para>
        /// </remarks>
        public void AddAppender(IAppender newAppender) 
        {
			Logger.LogDebug(">>> Entering AddAppender(newAppender={NewAppender})", newAppender);
            // Null values for newAppender parameter are strictly forbidden.
            if (newAppender == null)
            {
                throw new ArgumentNullException("newAppender");
            }

            this.m_appenderArray = null;
			Logger.LogDebug("AddAppender(newAppender={NewAppender}): this.m_appenderArray → {Arg1}", newAppender, this.m_appenderArray);
            if (this.m_appenderList == null) 
            {
                this.m_appenderList = new AppenderCollection(1);
				Logger.LogDebug("AddAppender(newAppender={NewAppender}): m_appenderList → {M_appenderList}", newAppender, m_appenderList);
            }
            if (!this.m_appenderList.Contains(newAppender))
            {
                this.m_appenderList.Add(newAppender);
            }
			Logger.LogDebug("<<< Exiting AddAppender(newAppender={NewAppender})", newAppender);
        }

        /// <summary>
        /// Gets all attached appenders.
        /// </summary>
        /// <returns>
        /// A collection of attached appenders, or <c>null</c> if there
        /// are no attached appenders.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The read only collection of all currently attached appenders.
        /// </para>
        /// </remarks>
        public AppenderCollection Appenders 
        {
            get
            {
                if (this.m_appenderList == null)
                {
                    // We must always return a valid collection
                    return AppenderCollection.EmptyCollection;
                }
                else 
                {
                    return AppenderCollection.ReadOnly(this.m_appenderList);
                }
            }
        }

        /// <summary>
        /// Gets an attached appender with the specified name.
        /// </summary>
        /// <param name="name">The name of the appender to get.</param>
        /// <returns>
        /// The appender with the name specified, or <c>null</c> if no appender with the
        /// specified name is found.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Lookup an attached appender by name.
        /// </para>
        /// </remarks>
        public IAppender GetAppender(string name) 
        {
			Logger.LogDebug(">>> Entering GetAppender(name={Name})", name);
            if (this.m_appenderList != null && name != null)
            {
                foreach(IAppender appender in this.m_appenderList)
                {
                    if (name == appender.Name)
                    {
                        return appender;
                    }
                }
            }
			Logger.LogDebug("<<< Exiting GetAppender(name={Name})", name);
            return null;   
        }

        /// <summary>
        /// Removes all attached appenders.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Removes and closes all attached appenders
        /// </para>
        /// </remarks>
        public void RemoveAllAppenders() 
        {
			Logger.LogDebug(">>> Entering RemoveAllAppenders()");
            if (this.m_appenderList != null) 
            {
                foreach(IAppender appender in this.m_appenderList)
                {
                    try
                    {
                        appender.Close();
                    }
                    catch(Exception ex)
                    {
                        LogLog.Error(declaringType, "Failed to Close appender ["+appender.Name+"]", ex);
                    }
                }

                this.m_appenderList = null;
				Logger.LogDebug("RemoveAllAppenders(): m_appenderList → {M_appenderList}", m_appenderList);
				Logger.LogDebug("RemoveAllAppenders(): this.m_appenderList → {Arg0}", this.m_appenderList);
                this.m_appenderArray = null;
            }
			Logger.LogDebug("<<< Exiting RemoveAllAppenders()");
			Logger.LogDebug("RemoveAllAppenders(): this.m_appenderArray → {Arg0}", this.m_appenderArray);
        }

        /// <summary>
        /// Removes the specified appender from the list of attached appenders.
        /// </summary>
        /// <param name="appender">The appender to remove.</param>
        /// <returns>The appender removed from the list</returns>
        /// <remarks>
        /// <para>
        /// The appender removed is not closed.
        /// If you are discarding the appender you must call
        /// <see cref="IAppender.Close"/> on the appender removed.
        /// </para>
        /// </remarks>
        public IAppender RemoveAppender(IAppender appender) 
        {
			Logger.LogDebug(">>> Entering RemoveAppender(appender={Appender})", appender);
            if (appender != null && this.m_appenderList != null) 
            {
                this.m_appenderList.Remove(appender);
                if (this.m_appenderList.Count == 0)
                {
                    this.m_appenderList = null;
					Logger.LogDebug("RemoveAppender(appender={Appender}): m_appenderList → {M_appenderList}", appender, m_appenderList);
					Logger.LogDebug("RemoveAppender(appender={Appender}): this.m_appenderList → {Arg1}", appender, this.m_appenderList);
                }

                this.m_appenderArray = null;
				Logger.LogDebug("RemoveAppender(appender={Appender}): this.m_appenderArray → {Arg1}", appender, this.m_appenderArray);
            }
			Logger.LogDebug("<<< Exiting RemoveAppender(appender={Appender})", appender);
            return appender;
        }

        /// <summary>
        /// Removes the appender with the specified name from the list of appenders.
        /// </summary>
        /// <param name="name">The name of the appender to remove.</param>
        /// <returns>The appender removed from the list</returns>
        /// <remarks>
        /// <para>
        /// The appender removed is not closed.
        /// If you are discarding the appender you must call
        /// <see cref="IAppender.Close"/> on the appender removed.
        /// </para>
        /// </remarks>
        public IAppender RemoveAppender(string name) 
        {
			Logger.LogDebug("<<< Exiting RemoveAppender(name={Name})", name);
            return this.RemoveAppender(this.GetAppender(name));
        }

        /// <summary>
        /// List of appenders
        /// </summary>
        private AppenderCollection m_appenderList;

        /// <summary>
        /// Array of appenders, used to cache the m_appenderList
        /// </summary>
        private IAppender[] m_appenderArray;

        /// <summary>
        /// The fully qualified type of the AppenderAttachedImpl class.
        /// </summary>
        /// <remarks>
        /// Used by the internal logger to record the Type of the
        /// log message.
        /// </remarks>
        private static readonly Type declaringType = typeof(AppenderAttachedImpl);
    }
}
