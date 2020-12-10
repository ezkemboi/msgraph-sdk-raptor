// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using MsGraphSDKSnippetsCompiler;
using NUnit.Framework;
using System;
using System.Text.RegularExpressions;

namespace TestsCommon
{
    /// <summary>
    /// TestRunner for C# compilation tests
    /// </summary>
    public static class CSharpExecuteTestRunner
    {
        /// <summary>
        /// template to compile snippets in
        /// </summary>
        private const string SDKShellTemplate = @"using System;
using Microsoft.Graph;
using MsGraphSDKSnippetsCompiler;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

// Disambiguate colliding namespaces
using DayOfWeek = Microsoft.Graph.DayOfWeek;
using TimeOfDay = Microsoft.Graph.TimeOfDay;
using KeyValuePair = Microsoft.Graph.KeyValuePair;

public class GraphSDKTest
{
    private IAuthenticationProvider authProvider = null;

    private async void Main()
    {
        authProvider = AuthenticationProvider.GetIAuthenticationProvider();

        #region msgraphsnippets
        //insert-code-here
        #endregion
    }
}";

        /// <summary>
        /// matches csharp snippet from C# snippets markdown output
        /// </summary>
        private const string Pattern = @"```csharp(.*)```";

        /// <summary>
        /// compiled version of the C# markdown regular expression
        /// uses Singleline so that (.*) matches new line characters as well
        /// </summary>
        private static readonly Regex RegExp = new Regex(Pattern, RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// 1. Fetches snippet from docs repo
        /// 2. Asserts that there is one and only one snippet in the file
        /// 3. Wraps snippet with compilable template
        /// 4. Attempts to compile and reports errors if there is any
        /// </summary>
        /// <param name="testData">Test data containing information such as snippet file name</param>
        public static void Run(ExecuteTestData executeTestData)
        {
            if (executeTestData == null)
            {
                throw new ArgumentNullException(nameof(executeTestData));
            }

            var testData = executeTestData.languageTestData;
            var fileContent = executeTestData.fileContent;

            var match = RegExp.Match(fileContent);
            Assert.IsTrue(match.Success, "Csharp snippet file is not in expected format!");

            var codeSnippetFormatted = match.Groups[1].Value
                .Replace("\r\n", "\r\n        ")            // add indentation to match with the template
                .Replace("\r\n        \r\n", "\r\n\r\n")    // remove indentation added to empty lines
                .Replace("\t", "    ")                      // do not use tabs
                .Replace("\r\n\r\n\r\n", "\r\n\r\n");       // do not have two consecutive empty lines

            var codeToCompile = BaseTestRunner.ConcatBaseTemplateWithSnippet(codeSnippetFormatted, SDKShellTemplate);

            // Compile Code
            var microsoftGraphCSharpCompiler = new MicrosoftGraphCSharpCompiler(testData.FileName, testData.DllPath);
            var compilationResultsModel = microsoftGraphCSharpCompiler.CompileSnippet(codeToCompile, testData.Version);

            if (compilationResultsModel.IsSuccess)
            {
                Assert.Pass();
            }

            var compilationOutputMessage = new CompilationOutputMessage(compilationResultsModel, codeToCompile, testData.DocsLink, testData.KnownIssueMessage, testData.IsKnownIssue);

            Assert.Fail($"{compilationOutputMessage}");
        }
    }
}
