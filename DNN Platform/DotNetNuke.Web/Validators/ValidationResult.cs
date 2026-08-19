// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Validators
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;

    /// <summary>A validation result.</summary>
    public class ValidationResult
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ValidationResult));
        /// <summary>Initializes a new instance of the <see cref="ValidationResult"/> class without any errors.</summary>
        public ValidationResult()
        {
			Logger.LogDebug(">>> Entering ValidationResult()");
            this.Errors = [];
			Logger.LogDebug("ValidationResult(): this.Errors → {Arg0}", this.Errors);
			Logger.LogDebug("ValidationResult(errors={Errors}): this.Errors → {Arg1}", errors, this.Errors);
			Logger.LogDebug("<<< Exiting ValidationResult()");
        }

        /// <summary>Initializes a new instance of the <see cref="ValidationResult"/> class with errors.</summary>
		Logger.LogDebug("<<< Exiting ValidationResult()");
        /// <param name="errors">The errors.</param>
        public ValidationResult(IEnumerable<ValidationError> errors)
        {
			Logger.LogDebug(">>> Entering ValidationResult(errors={Errors})", errors);
            Requires.NotNull("errors", errors);
            this.Errors = errors;
        }

        /// <summary>Gets a new successful instance.</summary>
        public static ValidationResult Successful => new();

        /// <summary>Gets the validation errors.</summary>
        public IEnumerable<ValidationError> Errors { get; }

        /// <summary>Gets a value indicating whether this result is valid.</summary>
        public bool IsValid => !this.Errors.Any();

        /// <summary>Combines this result with <paramref name="other"/>.</summary>
        /// <param name="other">The other validation result.</param>
        /// <returns>A new <see cref="ValidationResult"/> with the errors combined.</returns>
        public ValidationResult CombineWith(ValidationResult other)
        {
			Logger.LogDebug(">>> Entering CombineWith(other={Other})", other);
            Requires.NotNull("other", other);

            // Just concatenate the errors collection
			Logger.LogDebug("<<< Exiting CombineWith(other={Other})", other);
            return new ValidationResult(this.Errors.Concat(other.Errors));
        }
    }
}
