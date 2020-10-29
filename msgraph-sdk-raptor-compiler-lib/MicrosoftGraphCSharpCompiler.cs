﻿extern alias beta;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using MsGraphSDKSnippetsCompiler.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace MsGraphSDKSnippetsCompiler
{
    /// <summary>
    ///     Microsoft Graph SDK CSharp snippets compiler class
    /// </summary>
    public class MicrosoftGraphCSharpCompiler : IMicrosoftGraphSnippetsCompiler
    {
        private readonly string _markdownFileName;

        public MicrosoftGraphCSharpCompiler(string markdownFileName)
        {
            _markdownFileName = markdownFileName;
        }

        /// <summary>
        ///     Returns CompilationResultsModel which has the results status and the compilation diagnostics. 
        /// </summary>
        /// <param name="codeSnippet">The code snippet to be compiled.</param>
        /// <returns>CompilationResultsModel</returns>
        public CompilationResultsModel RunSnippet(string codeSnippet, Versions version)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(codeSnippet);

            string assemblyName = Path.GetRandomFileName();
            string commonAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            string graphAssemblyPathV1 = Path.GetDirectoryName(typeof(GraphServiceClient).Assembly.Location);
            string graphAssemblyPathBeta = Path.GetDirectoryName(typeof(beta.Microsoft.Graph.GraphServiceClient).Assembly.Location);

            List<MetadataReference> metadataReferences = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(Path.Combine(commonAssemblyPath, "System.Private.CoreLib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(commonAssemblyPath, "System.Console.dll")),
                MetadataReference.CreateFromFile(Path.Combine(commonAssemblyPath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(commonAssemblyPath, "netstandard.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(IAuthenticationProvider).Assembly.Location), "Microsoft.Graph.Core.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(AuthenticationProvider).Assembly.Location), "msgraph-sdk-raptor-compiler-lib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(Task).Assembly.Location), "System.Threading.Tasks.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(JToken).Assembly.Location), "Newtonsoft.Json.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(HttpClient).Assembly.Location), "System.Net.Http.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(Expression).Assembly.Location), "System.Linq.Expressions.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(Path).Assembly.Location), "System.IO.FileSystem.dll"))
            };

            //Use the right Microsoft Graph Version
            if(version == Versions.V1)
            {
                metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(graphAssemblyPathV1, "Microsoft.Graph.dll")));
            }
            else
            {
                metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(graphAssemblyPathBeta, "Microsoft.Graph.Beta.dll")));
            }
           
            CSharpCompilation compilation = CSharpCompilation.Create(
               assemblyName,
               syntaxTrees: new[] { syntaxTree },
               references: metadataReferences,
               options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var (emitResult, assembly) = GetEmitResult(compilation);
            CompilationResultsModel results = GetCompilationResults(emitResult);

            if (results.IsCompilationSuccessful)
            {
                try
                {
                    var clientId = string.Empty;
                    var tenantId = string.Empty;
                    var clientSecret = string.Empty;
                    dynamic instance = assembly.CreateInstance("GraphSDKTest");
                    IConfidentialClientApplication confidentialClientApp = ConfidentialClientApplicationBuilder
                        .Create(clientId)
                        .WithTenantId(tenantId)
                        .WithClientSecret(clientSecret)
                        .Build();
                    var authProvider = new ClientCredentialProvider(confidentialClientApp, "https://graph.microsoft.com/.default");
                    var task = instance.Main(authProvider) as Task;
                    task.Wait();
                    results.IsRunSuccessful = true;
                }
                catch (AggregateException ae)
                {
                    results.ExceptionMessage = ae.InnerException.Message;
                }
            }

            return results;
        }

        /// <summary>
        ///     Gets the result of the Compilation.Emit method.
        /// </summary>
        /// <param name="compilation">Immutable respresentation of a single invocation of the compiler</param>
        private (EmitResult, System.Reflection.Assembly) GetEmitResult(CSharpCompilation compilation)
        {
            System.Reflection.Assembly assembly = null;

            using MemoryStream memoryStream = new MemoryStream();
            EmitResult emitResult = compilation.Emit(memoryStream);

            if (emitResult.Success)
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                assembly = AssemblyLoadContext.Default.LoadFromStream(memoryStream);
            }
            return (emitResult, assembly);
        }

        /// <summary>
        ///     Checks whether the EmitResult is successfull and returns an instance of CompilationResultsModel. 
        /// </summary>
        /// <param name="emitResult">The result of the Compilation.Emit method.</param>
        private CompilationResultsModel GetCompilationResults(EmitResult emitResult)
        {
            CompilationResultsModel compilationResultsModel = new CompilationResultsModel();

            if (!emitResult.Success)
            {
                // We are only interested with warnings and errors hence the diagnostics filter
                IEnumerable<Microsoft.CodeAnalysis.Diagnostic> failures = emitResult.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                compilationResultsModel.IsCompilationSuccessful = false;
                compilationResultsModel.Diagnostics = failures;
                compilationResultsModel.MarkdownFileName = _markdownFileName;
            }
            else
            {
                compilationResultsModel.IsCompilationSuccessful = true;
                compilationResultsModel.Diagnostics = null;             
                compilationResultsModel.MarkdownFileName = _markdownFileName;
            }

            return compilationResultsModel;
        }
    }
}