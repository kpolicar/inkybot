using SystemServiceContainer = System.ComponentModel.Design.ServiceContainer;

namespace Inkybot.Design
{
    public class ServiceContainer :SystemServiceContainer
    {
        public T GetService<T>() {
            return (T) base.GetService(typeof(T));
        }
    }
}
