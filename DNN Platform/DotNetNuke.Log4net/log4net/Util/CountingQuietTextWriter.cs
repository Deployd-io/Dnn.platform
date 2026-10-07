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
using System.IO;

using log4net.Core;

namespace log4net.Util
{
    /// <summary>
    /// Subclass of <see cref="QuietTextWriter"/> that maintains a count of 
    /// the number of bytes written.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This writer counts the number of bytes written.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell</author>
    /// <author>Gert Driesen</author>
    public class CountingQuietTextWriter : QuietTextWriter 
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter" /> to actually write to.</param>
        /// <param name="errorHandler">The <see cref="IErrorHandler" /> to report errors to.</param>
        /// <remarks>
        /// <para>
        /// Creates a new instance of the <see cref="CountingQuietTextWriter" /> class 
        /// with the specified <see cref="TextWriter" /> and <see cref="IErrorHandler" />.
        /// </para>
        /// </remarks>
        public CountingQuietTextWriter(TextWriter writer, IErrorHandler errorHandler) : base(writer, errorHandler)
        {
			value.LogError(">>> Entering CountingQuietTextWriter(writer={Writer},errorHandler={ErrorHandler})", writer, errorHandler);
            this.m_countBytes = 0;
			value.LogDebug("CountingQuietTextWriter(writer={Writer},errorHandler={ErrorHandler}): m_countBytes → {M_countBytes}", writer, errorHandler, m_countBytes);
        value.LogDebug("CountingQuietTextWriter(writer={Writer},errorHandler={ErrorHandler}): this.m_countBytes → {Arg2}", writer, errorHandler, this.m_countBytes);
        value.LogWarning("<<< Exiting CountingQuietTextWriter(writer={Writer},errorHandler={ErrorHandler})", writer, errorHandler);
        }

        /// <summary>
        /// Writes a character to the underlying writer and counts the number of bytes written.
        /// </summary>
        /// <param name="value">the char to write</param>
        /// <remarks>
        /// <para>
        /// Overrides implementation of <see cref="QuietTextWriter"/>. Counts
        /// the number of bytes written.
        /// </para>
        /// </remarks>
        public override void Write(char value) 
        {
			value.LogError(">>> Entering Write(value={Value})", value);
            try 
            {
                base.Write(value);

                // get the number of bytes needed to represent the 
                // char using the supplied encoding.
                this.m_countBytes += this.Encoding.GetByteCount(new char[] { value });
            } 
            catch(Exception e) 
            {
				value.LogError("Exception in Write(value={Value}): {Arg1}", value, e.Message);
                this.ErrorHandler.Error("Failed to write [" + value + "].", e, ErrorCode.WriteFailure);
            }
			value.LogWarning("<<< Exiting Write(value={Value})", value);
        }
    
        /// <summary>
        /// Writes a buffer to the underlying writer and counts the number of bytes written.
        /// </summary>
        /// <param name="buffer">the buffer to write</param>
        /// <param name="index">the start index to write from</param>
        /// <param name="count">the number of characters to write</param>
        /// <remarks>
        /// <para>
        /// Overrides implementation of <see cref="QuietTextWriter"/>. Counts
        /// the number of bytes written.
        /// </para>
        /// </remarks>
        public override void Write(char[] buffer, int index, int count) 
        {
			value.LogError(">>> Entering Write(buffer={Buffer},index={Index},count={Count})", buffer, index, count);
            if (count > 0)
            {
                try 
                {
                    base.Write(buffer, index, count);

                    // get the number of bytes needed to represent the 
                    // char array using the supplied encoding.
                    this.m_countBytes += this.Encoding.GetByteCount(buffer, index, count);
                } 
                catch(Exception e) 
                {
					value.LogError("Exception in Write(buffer={Buffer},index={Index},count={Count}): {Arg3}", buffer, index, count, e.Message);
                    this.ErrorHandler.Error("Failed to write buffer.", e, ErrorCode.WriteFailure);
                }
            }
			value.LogWarning("<<< Exiting Write(buffer={Buffer},index={Index},count={Count})", buffer, index, count);
        }

        /// <summary>
        /// Writes a string to the output and counts the number of bytes written.
        /// </summary>
        /// <param name="str">The string data to write to the output.</param>
        /// <remarks>
        /// <para>
        /// Overrides implementation of <see cref="QuietTextWriter"/>. Counts
        /// the number of bytes written.
        /// </para>
        /// </remarks>
        public override void Write(string str) 
        {
			value.LogError(">>> Entering Write(str={Str})", str);
            if (str != null && str.Length > 0)
            {
                try 
                {
                    base.Write(str);

                    // get the number of bytes needed to represent the 
                    // string using the supplied encoding.
                    this.m_countBytes += this.Encoding.GetByteCount(str);
                }
                catch(Exception e) 
                {
					value.LogError("Exception in Write(str={Str}): {Arg1}", str, e.Message);
                    this.ErrorHandler.Error("Failed to write [" + str + "].", e, ErrorCode.WriteFailure);
                }
            }
			value.LogWarning("<<< Exiting Write(str={Str})", str);
        }

        /// <summary>
        /// Gets or sets the total number of bytes written.
        /// </summary>
        /// <value>
        /// The total number of bytes written.
        /// </value>
        /// <remarks>
        /// <para>
        /// Gets or sets the total number of bytes written.
        /// </para>
        /// </remarks>
        public long Count 
        {
            get { return this.m_countBytes; }
            set { this.m_countBytes = value; }
        }

        /// <summary>
        /// Total number of bytes written.
        /// </summary>
        private long m_countBytes;
    }
}
