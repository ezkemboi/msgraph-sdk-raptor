using System.Collections.Generic;
using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using TestsCommon;

namespace CsharpV1ExecuteTests
{
    [TestFixture]
    public class SnippetExecuteV1Tests
    {
        /// <summary>
        /// Gets TestCaseData for V1
        /// TestCaseData contains snippet file name, version and test case name
        /// </summary>
        public static IEnumerable<TestCaseData> TestDataV1 => TestDataGenerator.GetExecuteTestData(
            new RunSettings
            {
                Version = Versions.V1,
                Language = Languages.CSharp,
                KnownFailuresRequested = false
            });

        /// <summary>
        /// Represents test runs generated from test case data
        /// </summary>
        /// <param name="fileName">snippet file name in docs repo</param>
        /// <param name="docsLink">documentation page where the snippet is shown</param>
        /// <param name="version">Docs version (e.g. V1, Beta)</param>
        [Test]
        [TestCaseSource(typeof(SnippetExecuteV1Tests), nameof(TestDataV1))]
        public void Test(ExecuteTestData testData)
        {
            CSharpTestRunner.Execute(testData);
        }
    }
}
