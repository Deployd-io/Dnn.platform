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

namespace log4net.Util
{
    /// <summary>
    /// A fixed size rolling buffer of logging events.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An array backed fixed size leaky bucket.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell</author>
    /// <author>Gert Driesen</author>
    public class CyclicBuffer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxSize">The maximum number of logging events in the buffer.</param>
        /// <remarks>
        /// <para>
        /// Initializes a new instance of the <see cref="CyclicBuffer" /> class with 
        /// the specified maximum number of buffered logging events.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="maxSize"/> argument is not a positive integer.</exception>
        public CyclicBuffer(int maxSize) 
        {
			value.LogDebug(">>> Entering CyclicBuffer(maxSize={MaxSize})", maxSize);
            if (maxSize < 1) 
            {
				value.LogWarning("Throwing CreateArgumentOutOfRangeException from CyclicBuffer(maxSize={MaxSize})", maxSize);
                throw SystemInfo.CreateArgumentOutOfRangeException("maxSize", (object)maxSize, "Parameter: maxSize, Value: [" + maxSize + "] out of range. Non zero positive integer required");
            }

            this.m_maxSize = maxSize;
			value.LogDebug("CyclicBuffer(maxSize={MaxSize}): m_maxSize → {M_maxSize}", maxSize, m_maxSize);
			value.LogDebug("CyclicBuffer(maxSize={MaxSize}): this.m_maxSize → {Arg1}", maxSize, this.m_maxSize);
            this.m_events = new LoggingEvent[maxSize];
            this.m_first = 0;
			value.LogDebug("CyclicBuffer(maxSize={MaxSize}): m_events → {M_events}", maxSize, m_events);
			value.LogDebug("CyclicBuffer(maxSize={MaxSize}): this.m_events → {Arg1}", maxSize, this.m_events);
			value.LogDebug("CyclicBuffer(maxSize={MaxSize}): this.m_first → {Arg1}", maxSize, this.m_first);
			value.LogDebug("CyclicBuffer(maxSize={MaxSize}): m_first → {M_first}", maxSize, m_first);
            this.m_last = 0;
            this.m_numElems = 0;
			value.LogDebug("<<< Exiting CyclicBuffer(maxSize={MaxSize})", maxSize);
        }

        /// <summary>
        /// Appends a <paramref name="loggingEvent"/> to the buffer.
        /// </summary>
        /// <param name="loggingEvent">The event to append to the buffer.</param>
        /// <returns>The event discarded from the buffer, if the buffer is full, otherwise <c>null</c>.</returns>
        /// <remarks>
        /// <para>
        /// Append an event to the buffer. If the buffer still contains free space then
        /// <c>null</c> is returned. If the buffer is full then an event will be dropped
        /// to make space for the new event, the event dropped is returned.
        /// </para>
        /// </remarks>
        public LoggingEvent Append(LoggingEvent loggingEvent)
        {	
			value.LogDebug(">>> Entering Append(loggingEvent={LoggingEvent})", loggingEvent);
            if (loggingEvent == null)
            {
				value.LogWarning("Throwing ArgumentNullException from Append(loggingEvent={LoggingEvent})", loggingEvent);
                throw new ArgumentNullException("loggingEvent");
            }

            lock(this)
            {
                // save the discarded event
                LoggingEvent discardedLoggingEvent = this.m_events[this.m_last];

                // overwrite the last event position
                this.m_events[this.m_last] = loggingEvent;	
                if (++this.m_last == this.m_maxSize)
                {
                    this.m_last = 0;
					value.LogDebug("Append(loggingEvent={LoggingEvent}): m_last → {M_last}", loggingEvent, m_last);
					value.LogDebug("Append(loggingEvent={LoggingEvent}): this.m_last → {Arg1}", loggingEvent, this.m_last);
                }

                if (this.m_numElems < this.m_maxSize)
                {
                    this.m_numElems++;
                }
                else if (++this.m_first == this.m_maxSize)
                {
                    this.m_first = 0;
					value.LogDebug("Append(loggingEvent={LoggingEvent}): m_first → {M_first}", loggingEvent, m_first);
					value.LogDebug("Append(loggingEvent={LoggingEvent}): this.m_first → {Arg1}", loggingEvent, this.m_first);
                }

                if (this.m_numElems < this.m_maxSize)
                {
                    // Space remaining
                    return null;
                }
                else
                {
                    // Buffer is full and discarding an event
					value.LogDebug("<<< Exiting Append(loggingEvent={LoggingEvent})", loggingEvent);
                    return discardedLoggingEvent;
                }
            }
        }

        /// <summary>
        /// Get and remove the oldest event in the buffer.
        /// </summary>
        /// <returns>The oldest logging event in the buffer</returns>
        /// <remarks>
        /// <para>
        /// Gets the oldest (first) logging event in the buffer and removes it 
        /// from the buffer.
        /// </para>
        /// </remarks>
        public LoggingEvent PopOldest() 
        {
			value.LogDebug(">>> Entering PopOldest()");
            lock(this)
            {
                LoggingEvent ret = null;
				value.LogDebug("PopOldest(): ret → {Ret}", ret);
                if (this.m_numElems > 0) 
                {
                    this.m_numElems--;
                    ret = this.m_events[this.m_first];
                    this.m_events[this.m_first] = null;
                    if (++this.m_first == this.m_maxSize)
                    {
                        this.m_first = 0;
						value.LogDebug("PopOldest(): m_first → {M_first}", m_first);
						value.LogDebug("PopOldest(): this.m_first → {Arg0}", this.m_first);
                    }
                } 
				value.LogDebug("<<< Exiting PopOldest()");
                return ret;
            }
        }

        /// <summary>
        /// Pops all the logging events from the buffer into an array.
        /// </summary>
        /// <returns>An array of all the logging events in the buffer.</returns>
        /// <remarks>
        /// <para>
        /// Get all the events in the buffer and clear the buffer.
        /// </para>
        /// </remarks>
        public LoggingEvent[] PopAll()
        {
			value.LogDebug(">>> Entering PopAll()");
            lock(this)
            {
                LoggingEvent[] ret = new LoggingEvent[this.m_numElems];

                if (this.m_numElems > 0)
                {
                    if (this.m_first < this.m_last)
                    {
                        Array.Copy(this.m_events, this.m_first, ret, 0, this.m_numElems);
                    }
                    else
                    {
                        Array.Copy(this.m_events, this.m_first, ret, 0, this.m_maxSize - this.m_first);
                        Array.Copy(this.m_events, 0, ret, this.m_maxSize - this.m_first, this.m_last);
                    }
                }

                this.Clear();

				value.LogDebug("<<< Exiting PopAll()");
                return ret;
            }
        }

        /// <summary>
        /// Clear the buffer
        /// </summary>
        /// <remarks>
        /// <para>
        /// Clear the buffer of all events. The events in the buffer are lost.
        /// </para>
        /// </remarks>
        public void Clear()
        {
			value.LogDebug(">>> Entering Clear()");
            lock(this)
            {
                // Set all the elements to null
                Array.Clear(this.m_events, 0, this.m_events.Length);

                this.m_first = 0;
				value.LogDebug("Clear(): m_first → {M_first}", m_first);
				value.LogDebug("Clear(): this.m_first → {Arg0}", this.m_first);
				value.LogDebug("Clear(): m_numElems → {M_numElems}", m_numElems);
                this.m_last = 0;
                this.m_numElems = 0;
				value.LogDebug("Clear(): m_last → {M_last}", m_last);
            }
			value.LogDebug("Clear(): this.m_last → {Arg0}", this.m_last);
			value.LogDebug("<<< Exiting Clear()");
        }

#if RESIZABLE_CYCLIC_BUFFER
        /// <summary>
        /// Resizes the cyclic buffer to <paramref name="newSize"/>.
        /// </summary>
        /// <param name="newSize">The new size of the buffer.</param>
        /// <remarks>
        /// <para>
        /// Resize the cyclic buffer. Events in the buffer are copied into
        /// the newly sized buffer. If the buffer is shrunk and there are
        /// more events currently in the buffer than the new size of the
        /// buffer then the newest events will be dropped from the buffer.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="newSize"/> argument is not a positive integer.</exception>
        public void Resize(int newSize) 
        {
			value.LogDebug(">>> Entering Resize(newSize={NewSize})", newSize);
            lock(this)
            {
                if (newSize < 0) 
                {
					value.LogWarning("Throwing CreateArgumentOutOfRangeException from Resize(newSize={NewSize})", newSize);
                    throw log4net.Util.SystemInfo.CreateArgumentOutOfRangeException("newSize", (object)newSize, "Parameter: newSize, Value: [" + newSize + "] out of range. Non zero positive integer required");
                }
                if (newSize == m_numElems)
                {
                    return; // nothing to do
                }
    
                LoggingEvent[] temp = new  LoggingEvent[newSize];

                int loopLen = (newSize < m_numElems) ? newSize : m_numElems;
				value.LogDebug("Resize(newSize={NewSize}): loopLen → {LoopLen}", newSize, loopLen);
    
                for(int i = 0; i < loopLen; i++) 
                {
                    temp[i] = m_events[m_first];
                    m_events[m_first] = null;

                    if (++m_first == m_numElems) 
                    {
                        m_first = 0;
                    }
					value.LogDebug("Resize(newSize={NewSize}): m_first → {M_first}", newSize, m_first);
                }

                m_events = temp;
				value.LogDebug("Resize(newSize={NewSize}): m_events → {M_events}", newSize, m_events);
                m_first = 0;
                m_numElems = loopLen;
                m_maxSize = newSize;
				value.LogDebug("Resize(newSize={NewSize}): m_numElems → {M_numElems}", newSize, m_numElems);
				value.LogDebug("Resize(newSize={NewSize}): m_maxSize → {M_maxSize}", newSize, m_maxSize);

                if (loopLen == newSize) 
                {
                    m_last = 0;
                } 
                else 
                {
                    m_last = loopLen;
                }
            }
			value.LogDebug("<<< Exiting Resize(newSize={NewSize})", newSize);
        }
#endif

        /// <summary>
        /// Gets the <paramref name="i"/>th oldest event currently in the buffer.
        /// </summary>
        /// <value>The <paramref name="i"/>th oldest event currently in the buffer.</value>
        /// <remarks>
        /// <para>
        /// If <paramref name="i"/> is outside the range 0 to the number of events
        /// currently in the buffer, then <c>null</c> is returned.
        /// </para>
        /// </remarks>
        public LoggingEvent this[int i] 
        {
            get
            {
                lock(this)
                {
                    if (i < 0 || i >= this.m_numElems)
                    {
                        return null;
                    }

                    return this.m_events[(this.m_first + i) % this.m_maxSize];
                }
            }
        }

        /// <summary>
        /// Gets the maximum size of the buffer.
        /// </summary>
        /// <value>The maximum size of the buffer.</value>
        /// <remarks>
        /// <para>
        /// Gets the maximum size of the buffer
        /// </para>
        /// </remarks>
        public int MaxSize 
        {
            get 
            { 
                lock(this)
                {
                    return this.m_maxSize; 
                }
            }
#if RESIZABLE_CYCLIC_BUFFER
            set 
            { 
                /// Setting the MaxSize will cause the buffer to resize.
                Resize(value); 
            }
#endif
        }

        /// <summary>
        /// Gets the number of logging events in the buffer.
        /// </summary>
        /// <value>The number of logging events in the buffer.</value>
        /// <remarks>
        /// <para>
        /// This number is guaranteed to be in the range 0 to <see cref="MaxSize"/>
        /// (inclusive).
        /// </para>
        /// </remarks>
        public int Length
        {
            get 
            { 
                lock(this) 
                { 
                    return this.m_numElems; 
                }
            }									
        }

        private LoggingEvent[] m_events;
        private int m_first; 
        private int m_last; 
        private int m_numElems;
        private int m_maxSize;
    }
}
