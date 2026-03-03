using System;
using System.Diagnostics;
using System.Threading;
using Inkybot.Dofus;
using Inkybot.Events;
using Inkybot.Exceptions;

namespace Inkybot.Services
{
    internal static class MageRetryHandler
    {
        private const int MaxAttempts = 3;

        public struct Result
        {
            public bool AutoShutdown;
            public bool StopMage;
        }

        public static Result ExecuteWithRetry(
            Func<bool> action,
            MageSession session,
            ScreenReaderDataProvider dataProvider,
            Action<MagingJobErrorEventArgs> onError,
            Action<MagingJobErrorEventArgs> onWarning) {

            var result = new Result();
            var wasRestarting = session.IsRestarting;

            for (var attempt = 0; attempt < MaxAttempts; attempt++) {
                if (attempt > 0) {
                    Debug.WriteLine("Restarting mage, attempt " + attempt);
                    session.IsRestarting = true;
                }

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
                    session.UnsuccessfulCombineTicks++;
                    LogException(exception);

                    var additionalInfo = !Helpers.System.IsRunnningAsAdmin()
                        ? "Please try running Inkybot as an administrator."
                        : "";

                    if (!Properties.Settings.Default.autoRestartBot || attempt >= MaxAttempts - 1) {
                        onError(new MagingJobErrorEventArgs(exception, additionalInfo));
                        result.AutoShutdown = true;
                        result.StopMage = true;
                        break;
                    }

                    onWarning(new MagingJobErrorEventArgs(exception, additionalInfo));
                    Thread.Sleep(1000);
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

        private static void SaveScanOnItemError(Exception exception, ScreenReaderDataProvider dataProvider) {
            if (exception is ItemHasChangedException || exception is ItemHasNotChangedException)
                dataProvider.Scan?.Save();
        }

        private static void LogException(Exception exception) {
            if (exception is AggregateException aggregateException) {
                Debug.WriteLine("Aggregate exception!");
                foreach (var inner in aggregateException.InnerExceptions) {
                    Debug.WriteLine(inner.Message);
                    Debug.WriteLine(inner.StackTrace);
                }
            } else {
                Debug.WriteLine(exception.Message);
                Debug.WriteLine(exception.StackTrace);
            }
        }
    }
}
