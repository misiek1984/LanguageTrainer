using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StructureMap;
using StructureMap.Configuration.DSL;
using StructureMap.Pipeline;

namespace MK.Utilities
{
    public static class ServiceProvider
    {
        private static object _contaierLock = new object();
        private static Container _contaier;

        private static object _lock = new object();
        private static List<Object> _services = new List<object>();

        static ServiceProvider()
        {
            _contaier = new Container();
        }

        public static void RegisterService<T>(T service) where T : class
        {
            lock (_lock)
            {
                _services.Remove(service);
                _services.Add(service);
            }
        }
        public static T GetService<T>() where T : class
        {
            object res;
            lock (_lock)
            {
                res = (from s in _services
                       where s is T
                       select s).FirstOrDefault();
            }

            if (res == null)
                res = Get<T>();

            Inject(res);

            return res as T;
        }

        public static void Use(Container container)
        {
            lock (_contaierLock)
            {
                _contaier = container;
            }
        }

        public static void Register<I, T>(bool singleton = true, Action<SmartInstance<T,I>> action = null)
            where I : class
            where T : class, I
        {
            lock (_contaierLock)
            {
                _contaier.Configure(_ => _.Policies.SetAllProperties(sc => sc.OfType<I>()));

                _contaier.Configure(_ =>
                {
                    var res = _.For<I>().Use<T>();

                    if (singleton)
                        res.Singleton();

                    if (action != null)
                        action(res);
                });
            }
        }

        public static T Get<T>() where T : class
        {
            lock (_contaierLock)
            {
                return _contaier.GetInstance<T>();
            }
        }

        public static T Inject<T>(T instance) where T : class
        {
            lock (_contaierLock)
            {
                _contaier.BuildUp(instance);
                return instance;
            }
        }
    }
}
