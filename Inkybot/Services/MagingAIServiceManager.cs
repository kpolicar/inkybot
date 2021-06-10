using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Api.Resources;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Dofus.Domain;
using Inkybot.Events;
using Westwind.Scripting;
using DofusMagingAIContract = Inkybot.Dofus.Contracts.DofusMagingAI;
using StatConfigProviderContract = Inkybot.Dofus.Contracts.StatConfigProvider;

namespace Inkybot.Services
{
    public class MagingAIServiceManager : HasDependencies
    {
        private ServiceContainer serviceContainer = null!;
        private bool? previousUserFetchedIsFreeTrial;
        private AuthManager authManager = null!;
        private ActionFactory actionFactory = null!;
        private StatConfigProviderContract statConfigProvider = null!;
        public event EventHandler<MagingAIChangedEventArgs>? MagingAIChanged;
        private bool UsingCustomScript;

        public void BindDependencies(ServiceContainer serviceContainer) {
            var apiClient = serviceContainer.GetService<ApiClient>();
            authManager = serviceContainer.GetService<AuthManager>();
            actionFactory = serviceContainer.GetService<ActionFactory>();
            statConfigProvider = serviceContainer.GetService<StatConfigProviderContract>();
            apiClient.UserFetched += OnUserFetched;
            this.serviceContainer = serviceContainer;
        }

        private void OnUserFetched(object sender, FetchedUserEventArgs e) {
            if (!e.user.is_free_trial && !e.user.is_subscribed)
                return;
            
            if (previousUserFetchedIsFreeTrial == null || previousUserFetchedIsFreeTrial != e.user.is_free_trial) {
                if (!UsingCustomScript)
                    UseBuiltInAIScript(e.user);
            }
            
            if (UsingCustomScript && !e.user.canUseCustomMagingAI)
                UseBuiltInAIScript(e.user);
        }

        public void UseCustomAIScript(string scriptPath) {
            var script = new CSharpScriptExecution {
                SaveGeneratedCode = true,
                CompilerMode = ScriptCompilerModes.Roslyn
            };
            var code = File.ReadAllText(scriptPath);
            script.AddDefaultReferencesAndNamespaces();
            script.AddAssembly("Inkybot.Dofus.dll");
            script.AddAssembly("System.Windows.Forms.dll");
            script.AddAssembly("System.Drawing.dll");

            try {
                DofusMagingAIContract magus = script.CompileClass(code);
                if (script.Error) {
                    throw script.LastException;
                }
                if (magus is HasDependencies dependant)
                    dependant.BindDependencies(serviceContainer);
                if (magus is CustomDofusMagingAI customDofusMagingAI) {
                    var defaultAI = new DofusMagingAI();
                    defaultAI.BindDependencies(serviceContainer);
                    defaultAI.Init();
                    BindCustomMagingAIWithDefaultAI(defaultAI, customDofusMagingAI);
                }
                magus.Init();
            
                serviceContainer.ReplaceService<DofusMagingAIContract>(magus);
                MagingAIChanged?.Invoke(this, new MagingAIChangedEventArgs(magus));
            
                UsingCustomScript = true;
            } catch (Exception e) {
                Debug.WriteLine(e);
                throw;
            }

        }

        private void BindCustomMagingAIWithDefaultAI(DofusMagingAI defaultAI, CustomDofusMagingAI customDofusMagingAI) {
            defaultAI.OverridePerfectionResolve = resolve => customDofusMagingAI.OverrideMageToPerfectStatsWithSink(resolve.Mage);
            defaultAI.OverrideReachMinimumResolve = resolve => customDofusMagingAI.OverrideOvermageToReachMinimum(resolve.Mage);
            defaultAI.OverrideFinishSinkOverride = resolve => customDofusMagingAI.OverrideOvermageWithRemainingSink(resolve.Mage);
            defaultAI.OverrideExoResolve = resolve => customDofusMagingAI.OverrideExoMage(resolve.Mage);
            customDofusMagingAI.SetDefaultMagingAI(defaultAI);
        }

        public void UseBuiltInAIScript(User? user=null) {
            user ??= authManager.User;
            
            var magus = !user?.is_free_trial ?? true
                ? (DofusMagingAIContract) new DofusMagingAI()
                : (DofusMagingAIContract) new DofusStandardStatsMagingAI();

            if (magus is HasDependencies dependant)
                dependant.BindDependencies(serviceContainer);
            magus.Init();

            serviceContainer.ReplaceService<DofusMagingAIContract>(magus);
            previousUserFetchedIsFreeTrial = user?.is_free_trial ?? false;
            MagingAIChanged?.Invoke(this, new MagingAIChangedEventArgs(magus));
            
            UsingCustomScript = false;
        }
    }
}
