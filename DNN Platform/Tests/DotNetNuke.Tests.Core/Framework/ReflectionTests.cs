namespace DotNetNuke.Tests.Core.Framework
{
    using System.Text;

    using DotNetNuke.Framework;
    using NUnit.Framework;

    public class ReflectionTests
    {
        [Test]
        public void CreateInstance_WithArgs_WorksCorrectly()
        {
			mockEventLogger.LogDebug(">>> Entering CreateInstance_WithArgs_WorksCorrectly()");
            // Arrange
            var typeToCreate = typeof(StringBuilder);
			mockEventLogger.LogDebug("CreateInstance_WithArgs_WorksCorrectly(): typeToCreate → {TypeToCreate}", typeToCreate);
            var argToPass = new object[] { "one" };
			mockEventLogger.LogDebug("CreateInstance_WithArgs_WorksCorrectly(): argToPass → {ArgToPass}", argToPass);

            // Act
            var result = (StringBuilder)Reflection.CreateInstance(typeToCreate, argToPass);
			mockEventLogger.LogDebug("CreateInstance_WithArgs_WorksCorrectly(): result → {Result}", result);

            // Assert
            Assert.That(result.ToString(), Is.EqualTo("one"));
			mockEventLogger.LogDebug("<<< Exiting CreateInstance_WithArgs_WorksCorrectly()");
        }

        [Test]
        public void CreateInstance_WithoutArgs_WorksCorrectly()
        {
			mockEventLogger.LogDebug(">>> Entering CreateInstance_WithoutArgs_WorksCorrectly()");
            // Arrange
            var typeToCreate = typeof(StringBuilder);
			mockEventLogger.LogDebug("CreateInstance_WithoutArgs_WorksCorrectly(): typeToCreate → {TypeToCreate}", typeToCreate);

            // Act
            var result = (StringBuilder)Reflection.CreateInstance(typeToCreate);
			mockEventLogger.LogDebug("CreateInstance_WithoutArgs_WorksCorrectly(): result → {Result}", result);

            // Assert
            Assert.That(result.ToString(), Is.EqualTo(string.Empty));
			mockEventLogger.LogDebug("<<< Exiting CreateInstance_WithoutArgs_WorksCorrectly()");
        }
    }
}
