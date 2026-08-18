// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.UI.Validation
{
    using System.Linq;

    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Web.Validators;
    using Moq;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class ValidatorTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private static readonly ValidationResult FailedResult = new ValidationResult(new[] { new ValidationError() });

        private static readonly ValidationResult AnotherFailedResult = new ValidationResult(new[] { new ValidationError() });

        [Test]
        public void ValidateObject_Returns_Successful_Result_If_All_Validators_Return_Successful()
        {
			_logger.LogDebug(">>> Entering ValidateObject_Returns_Successful_Result_If_All_Validators_Return_Successful()");
            // Arrange
            var validator = new Validator();
			_logger.LogDebug("ValidateObject_Returns_Successful_Result_If_All_Validators_Return_Successful(): validator → {Validator}", validator);
            object target = new object();
			_logger.LogDebug("ValidateObject_Returns_Successful_Result_If_All_Validators_Return_Successful(): target → {Target}", target);
            SetupValidators(validator, target, ValidationResult.Successful, ValidationResult.Successful, ValidationResult.Successful);

            // Act
            ValidationResult result = validator.ValidateObject(target);
			_logger.LogDebug("ValidateObject_Returns_Successful_Result_If_All_Validators_Return_Successful(): result → {Result}", result);

            // Assert
            Assert.That(result.IsValid, Is.True);
			_logger.LogDebug("<<< Exiting ValidateObject_Returns_Successful_Result_If_All_Validators_Return_Successful()");
        }

        [Test]
        public void ValidateObject_Returns_Failed_Result_If_Any_Validator_Returns_Failed()
        {
			_logger.LogDebug(">>> Entering ValidateObject_Returns_Failed_Result_If_Any_Validator_Returns_Failed()");
            // Arrange
            var validator = new Validator();
			_logger.LogDebug("ValidateObject_Returns_Failed_Result_If_Any_Validator_Returns_Failed(): validator → {Validator}", validator);
            object target = new object();
			_logger.LogDebug("ValidateObject_Returns_Failed_Result_If_Any_Validator_Returns_Failed(): target → {Target}", target);
            SetupValidators(validator, target, ValidationResult.Successful, ValidationResult.Successful, FailedResult, ValidationResult.Successful);

            // Act
            ValidationResult result = validator.ValidateObject(target);
			_logger.LogDebug("ValidateObject_Returns_Failed_Result_If_Any_Validator_Returns_Failed(): result → {Result}", result);

            // Assert
            Assert.That(result.IsValid, Is.False);
			_logger.LogDebug("<<< Exiting ValidateObject_Returns_Failed_Result_If_Any_Validator_Returns_Failed()");
        }

        [Test]
        public void ValidateObject_Collects_Errors_From_All_Validators()
        {
			_logger.LogDebug(">>> Entering ValidateObject_Collects_Errors_From_All_Validators()");
            // Arrange
            var validator = new Validator();
			_logger.LogDebug("ValidateObject_Collects_Errors_From_All_Validators(): validator → {Validator}", validator);
            object target = new object();
			_logger.LogDebug("ValidateObject_Collects_Errors_From_All_Validators(): target → {Target}", target);
            SetupValidators(validator, target, ValidationResult.Successful, FailedResult, AnotherFailedResult, ValidationResult.Successful);

            // Act
            ValidationResult result = validator.ValidateObject(target);
			_logger.LogDebug("ValidateObject_Collects_Errors_From_All_Validators(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsValid, Is.False);
                Assert.That(result.Errors.Count(), Is.EqualTo(2));
            }
            EnumerableAssert.ElementsMatch(new[] { FailedResult, AnotherFailedResult }, result.Errors, (e, a) => ReferenceEquals(e.Errors.First(), a));
			_logger.LogDebug("<<< Exiting ValidateObject_Collects_Errors_From_All_Validators()");
        }

        private static void SetupValidators(Validator validator, object target, params ValidationResult[] validationResults)
        {
            validator.Validators.Clear();
            for (int i = 0; i < validationResults.Length; i++)
            {
                var mockValidator = new Mock<ObjectValidator>();
                mockValidator.Setup(v => v.ValidateObject(target)).Returns(validationResults[i]);
                validator.Validators.Add(mockValidator.Object);
            }
			_logger.LogDebug("<<< Exiting SetupValidators(validator={Validator},target={Target},validationResults={ValidationResults})", validator, target, validationResults);
        }
    }
}
