using System;
using NLog;
using System.Threading;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Dofus;
using Inkybot.Events;
using Inkybot.Exceptions;

namespace Inkybot.Services
{
    internal static class MageRetryHandler
    {
        private static readonly Logger Log = LogManager.GetLogger("mage");
        private const int MaxAttempts = 3;

        public struct Result
        {
            public bool AutoShutdown;
            public bool StopMage;
        }

        public static Result ExecuteWithRetry(
            Func<bool> action,
            MageSession session,
            IMagingDataProvider dataProvider,
            Action<MagingJobErrorEventArgs> onError,
            Action<MagingJobErrorEventArgs> onWarning) {

            var result = new Result();
            var wasRestarting = session.IsRestarting;
            var consecutiveFailures = 0;

            while (consecutiveFailures < MaxAttempts) {
                if (consecutiveFailures > 0) {
                    Log.Info("Restarting mage, attempt " + consecutiveFailures);
                    session.IsRestarting = true;
                }

                var ticksBefore = session.Ticks;

                try {
                    result.AutoShutdown = action();
                    break;
                } catch (Exception exception) when (IsFatalException(exception)) {
                    SaveScanOnItemError(exception, dataProvider);
                    result.AutoShutdown = ResolveAutoShutdown(exception, session.IsRestarting);
                    result.StopMage = true;
                    onError(new MagingJobErrorEventArgs(exception));
                    break;
                } catch (Exception exception) {
                    // If the bot completed successful ticks before failing,
                    // it recovered — reset the failure counter.
                    // (session.Ticks includes the failing tick, so we need > ticksBefore + 1)
                    if (session.Ticks > ticksBefore + 1)
                        consecutiveFailures = 0;

                    consecutiveFailures++;

                    var additionalInfo = !Helpers.System.IsRunnningAsAdmin()
                        ? "Please try running Inkybot as an administrator."
                        : "";

                    if (!Properties.Settings.Default.autoRestartBot || consecutiveFailures >= MaxAttempts) {
                        // If we timed out MaxAttempts times in a row, it's almost certainly
                        // because we ran out of that rune.
                        if (exception is ChangeCheckTimeoutException && session.PreviousAction is CombineRune combineRune)
                            exception = new OutOfRunesException(combineRune.Rune);

                        dataProvider.SaveScan();
                        onError(new MagingJobErrorEventArgs(exception, additionalInfo));
                        result.AutoShutdown = true;
                        result.StopMage = true;
                        break;
                    }

                    onWarning(new MagingJobErrorEventArgs(exception, additionalInfo));

                    if (session.ManuallyStopped) {
                        result.StopMage = true;
                        break;
                    }

                    session.IsMaging = true;
                }
            }

            session.IsRestarting = wasRestarting;
            return result;
        }

        internal static bool IsFatalException(Exception exception) {
            return exception is OutOfRunesException
                || exception is NoItemToMageFoundException
                || exception is UserForbiddenException
                || exception is ItemDoesNotMatchPresetException
                || exception is ItemHasChangedException
                || exception is ItemHasNotChangedException
                || exception is OperationCanceledException;
        }

        private static bool ResolveAutoShutdown(Exception exception, bool restarting) {
            if (exception is OperationCanceledException) return false;
            if (exception is NoItemToMageFoundException || exception is UserForbiddenException) return restarting;
            return true;
        }

        private static void SaveScanOnItemError(Exception exception, IMagingDataProvider dataProvider) {
            if (exception is ItemHasChangedException || exception is ItemHasNotChangedException)
                dataProvider.SaveScan();
        }
    }
}
