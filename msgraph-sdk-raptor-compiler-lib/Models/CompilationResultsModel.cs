using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MsGraphSDKSnippetsCompiler.Models
{
    public class CompilationResultsModel
    {
        public bool IsCompilationSuccessful { get; set; }
        public IEnumerable<Diagnostic> Diagnostics { get; set; }
        public string MarkdownFileName { get; set; }
        public string ExceptionMessage { get; set; }
        public bool IsRunSuccessful { get; set; } = false;
    }
}
