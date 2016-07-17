using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Ninject;
using Ninject.Modules;

using StructureMap;

namespace MK.Utilities.Tests
{
	public interface  ITest
	{
	    int Id { get; }
        IFun Fun { get; set; }
	}

    public interface IFun
    {
        int Id { get; }
    }

	public class Test : ITest
	{
	    private static int Counter = 0;

	    public int Id { get; private set; }

		public IFun Fun { get; set; }

        public Test()
        {
            Id = Counter++;
        }

	    public Test(IFun fun)
			: this()
		{
	        Fun = fun;
		}
	}

    public class Fun : IFun
    {
        private static int Counter = 0;

        public int Id { get; private set; }
	
        public Fun()
        {
            Id = Counter++;
        }
    }

    public class ControllerWithSetter
    {
		[Inject]
        public ITest Test { get; set; }
        [Inject]
        public IFun Fun { get; set; }
    }

    public class ControllerWithConstructor
    {
        public ITest Test { get; set; }
        public IFun Fun { get; set; }
        public ControllerWithConstructor(ITest test, IFun fun)
        {
            Test = test;
            Fun = fun;

        }
    }

    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind<ITest>().To<Test>().InSingletonScope();
            Bind<IFun>().To<Fun>();//.InSingletonScope();
            

        }
    }

    [TestClass]
    public class ServiceProviderTests
    {
        [TestMethod]
        public void ServiceProviderTest()
        {
            ServiceProvider.Register<IFun, Fun>(false);
            ServiceProvider.Register<ITest, Test>();

            var test_1 = ServiceProvider.Get<ITest>();
            Assert.IsInstanceOfType(test_1, typeof(Test));
            Assert.IsNotNull(test_1.Fun);

            var fun_1 = ServiceProvider.Get<IFun>();
            Assert.IsInstanceOfType(fun_1, typeof(Fun));
            Assert.AreNotEqual(test_1.Fun.Id, fun_1.Id);

            var test_2 = ServiceProvider.Get<ITest>();
            Assert.IsNotNull(test_2.Fun);
            Assert.AreEqual(test_1.Id, test_2.Id);
            Assert.AreEqual(test_1.Fun.Id, test_2.Fun.Id);

            var c = ServiceProvider.Inject(new ControllerWithSetter());
            Assert.IsNotNull(c.Test);
            Assert.IsNotNull(c.Fun);
            Assert.AreEqual(test_1.Id, c.Test.Id);
            Assert.AreEqual(test_1.Fun.Id, c.Test.Fun.Id);
            Assert.AreNotEqual(test_1.Fun.Id, c.Fun.Id);

            var c2 = ServiceProvider.Get<ControllerWithConstructor>();
            Assert.IsNotNull(c2.Test);
            Assert.IsNotNull(c2.Fun);
            Assert.AreEqual(test_1.Id, c2.Test.Id);
            Assert.AreEqual(test_1.Fun.Id, c2.Test.Fun.Id);
            Assert.AreNotEqual(test_1.Fun.Id, c2.Fun.Id);
        } 
        
        [TestMethod]
        public void StructureMapTest()
        {
            var container = new Container(x =>
                {
                    x.For<ITest>().Use<Test>().Singleton();
                    x.For<IFun>().Use<Fun>();

					x.Policies.SetAllProperties(sc => sc.OfType<ITest>());
					x.Policies.SetAllProperties(sc => sc.OfType<IFun>());
                });

            var test_1 = container.GetInstance<ITest>();
            Assert.IsInstanceOfType(test_1, typeof(Test));
            Assert.IsNotNull(test_1.Fun);

            var fun_1 = container.GetInstance<IFun>();
            Assert.IsInstanceOfType(fun_1, typeof(Fun));
            Assert.AreNotEqual(test_1.Fun.Id, fun_1.Id);

            var test_2 = container.GetInstance<ITest>();
            Assert.IsNotNull(test_2.Fun);
            Assert.AreEqual(test_1.Id, test_2.Id);
            Assert.AreEqual(test_1.Fun.Id, test_2.Fun.Id);

            var c = new ControllerWithSetter();
            container.BuildUp(c);
            Assert.IsNotNull(c.Test);
            Assert.IsNotNull(c.Fun);
            Assert.AreEqual(test_1.Id, c.Test.Id);
            Assert.AreEqual(test_1.Fun.Id, c.Test.Fun.Id);
            Assert.AreNotEqual(test_1.Fun.Id, c.Fun.Id);

            var c2 = container.GetInstance<ControllerWithConstructor>();
            Assert.IsNotNull(c2.Test);
            Assert.IsNotNull(c2.Fun);
            Assert.AreEqual(test_1.Id, c2.Test.Id);
            Assert.AreEqual(test_1.Fun.Id, c2.Test.Fun.Id);
            Assert.AreNotEqual(test_1.Fun.Id, c2.Fun.Id);
        }

        [TestMethod]
        public void NinjectTest()
        {
            var kernel = new StandardKernel(new Module());

            var test_1 = kernel.Get<ITest>();
            Assert.IsInstanceOfType(test_1, typeof(Test));
            Assert.IsNotNull(test_1.Fun);

            var fun_1 = kernel.Get<IFun>();
            Assert.IsInstanceOfType(fun_1, typeof(Fun));
            Assert.AreNotEqual(test_1.Fun.Id, fun_1.Id);

            var test_2 = kernel.Get<ITest>();
            Assert.IsNotNull(test_2.Fun);
            Assert.AreEqual(test_1.Id, test_2.Id);
            Assert.AreEqual(test_1.Fun.Id, test_2.Fun.Id);

            var c = kernel.Get<ControllerWithSetter>();
            Assert.IsNotNull(c.Test);
            Assert.IsNotNull(c.Fun);
            Assert.AreEqual(test_1.Id, c.Test.Id);
            Assert.AreEqual(test_1.Fun.Id, c.Test.Fun.Id);
            Assert.AreNotEqual(test_1.Fun.Id, c.Fun.Id);

            var c2 = kernel.Get<ControllerWithConstructor>();
            Assert.IsNotNull(c2.Test);
            Assert.IsNotNull(c2.Fun);
            Assert.AreEqual(test_1.Id, c2.Test.Id);
            Assert.AreEqual(test_1.Fun.Id, c2.Test.Fun.Id);
            Assert.AreNotEqual(test_1.Fun.Id, c2.Fun.Id);
        }

    }
}
