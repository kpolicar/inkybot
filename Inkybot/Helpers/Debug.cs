using System.Reflection;

namespace Inkybot.Helpers
{
    public static class Debug
    {
        public static object Call(object instance, string method, object[]? methodParams = null) {
            var dynMethod = instance.GetType().GetMethod(method, 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            return dynMethod!.Invoke(instance, methodParams);
        }
        
        public static T GetFieldValue<T>(this object obj, string name) {
            // Set the flags so that private and public fields from instances will be found
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = obj.GetType().GetField(name, bindingFlags);
            return (T)field?.GetValue(obj)!;
        }
    }
}
