using MsGraphSDKSnippetsCompiler.Models;

namespace MsGraphSDKSnippetsCompiler
{
    public interface IMicrosoftGraphSnippetsCompiler
    {
        CompilationResultsModel RunSnippet(string codeSnippet, Versions version);
    }
}