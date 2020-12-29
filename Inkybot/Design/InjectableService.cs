namespace Inkybot.Design
{
    public interface InjectableService
    {
        public void BindDependencies(ServiceContainer serviceContainer);
    }
}
