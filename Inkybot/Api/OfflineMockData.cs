using System;
using Inkybot.Api.Resources;

namespace Inkybot.Api
{
    /// <summary>
    ///     Central source of the dummy data returned by the API layer while the app runs offline.
    ///     The inkybot.me backend has been shut down permanently, so every network-backed call in
    ///     <see cref="ApiClient"/>, <see cref="ApiAuthManager"/> and <see cref="ApiConnection"/> is
    ///     served from here instead. The <see cref="User"/> and <see cref="FreeTrial"/> resources
    ///     expose read-only getters over obfuscated backing fields, so they are populated by assigning
    ///     those fields directly.
    /// </summary>
    internal static class OfflineMockData
    {
        public static User MockUser() => new User {
            gMUnPVsYkgMgZqkulaQJ = "offline@inkybot.local", // email
            RTPbmvpAWeIUXnsnPdcm = "Offline User",          // name
            xXEgoygDzogjOgiNIxJH = true,                    // is_subscribed
            gNdQBAzXtFIXjCfjNPIK = false,                   // is_free_trial
            zSinZTfatTfVLaFljOqC = null,                    // free_trial_ends_at
            dSkngxkoTRycwTHwkRWq = false,                   // free_trial_available
            JKrhIgzULnxEiszhYvam = false,                   // onStarterPlan
            xrxqfXZtzcPHQMrccSSd = false,                   // onStandardPlan
            MgtwORXnPLSWksFIVdJg = true,                    // onUnlimitedPlan
            tNzptPqDMerMLogKzJfk = true,                    // canUseCustomMagingAI
            JVaYDkExoaVfqEqSWlwA = true,                    // canViewStatistics
            tnsVaYvUYfUyMoUCLcSk = true,                    // canCreateStatistics
            ItfTLInEaoqhyTIMXclm = true,                    // canPublishExos
            sLQXvDUEEttotgvaSwTj = true,                    // canMageExos
            QMESAtjSbArNTcegpcQn = true,                    // canUseMageQueue
            HMsEyaQcEUMkgnCBhgvX = int.MaxValue,            // numberOfExoMagesLeftInPlan
        };

        public static FreeTrial MockFreeTrial() => new FreeTrial {
            k3ExasHrvpLP4Rm = false,           // expired
            Ly7lqp8846XaG7P = DateTime.Now,    // created_at
        };

        public static VersionDetails MockVersionDetails() => new VersionDetails {
            number = Program.VersionNumber,
            name = Program.Version,
            endpoint = Program.VersionEndpoint,
        };

        public static AuthDetails MockAuthDetails() => new AuthDetails {
            access_token = "offline",
            refresh_token = "offline",
            expires_in = int.MaxValue,
            token_type = "Bearer",
        };
    }
}
