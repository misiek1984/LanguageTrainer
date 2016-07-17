using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using Microsoft.CSharp;
using Mono.CSharp;

namespace MK.Scripting
{
    public static class EvaluatorExtensions
    {
        public static T Eval<T>(this string script, bool repleaceQuotes = false)
        {
            var scriptToRun = repleaceQuotes ? script.Replace("'", "\"") : script;

            return (T)Evaluator.Evaluate(scriptToRun);
        }

        public static void Run(this string script, bool repleaceQuotes = false)
        {
            var scriptToRun = repleaceQuotes ? script.Replace("'", "\"") : script;
            Evaluator.Run(scriptToRun);
        }
    }

    public static class ScriptContext
    {
        static ScriptContext()
        {
            Items = new Dictionary<string, object>();
        }

        private static object _result;
        public static object Result
        {
            get { return _result; }
            set { _result = value; }
        }

        private static IDictionary<string, object> _items;
        public static IDictionary<string, object> Items
        {
            get { return _items; }
            set { _items = value; }
        }
    }

    public class MonoCSharpProcessor : IScriptProcessor
    {
        public MonoCSharpProcessor()
        {
            Evaluator.Init(new string[] { });
            Evaluator.ReferenceAssembly(typeof(MonoCSharpProcessor).Assembly);
            Evaluator.Run("using System;");
            Evaluator.Run("using System.Collections;");
            Evaluator.Run("using System.Collections.Generic;");
            Evaluator.Run("using System.Linq;");
            Evaluator.Run("using System.Text;");
            Evaluator.Run("using System.Xml;");
            Evaluator.Run("using " + typeof(ScriptContext).Namespace + ";");
        }

        public void Evaluate(string script, IDictionary<string, object> parameters = null)
        {
            var sb = PrepareParameters(parameters);
            sb.AppendLine(script);

            Evaluator.
            sb.ToString().Run();
        }

        public T Evaluate<T>(string script, IDictionary<string, object> parameters = null)
        {
            Evaluate(script, parameters);

            return (T)ScriptContext.Result;
        }


        private static StringBuilder PrepareParameters(IDictionary<string, object> parameters)
        {
            ScriptContext.Items = parameters;

            var sb = new StringBuilder();

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    var typeName = ExtractTypeName(kvp);

                    sb.AppendFormat("var {1} = ({2}){0}.Items[\"{1}\"];", typeof(ScriptContext).Name, kvp.Key, typeName);
                    sb.AppendLine();
                }
            }

            return sb;
        }

        private static string ExtractTypeName(KeyValuePair<string, object> kvp)
        {
            var type = kvp.Value.GetType();

            using (var provider = new CSharpCodeProvider())
            {
                var typeRef = new CodeTypeReference(type);
                return  provider.GetTypeOutput(typeRef);
            }
        }
    }
}
