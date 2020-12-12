using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MsGraphSDKSnippetsCompiler.Models
{
    public record CompilationResultsModel(bool Success, IEnumerable<Diagnostic> Diagnostics, string MarkdownFileName);

    public record ExecutionResultsModel(CompilationResultsModel CompilationResult, bool Success, string ExceptionMessage = null);
}
