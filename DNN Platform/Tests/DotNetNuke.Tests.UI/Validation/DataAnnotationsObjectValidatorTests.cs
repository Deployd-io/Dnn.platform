// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.UI.Validation
{
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    using DotNetNuke.Web.Validators;
    using NUnit.Framework;

    using ValidationResult = DotNetNuke.Web.Validators.ValidationResult;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class DataAnnotationsObjectValidatorTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private const string ErrorMessageConst = "Yo, you have to specify 5 characters for Bar";

        public static string ErrorMessage
        {
            get
            {
                return ErrorMessageConst;
            }
        }

        [Test]
        public void ValidateObject_Returns_Successful_Result_If_All_Attributes_On_All_Properties_Validate()
        {
			_logger.LogDebug(">>> Entering ValidateObject_Returns_Successful_Result_If_All_Attributes_On_All_Properties_Validate()");
            // Arrange
            TestClass cls = new TestClass { Foo = new object(), Bar = "Baz" };
			_logger.LogDebug("ValidateObject_Returns_Successful_Result_If_All_Attributes_On_All_Properties_Validate(): cls → {Cls}", cls);
            DataAnnotationsObjectValidator validator = new DataAnnotationsObjectValidator();
			_logger.LogDebug("ValidateObject_Returns_Successful_Result_If_All_Attributes_On_All_Properties_Validate(): validator → {Validator}", validator);

            // Act
            ValidationResult result = validator.ValidateObject(cls);
			_logger.LogDebug("ValidateObject_Returns_Successful_Result_If_All_Attributes_On_All_Properties_Validate(): result → {Result}", result);

            // Assert
            Assert.That(result.IsValid, Is.True);
			_logger.LogDebug("<<< Exiting ValidateObject_Returns_Successful_Result_If_All_Attributes_On_All_Properties_Validate()");
        }

        [Test]
        public void ValidateObject_Returns_Failed_Result_If_Any_Attribute_Does_Not_Validate()
        {
			_logger.LogDebug(">>> Entering ValidateObject_Returns_Failed_Result_If_Any_Attribute_Does_Not_Validate()");
            // Arrange
            TestClass cls = new TestClass { Foo = new object(), Bar = "BarBaz" };
			_logger.LogDebug("ValidateObject_Returns_Failed_Result_If_Any_Attribute_Does_Not_Validate(): cls → {Cls}", cls);
            DataAnnotationsObjectValidator validator = new DataAnnotationsObjectValidator();
			_logger.LogDebug("ValidateObject_Returns_Failed_Result_If_Any_Attribute_Does_Not_Validate(): validator → {Validator}", validator);

            // Act
            ValidationResult result = validator.ValidateObject(cls);
			_logger.LogDebug("ValidateObject_Returns_Failed_Result_If_Any_Attribute_Does_Not_Validate(): result → {Result}", result);

            // Assert
            Assert.That(result.IsValid, Is.False);
			_logger.LogDebug("<<< Exiting ValidateObject_Returns_Failed_Result_If_Any_Attribute_Does_Not_Validate()");
        }

        [Test]
        public void ValidateObject_Collects_Error_Messages_From_Validation_Attributes()
        {
			_logger.LogDebug(">>> Entering ValidateObject_Collects_Error_Messages_From_Validation_Attributes()");
            // Arrange
            TestClass cls = new TestClass { Foo = null, Bar = "BarBaz" };
			_logger.LogDebug("ValidateObject_Collects_Error_Messages_From_Validation_Attributes(): cls → {Cls}", cls);
            DataAnnotationsObjectValidator validator = new DataAnnotationsObjectValidator();
			_logger.LogDebug("ValidateObject_Collects_Error_Messages_From_Validation_Attributes(): validator → {Validator}", validator);

            // Act
            ValidationResult result = validator.ValidateObject(cls);
			_logger.LogDebug("ValidateObject_Collects_Error_Messages_From_Validation_Attributes(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsValid, Is.False);
                Assert.That(result.Errors.Where(e => e.PropertyName == "Foo").Single().ErrorMessage, Is.EqualTo("Dude, you forgot to enter a Foo"));
                Assert.That(result.Errors.Where(e => e.PropertyName == "Bar").Single().ErrorMessage, Is.EqualTo("Yo, you have to specify 5 characters for Bar"));
            }
			_logger.LogDebug("<<< Exiting ValidateObject_Collects_Error_Messages_From_Validation_Attributes()");
        }

        [Test]
        public void ValidateObject_Collects_ValidationAttribute_Objects_From_Failed_Validation()
        {
			_logger.LogDebug(">>> Entering ValidateObject_Collects_ValidationAttribute_Objects_From_Failed_Validation()");
            // Arrange
            TestClass cls = new TestClass { Foo = null, Bar = "BarBaz" };
			_logger.LogDebug("ValidateObject_Collects_ValidationAttribute_Objects_From_Failed_Validation(): cls → {Cls}", cls);
            DataAnnotationsObjectValidator validator = new DataAnnotationsObjectValidator();
			_logger.LogDebug("ValidateObject_Collects_ValidationAttribute_Objects_From_Failed_Validation(): validator → {Validator}", validator);

            // Act
            ValidationResult result = validator.ValidateObject(cls);
			_logger.LogDebug("ValidateObject_Collects_ValidationAttribute_Objects_From_Failed_Validation(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsValid, Is.False);
                Assert.That(result.Errors.Single(e => e.PropertyName == "Foo").Validator, Is.InstanceOf<RequiredAttribute>());
                Assert.That(result.Errors.Single(e => e.PropertyName == "Bar").Validator, Is.InstanceOf<StringLengthAttribute>());
            }
			_logger.LogDebug("<<< Exiting ValidateObject_Collects_ValidationAttribute_Objects_From_Failed_Validation()");
        }

        public class TestClass
        {
            [Required(ErrorMessage = "Dude, you forgot to enter a {0}")]
            public object Foo { get; set; }

            [StringLength(5, ErrorMessageResourceName = "ErrorMessage", ErrorMessageResourceType = typeof(DataAnnotationsObjectValidatorTests))]
            public string Bar { get; set; }
        }
    }
}
