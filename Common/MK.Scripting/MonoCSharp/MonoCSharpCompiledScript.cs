using Mono.CSharp;

namespace MK.Scripting.MonoCSharp
{
    public class MonoCSharpCompiledScript : ICompiledScript
    {
        public CompiledMethod CompiledMethod { get; set; }

        public bool IsCompiled
        {
            get { return CompiledMethod != null; }
        }
    }
}
