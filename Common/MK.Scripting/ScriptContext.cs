using System.Collections.Generic;

namespace MK.Scripting
{
    public static class ScriptContext
    {
        static ScriptContext()
        {
            Items = new Dictionary<string, object>();
        }

        public static object Result { get; set; }

        public static IDictionary<string, object> Items { get; set; }
    }
}
