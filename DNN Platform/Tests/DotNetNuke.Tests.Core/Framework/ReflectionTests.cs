namespace DotNetNuke.Tests.Core.Framework
{
    using System.Text;

    using DotNetNuke.Framework;
    using NUnit.Framework;

    public class ReflectionTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [Test]
        public void CreateInstance_WithArgs_WorksCorrectly()
        {
			Logger.LogDebug(">>> Entering CreateInstance_WithArgs_WorksCorrectly()");
            // Arrange
            var typeToCreate = typeof(StringBuilder);
            var argToPass = new object[] { "one" };
			Logger.LogDebug("CreateInstance_WithArgs_WorksCorrectly(): typeToCreate → {TypeToCreate}", typeToCreate);
			Logger.LogDebug("CreateInstance_WithArgs_WorksCorrectly(): argToPass → {ArgToPass}", argToPass);

            // Act
            var result = (StringBuilder)Reflection.CreateInstance(typeToCreate, argToPass);

			Logger.LogDebug("CreateInstance_WithArgs_WorksCorrectly(): result → {Result}", result);
            // Assert
            Assert.That(result.ToString(), Is.EqualTo("one"));
			Logger.LogDebug("<<< Exiting CreateInstance_WithArgs_WorksCorrectly()");
        }

        [Test]
        public void CreateInstance_WithoutArgs_WorksCorrectly()
        {
			Logger.LogDebug(">>> Entering CreateInstance_WithoutArgs_WorksCorrectly()");
            // Arrange
            var typeToCreate = typeof(StringBuilder);

			Logger.LogDebug("CreateInstance_WithoutArgs_WorksCorrectly(): typeToCreate → {TypeToCreate}", typeToCreate);
            // Act
            var result = (StringBuilder)Reflection.CreateInstance(typeToCreate);

			Logger.LogDebug("CreateInstance_WithoutArgs_WorksCorrectly(): result → {Result}", result);
            // Assert
            Assert.That(result.ToString(), Is.EqualTo(string.Empty));
			Logger.LogDebug("<<< Exiting CreateInstance_WithoutArgs_WorksCorrectly()");
        }
    }
}
