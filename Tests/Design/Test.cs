using System;
using System.Collections.Generic;
using Inkybot.Design;
using NUnit.Framework;

namespace Tests.Design
{
    public abstract class Test
    {
        private Dictionary<Type, object> _services;

        protected ServiceContainer ServiceContainer;
            
        [SetUp]
        protected virtual void Init() {
            ServiceContainer = new ServiceContainer();
            _services = Services();
            BindServices();
        }

        protected abstract Dictionary<Type, object> Services();
        
        private void BindServices() {
            foreach (var serviceBinding in _services) {
                var @abstract = serviceBinding.Key;
                var concrete = serviceBinding.Value;
                ServiceContainer.AddService(@abstract, concrete);
            }
            foreach (var serviceBinding in _services) {
                var concrete = serviceBinding.Value;
                if (concrete is InjectableService service) {
                    service.BindDependencies(ServiceContainer);
                }
            }
        }
    }
}
