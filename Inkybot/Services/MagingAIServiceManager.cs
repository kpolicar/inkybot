using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Api.Resources;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus.Contracts;
using Inkybot.Dofus.Domain;
using Inkybot.Events;
using Westwind.Scripting;
using DofusMagingAIContract = Inkybot.Dofus.Contracts.DofusMagingAI;

namespace Inkybot.Services
{
    public class MagingAIServiceManager : HasDependencies
    {
        private ServiceContainer serviceContainer = null!;
        private bool? previousUserFetchedIsFreeTrial;
        private AuthManager authManager = null!;
        private ActionFactory actionFactory = null!;
        public event EventHandler<MagingAIChangedEventArgs>? MagingAIChanged; 

        public void BindDependencies(ServiceContainer serviceContainer) {
            var apiClient = serviceContainer.GetService<ApiClient>();
            authManager = serviceContainer.GetService<AuthManager>();
            actionFactory = serviceContainer.GetService<ActionFactory>();
            apiClient.UserFetched += OnUserFetched;
            this.serviceContainer = serviceContainer;
        }

        private void OnUserFetched(object sender, FetchedUserEventArgs e) {
            UseBuiltInAIScript(e.user);
        }

        public void UseCustomAIScript(string scriptPath) {
            var script = new CSharpScriptExecution {
                SaveGeneratedCode = true,
                CompilerMode = ScriptCompilerModes.Classic
            };
            var code = File.ReadAllText(scriptPath);
            script.AddDefaultReferencesAndNamespaces();
            script.AddAssembly("Inkybot.Dofus.dll");
            script.AddAssembly("System.Windows.Forms.dll");

            DofusMagingAIContract magus = script.CompileClass(code);
            magus.AddServices(actionFactory);
            magus.Init();
        }

        public void UseBuiltInAIScript(User? user=null) {
            user ??= authManager.User!;
            
            if (!user.is_free_trial && !user.is_subscribed)
                return;
            if (previousUserFetchedIsFreeTrial != null && previousUserFetchedIsFreeTrial == user.is_free_trial)
                return;
            var magus = !user.is_free_trial
                ? (DofusMagingAIContract) new DofusMagingAI()
                : (DofusMagingAIContract) new DofusStandardStatsMagingAI();

            if (magus is HasDependencies dependant) {
                dependant.BindDependencies(serviceContainer);
            }
            magus.AddServices(actionFactory);
            magus.Init();

            serviceContainer.ReplaceService<DofusMagingAIContract>(magus);
            previousUserFetchedIsFreeTrial = user.is_free_trial;
            MagingAIChanged?.Invoke(this, new MagingAIChangedEventArgs(magus));
        }
    }
}
