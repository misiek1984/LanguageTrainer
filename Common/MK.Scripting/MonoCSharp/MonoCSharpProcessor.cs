using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using MK.Utilities;
using Microsoft.CSharp;

using Mono.CSharp;

namespace MK.Scripting.MonoCSharp
{
    public class MonoCSharpProcessor : IScriptProcessor
    {
        #region Fields & Properties

        private Evaluator _evaluator;

        private StringBuilder _sb = new StringBuilder();

        private MonoCSharpCompiledScript _lastCompiledMethod;

        public string MessageOutput
        {
            get { return _sb.ToString(); }
        }

        #endregion

        #region Constructor

        public MonoCSharpProcessor()
        {
            _evaluator = new Evaluator(
                new CompilerContext(
                    new CompilerSettings(),
                    new StreamReportPrinter(new StringWriter(_sb))));

            _evaluator.ReferenceAssembly(typeof(MonoCSharpProcessor).Assembly);

            _evaluator.Run("using System;");
            _evaluator.Run("using System.Collections;");
            _evaluator.Run("using System.Collections.Generic;");
            _evaluator.Run("using System.Linq;");
            _evaluator.Run("using System.Text;");
            _evaluator.Run("using System.Xml;");
            _evaluator.Run("using " + typeof(ScriptContext).Namespace + ";");
        }

        #endregion

        #region Public Methods

        public void AddReferenceAssembly(Assembly assembly)
        {
            assembly.NotNull("assembly");
            _sb.Clear();

            _evaluator.ReferenceAssembly(assembly);
        }

        public void AddNamespace(string ns)
        {
            ns.NotNullAndEmpty("ns");
            _sb.Clear();

            _evaluator.Run("using " + ns + ";");
        }

        public ICompiledScript Compile(string script, IDictionary<string, object> parameters = null)
        {
            _sb.Clear();

            var sb = PrepareParameters(parameters);
            sb.AppendLine(script);

            _lastCompiledMethod = new MonoCSharpCompiledScript();

            var counter = 0;
            while (!_lastCompiledMethod.IsCompiled && counter < 5)
            {
                _lastCompiledMethod.CompiledMethod = _evaluator.Compile(sb.ToString());
                counter++;
            }
            return _lastCompiledMethod;
        }

        public void Evaluate(IDictionary<string, object> parameters = null)
        {
            Evaluate(_lastCompiledMethod, parameters);
        }

        public void Evaluate(string script, IDictionary<string, object> parameters = null)
        {
            _sb.Clear();

            var sb = PrepareParameters(parameters);
            sb.AppendLine(script);

            _evaluator.Run(sb.ToString());
        }

        public void Evaluate(ICompiledScript script, IDictionary<string, object> parameters = null)
        {
            ScriptContext.Items = parameters;

            object res = null;
            ((MonoCSharpCompiledScript)script).CompiledMethod.Invoke(ref res);
        }

        public T Evaluate<T>(IDictionary<string, object> parameters = null)
        {
            return Evaluate<T>(_lastCompiledMethod, parameters);
        }

        public T Evaluate<T>(string script, IDictionary<string, object> parameters = null)
        {
            _sb.Clear();

            Evaluate(script, parameters);

            return (T)ScriptContext.Result;
        }

        public T Evaluate<T>(ICompiledScript script, IDictionary<string, object> parameters = null)
        {
            ScriptContext.Items = parameters;
            
             object res = null;
            ((MonoCSharpCompiledScript)script).CompiledMethod.Invoke(ref res);

            return (T) ScriptContext.Result;
        }

        #endregion

        #region Private Methods

        private static StringBuilder PrepareParameters(IDictionary<string, object> parameters)
        {
            ScriptContext.Items = parameters;

            var sb = new StringBuilder();

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    var typeName = ExtractTypeName(kvp);

                    sb.AppendFormat("var {0} = ({1}){2}.Items[\"{0}\"];", kvp.Key, typeName, typeof(ScriptContext).Name);
                    sb.AppendLine();
                }
            }

            return sb;
        }

        private static string ExtractTypeName(KeyValuePair<string, object> kvp)
        {
            if (kvp.Value == null)
                throw new Exception("null parameters are not supported!");

            var type = kvp.Value.GetType();

            using (var provider = new CSharpCodeProvider())
            {
                var typeRef = new CodeTypeReference(type);
                return  provider.GetTypeOutput(typeRef);
            }
        }

        #endregion
    }
}
