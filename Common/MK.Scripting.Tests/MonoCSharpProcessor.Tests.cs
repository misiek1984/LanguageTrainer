using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using MK.MyMath;
using MK.Scripting.MonoCSharp;

namespace MK.Scripting.Tests
{
    [TestClass]
    public class MonoCSharpProcessorTests
    {
        public const string CalculateDistanceScript =
             "var counter = 0;\n" +
             "for(var i  = 0; i < v1.Count; ++i)\n" +
             "     if(v1[i] != v2[i])\n" +
             "         counter++;\n" +
             "ScriptContext.Result = (double)counter;";

        public const string TypoScript = "Console.Writeline(\"Hello\")";

        public const string CreateNameValueCollectionScript = "var nvc = new NameValueCollection();";

        public const string ModifyInputScript =
             "for(var i  = 0; i < v1.Count; ++i)\n" +
             "     v1[i] = 0;";

        [TestMethod]
        public void Compile_MissingUsing_ShouldNotBeCompiled()
        {
            var processor = new MonoCSharpProcessor();

            var script = processor.Compile(CreateNameValueCollectionScript);

            Assert.IsFalse(script.IsCompiled);
            Assert.IsFalse(String.IsNullOrEmpty( processor.MessageOutput.ToString()));

            processor.AddNamespace("System.Collections.Specialized");

            script = processor.Compile(CreateNameValueCollectionScript);

            Assert.IsTrue(script.IsCompiled);
            Assert.IsTrue(String.IsNullOrEmpty(processor.MessageOutput.ToString()));
        }

        [TestMethod]
        public void Compile_TypoScript_ShouldNotBeCompiled()
        {
            var processor = new MonoCSharpProcessor();

            var script = processor.Compile(TypoScript);

            Assert.IsFalse(script.IsCompiled);
            Assert.IsFalse(String.IsNullOrEmpty(processor.MessageOutput.ToString()));
        }

        [TestMethod]
        public void CompileAndEvaluate_CalculateDistanceScript_ResultShouldBeCorrect()
        {
            var processor = new MonoCSharpProcessor();
            processor.AddReferenceAssembly(typeof(MyMath.MyMath).Assembly);

            var parameters = new Dictionary<string, object> { { "v1", new VectorD() }, { "v2", new VectorD() } };
            var script = processor.Compile(CalculateDistanceScript, parameters);

            Assert.IsTrue(script.IsCompiled);
            Assert.IsTrue(String.IsNullOrEmpty(processor.MessageOutput.ToString()));


            parameters["v1"] = new VectorD() { 1, 2, 3, 4, 5};
            parameters["v2"] = new VectorD() { 1, 2, -3,4, -5 } ;
            var distance =  processor.Evaluate<double>(script, parameters);
            Assert.AreEqual(2, distance);


            parameters["v1"] = new VectorD() { 1, 2, 3, 4, 5 };
            parameters["v2"] = new VectorD() { -1, -2, -3, -4, -5 };
            distance = processor.Evaluate<double>(script, parameters);
            Assert.AreEqual(5, distance);


            distance = processor.Evaluate<double>(parameters);
            Assert.AreEqual(5, distance);
        }

        [TestMethod]
        public void DoNotCompileAndEvaluate_CalculateDistanceScript_ResultShouldBeCorrect()
        {
            var processor = new MonoCSharpProcessor();
            processor.AddReferenceAssembly(typeof(MyMath.MyMath).Assembly);

            var parameters = new Dictionary<string, object> { { "v1", new VectorD() }, { "v2", new VectorD() } };

            parameters["v1"] = new VectorD() { 1, 2, 3, 4, 5 };
            parameters["v2"] = new VectorD() { 1, 2, -3, 4, -5 };
            var distance = processor.Evaluate<double>(CalculateDistanceScript, parameters);

            Assert.AreEqual(2, distance);

            parameters["v1"] = new VectorD() { 1, 2, 3, 4, 5 };
            parameters["v2"] = new VectorD() { -1, -2, -3, -4, -5 };
            distance = processor.Evaluate<double>(CalculateDistanceScript, parameters);

            Assert.AreEqual(5, distance);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void DoNotCompile_CallEvaluateByPassingOnlyParameters_CalculateDistanceScript_ExceptionShouldBeThrown()
        {
            var processor = new MonoCSharpProcessor();
            processor.AddReferenceAssembly(typeof(MyMath.MyMath).Assembly);

            var parameters = new Dictionary<string, object> { { "v1", new VectorD() }, { "v2", new VectorD() } };


            parameters["v1"] = new VectorD() { 1, 2, 3, 4, 5 };
            parameters["v2"] = new VectorD() { -1, -2, -3, -4, -5 };
            var distance = processor.Evaluate<double>(CalculateDistanceScript, parameters);
            Assert.AreEqual(5, distance);


            distance = processor.Evaluate<double>(parameters);
        }

        [TestMethod]
        public void CompileAndEvaluate_ModifyInputScript_ElementsOfInputListShouldBeSetToZero()
        {
            var processor = new MonoCSharpProcessor();
            processor.AddReferenceAssembly(typeof(MyMath.MyMath).Assembly);

            var parameters = new Dictionary<string, object> { { "v1", new VectorD() } };
            var script = processor.Compile(ModifyInputScript, parameters);

            Assert.IsTrue(script.IsCompiled);
            Assert.IsTrue(String.IsNullOrEmpty(processor.MessageOutput.ToString()));

            var v = new VectorD() { 1, 2, 3, 4, 5 }; ;
            parameters["v1"] = v;
            processor.Evaluate(script, parameters);
            Assert.IsTrue(v.All(e => e == 0));


            v = new VectorD() { 1, 2, 3, 4, 5 };
            parameters["v1"] = v;
            processor.Evaluate(parameters);
            Assert.IsTrue(v.All(e => e == 0));
        }

        [TestMethod]
        public void DoNotCompileAndEvaluate_ModifyInputScript_ElementsOfInputListShouldBeSetToZero()
        {
            var processor = new MonoCSharpProcessor();
            processor.AddReferenceAssembly(typeof(MyMath.MyMath).Assembly);

            var parameters = new Dictionary<string, object> { { "v1", new VectorD() } };


            var v = new VectorD() { 1, 2, 3, 4, 5 }; ;
            parameters["v1"] = v;
            processor.Evaluate(ModifyInputScript, parameters);
            Assert.IsTrue(v.All(e => e == 0));
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void DoNotCompile_CallEvaluateByPassingOnlyParameters_ModifyInputScript_ElementsOfInputListShouldBeSetToZero()
        {
            var processor = new MonoCSharpProcessor();
            processor.AddReferenceAssembly(typeof(MyMath.MyMath).Assembly);

            var parameters = new Dictionary<string, object> { { "v1", new VectorD() } };


            var v = new VectorD() { 1, 2, 3, 4, 5 }; ;
            parameters["v1"] = v;
            processor.Evaluate(ModifyInputScript, parameters);
            Assert.IsTrue(v.All(e => e == 0));


            v = new VectorD() { 1, 2, 3, 4, 5 };
            parameters["v1"] = v;
            processor.Evaluate(parameters);
            Assert.IsTrue(v.All(e => e == 0));

        }
    }
}
