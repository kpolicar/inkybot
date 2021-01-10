namespace Inkybot.Design
{
    public interface HasDependencies
    {
        public void BindDependencies(ServiceContainer serviceContainer);
    }
}
