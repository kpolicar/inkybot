using System;
using System.Collections.Generic;
using Inkybot.Contracts;
using Tests.Services;

namespace Tests.Design
{
    public abstract class SingleImageScreenReaderTest : ScreenReaderTest
    {
        protected abstract string Path {
            get;
        }

        protected override Dictionary<Type, object> Services() {
            var services = base.Services();
            var screen = new FileScreenCapture(Path);
            services[typeof(ScreenCapture)] = screen;
            
            return services;
        }
    }
}
