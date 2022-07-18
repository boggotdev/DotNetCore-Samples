using Tech.Models.Common;
using Tech.Services.Helpers;
using Tech.Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity;

namespace Tech.Services.Common
{
    public class ServiceManager
    {
        private readonly IUnityContainer _container;
        private readonly AppSettings _appSettings;
        private static ConcurrentDictionary<string, List<Type>> _subscribers;

        static ServiceManager()
        {
            var initialCapacity = 50;
            int concurrencyLevel = Environment.ProcessorCount * 2;
            _subscribers = new ConcurrentDictionary<string, List<Type>>(concurrencyLevel, initialCapacity);
        }

        public ServiceManager(
            IUnityContainer container,
            AppSettings appSettings
            )
        {
            _container = container;
            _appSettings = appSettings;
        }

        public static void Subscribe<TArgs, SType>() where TArgs : EventArgs where SType : IServiceHandler<TArgs>
        {
            var name = typeof(TArgs).Name;

            if (!_subscribers.ContainsKey(name))
                _subscribers.TryAdd(name, new List<Type>());

            _subscribers[name].Add(typeof(SType));
        }

        public virtual void Execute<TArgs>(object sender, TArgs args) where TArgs : EventArgs
        {
            if (args is null)
                return;

            var name = args.GetType().Name;

            if (_subscribers.ContainsKey(name))
            {
                foreach (var handler in _subscribers[name])
                {
                    var service = (IServiceHandler<TArgs>)_container.Resolve(handler);

                    Task task = service.RunAsync(args);

                    task.ContinueWith(
                        task => ExceptionLoggerHelper.HandleServiceExceptionAsync(
                            _appSettings,
                            task.Exception.InnerException),
                        TaskContinuationOptions.OnlyOnFaulted
                        );
                }
            }
        }
    }
}
