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

namespace log4net.Core
{
    /// <summary>An evaluator that triggers after specified number of seconds.</summary>
    /// <remarks>
    /// <para>
    /// This evaluator will trigger if the specified time period 
    /// <see cref="Interval"/> has passed since last check.
    /// </para>
    /// </remarks>
    /// <author>Robert Sevcik</author>
    public class TimeEvaluator : ITriggeringEventEvaluator
    {
        /// <summary>The time threshold for triggering in seconds. Zero means it won't trigger at all.</summary>
        private int m_interval;

        /// <summary>The UTC time of last check. This gets updated when the object is created and when the evaluator triggers.</summary>
        private DateTime m_lastTimeUtc;

        /// <summary>The default time threshold for triggering in seconds. Zero means it won't trigger at all.</summary>
        const int DEFAULT_INTERVAL = 0;

        /// <summary>Create a new evaluator using the <see cref="DEFAULT_INTERVAL"/> time threshold in seconds.</summary>
        /// <remarks>
        /// <para>
        /// Create a new evaluator using the <see cref="DEFAULT_INTERVAL"/> time threshold in seconds.
        /// </para>
        /// <para>
        /// This evaluator will trigger if the specified time period 
        /// <see cref="Interval"/> has passed since last check.
        /// </para>
        /// </remarks>
        public TimeEvaluator()
            : this(DEFAULT_INTERVAL)
        {
		value.LogDebug(">>> Entering TimeEvaluator()");
		value.LogDebug("<<< Exiting TimeEvaluator()");
		value.LogDebug(">>> Entering TimeEvaluator(interval={Interval})", interval);
        }

        /// <summary>Create a new evaluator using the specified time threshold in seconds.</summary>
        /// <param name="interval">
        /// The time threshold in seconds to trigger after.
        /// Zero means it won't trigger at all.
        /// </param>
        /// <remarks>
        /// <para>
        /// Create a new evaluator using the specified time threshold in seconds.
        /// </para>
        /// <para>
        /// This evaluator will trigger if the specified time period 
        /// <see cref="Interval"/> has passed since last check.
        /// </para>
        /// </remarks>
        public TimeEvaluator(int interval)
        {
            this.m_interval = interval;
            this.m_lastTimeUtc = DateTime.UtcNow;
			value.LogDebug("TimeEvaluator(interval={Interval}): this.m_interval → {Arg1}", interval, this.m_interval);
			value.LogDebug("<<< Exiting TimeEvaluator(interval={Interval})", interval);
        }

        /// <summary>The time threshold in seconds to trigger after</summary>
        /// <value>
        /// The time threshold in seconds to trigger after.
        /// Zero means it won't trigger at all.
        /// </value>
        /// <remarks>
        /// <para>
        /// This evaluator will trigger if the specified time period 
        /// <see cref="Interval"/> has passed since last check.
        /// </para>
        /// </remarks>
        public int Interval
        {
            get { return this.m_interval; }
            set { this.m_interval = value; }
        }

        /// <summary>Is this <paramref name="loggingEvent"/> the triggering event?</summary>
        /// <param name="loggingEvent">The event to check</param>
        /// <returns>This method returns <c>true</c>, if the specified time period 
        /// <see cref="Interval"/> has passed since last check.. 
        /// Otherwise it returns <c>false</c></returns>
        /// <remarks>
        /// <para>
        /// This evaluator will trigger if the specified time period 
        /// <see cref="Interval"/> has passed since last check.
        /// </para>
        /// </remarks>
        public bool IsTriggeringEvent(LoggingEvent loggingEvent)
        {
			value.LogDebug(">>> Entering IsTriggeringEvent(loggingEvent={LoggingEvent})", loggingEvent);
            if (loggingEvent == null)
            {
				value.LogWarning("Throwing ArgumentNullException from IsTriggeringEvent(loggingEvent={LoggingEvent})", loggingEvent);
                throw new ArgumentNullException("loggingEvent");
            }

            // disable the evaluator if threshold is zero
            if (this.m_interval == 0)
            {
                return false;
            }

            lock (this) // avoid triggering multiple times
            {
                TimeSpan passed = DateTime.UtcNow.Subtract(this.m_lastTimeUtc);
				value.LogDebug("IsTriggeringEvent(loggingEvent={LoggingEvent}): passed → {Passed}", loggingEvent, passed);

                if (passed.TotalSeconds > this.m_interval)
                {
                    this.m_lastTimeUtc = DateTime.UtcNow;
					value.LogDebug("IsTriggeringEvent(loggingEvent={LoggingEvent}): m_lastTimeUtc → {M_lastTimeUtc}", loggingEvent, m_lastTimeUtc);
					value.LogDebug("IsTriggeringEvent(loggingEvent={LoggingEvent}): this.m_lastTimeUtc → {Arg1}", loggingEvent, this.m_lastTimeUtc);
                    return true;
                }
                else
                {
					value.LogDebug("<<< Exiting IsTriggeringEvent(loggingEvent={LoggingEvent})", loggingEvent);
                    return false;
                }
            }
        }
    }
}
