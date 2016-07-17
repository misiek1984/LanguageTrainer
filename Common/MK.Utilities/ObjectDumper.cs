using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace MK.Utilities
{
    /// <summary>
    /// Copyright (C) Microsoft Corporation.  All rights reserved.
    /// http://code.msdn.microsoft.com/csharpsamples/Release/ProjectReleases.aspx?ReleaseId=8
    /// </summary>
    public class ObjectDumper
    {
        public string _indent = "\t";
        public string _tab = "\t";

        public static void Write(object element, string indent = "\t", string tab = "\t")
        {
            Write(element, 0, indent, tab);
        }

        public static void Write(object element, int depth, string indent = "\t", string tab = "\t")
        {
            Write(element, depth, Console.Out, indent, tab);
        }

        public static void Write(object element, int depth, TextWriter log, string indent = "\t", string tab = "\t")
        {
            ObjectDumper dumper = new ObjectDumper(depth, indent, tab);
            dumper.writer = log;
            dumper.WriteObject(null, element);
        }

        private TextWriter writer;
        private int pos;
        private int level;
        private int depth;

        private ObjectDumper(int depth, string indent = "\t", string tab = "\t")
        {
            this.depth = depth;
            _indent = indent;
            _tab = tab;
        }

        private void Write(string s)
        {
            if (s != null)
            {
                writer.Write(s);
                pos += s.Length;
            }
        }

        private void WriteIndent()
        {
            for (int i = 0; i < level; i++) writer.Write(_indent);
        }

        private void WriteLine()
        {
            writer.WriteLine();
            pos = 0;
        }

        private void WriteTab()
        {
            Write(_tab);
            while (pos % 8 != 0) Write(_tab);
        }

        private void WriteObject(string prefix, object element)
        {
            if (element == null || element is ValueType || element is string)
            {
                WriteIndent();
                Write(prefix);
                WriteValue(element);
                WriteLine();
            }
            else
            {
                IEnumerable enumerableElement = element as IEnumerable;
                if (enumerableElement != null)
                {
                    foreach (object item in enumerableElement)
                    {
                        if (item is IEnumerable && !(item is string))
                        {
                            WriteIndent();
                            Write(prefix);
                            Write("...");
                            WriteLine();
                            if (level < depth)
                            {
                                level++;
                                try
                                {
                                    WriteObject(prefix, item);
                                }
                                catch
                                { }
                                level--;
                            }
                        }
                        else
                        {
                            WriteObject(prefix, item);
                        }
                    }
                }
                else
                {
                    MemberInfo[] members = element.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                    WriteIndent();
                    Write(prefix);
                    bool propWritten = false;
                    foreach (MemberInfo m in members)
                    {
                        FieldInfo f = m as FieldInfo;
                        PropertyInfo p = m as PropertyInfo;
                        if (f != null || p != null)
                        {
                            if (propWritten)
                            {
                                WriteTab();
                            }
                            else
                            {
                                propWritten = true;
                            }
                            Write(m.Name);
                            Write("=");
                            Type t = f != null ? f.FieldType : p.PropertyType;
                            if (t.IsValueType || t == typeof(string))
                            {
                                WriteValue(f != null ? f.GetValue(element) : p.GetValue(element, null));
                            }
                            else
                            {
                                if (typeof(IEnumerable).IsAssignableFrom(t))
                                {
                                    Write("...");
                                }
                                else
                                {
                                    Write("{ }");
                                }
                            }
                        }
                    }
                    if (propWritten) WriteLine();
                    if (level < depth)
                    {
                        foreach (MemberInfo m in members)
                        {
                            FieldInfo f = m as FieldInfo;
                            PropertyInfo p = m as PropertyInfo;
                            if (f != null || p != null)
                            {
                                Type t = f != null ? f.FieldType : p.PropertyType;
                                if (!(t.IsValueType || t == typeof(string)))
                                {
                                    object value = f != null ? f.GetValue(element) : p.GetValue(element, null);
                                    if (value != null)
                                    {
                                        level++;
                                        try
                                        {
                                            WriteObject(m.Name + ": ", value);
                                        }
                                        catch
                                        { }
                                        level--;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void WriteValue(object o)
        {
            if (o == null)
            {
                Write("null");
            }
            else if (o is DateTime)
            {
                Write(((DateTime)o).ToShortDateString());
            }
            else if (o is ValueType || o is string)
            {
                Write(o.ToString());
            }
            else if (o is IEnumerable)
            {
                Write("...");
            }
            else
            {
                Write("{ }");
            }
        }
    }
}
