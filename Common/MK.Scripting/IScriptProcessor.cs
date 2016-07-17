using System.Collections.Generic;
using System.Reflection;

namespace MK.Scripting
{
    public interface IScriptProcessor
    {
        string MessageOutput { get; }

        void AddReferenceAssembly(Assembly assembly);

        void AddNamespace(string ns);

        ICompiledScript Compile(string script, IDictionary<string, object> parameters = null);

        void Evaluate(ICompiledScript script, IDictionary<string, object> parameters = null);
        void Evaluate(IDictionary<string, object> parameters = null);
        void Evaluate(string script, IDictionary<string, object> parameters = null);

        T Evaluate<T>(ICompiledScript script, IDictionary<string, object> parameters = null);
        T Evaluate<T>(IDictionary<string, object> parameters = null);
        T Evaluate<T>(string script, IDictionary<string, object> parameters = null);
    }
}
