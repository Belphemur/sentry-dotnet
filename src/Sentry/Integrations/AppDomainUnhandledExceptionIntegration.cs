using System;
using System.Runtime.ExceptionServices;
using System.Security;
using Sentry.Internal;
using Sentry.Protocol;

namespace Sentry.Integrations
{
    internal class AppDomainUnhandledExceptionIntegration : ISdkIntegration
    {
        private readonly IAppDomain _appDomain;
        private IHub? _hub;

        internal AppDomainUnhandledExceptionIntegration(IAppDomain? appDomain = null)
            => _appDomain = appDomain ?? AppDomainAdapter.Instance;

        public void Register(IHub hub, SentryOptions _)
        {
            _hub = hub;
            _appDomain.UnhandledException += Handle;
        }

        // Internal for testability
#if !NET6_0_OR_GREATER
        [HandleProcessCorruptedStateExceptions]
#endif
        [SecurityCritical]
        internal void Handle(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                ex.Data[Mechanism.HandledKey] = false;
                ex.Data[Mechanism.MechanismKey] = "AppDomain.UnhandledException";
                _ = _hub?.CaptureException(ex);
            }

            if (e.IsTerminating)
            {
                (_hub as IDisposable)?.Dispose();
            }
        }
    }
}
