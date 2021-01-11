using System;
using System.Threading.Tasks;
using Inkybot.Api;
using Inkybot.Api.Resources;
using Inkybot.Events;

namespace Inkybot.Contracts
{
    public interface AuthManager
    {
        public User? User {
            get;
        }
        public event EventHandler<ApiConnectionChangedEventArgs>? ConnectionChanged;
        public Task<ApiConnection?> Login(string username, string password);
        public void Logout();
    }
}
