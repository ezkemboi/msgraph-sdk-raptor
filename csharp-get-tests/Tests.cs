using System.Collections.Generic;
using NUnit.Framework;
using TestsCommon;

namespace CSharpGetTests
{
    public class Tests
    {
        public static IEnumerable<TestCaseData> TestDataV1 => TestDataGenerator.GetTestCaseData(
            new RunSettings
            {
                Version = Versions.V1,
                Language = Languages.CSharp,
                KnownFailuresRequested = true
            });

        /// <summary>
        /// Represents test runs generated from test case data
        /// </summary>
        /// <param name="fileName">snippet file name in docs repo</param>
        /// <param name="docsLink">documentation page where the snippet is shown</param>
        /// <param name="version">Docs version (e.g. V1, Beta)</param>
        [Test]
        [TestCaseSource(typeof(KnownFailuresV1), nameof(TestDataV1))]
        public void Test(LanguageTestData testData)
        {
            CSharpTestRunner.Run(testData);
        }
        [Test]
        public void Test()
        {
            Assert.Pass();
        }
    }
}
