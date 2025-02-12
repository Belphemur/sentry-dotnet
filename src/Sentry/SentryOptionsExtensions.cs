using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Sentry.Extensibility;
using Sentry.Infrastructure;
using Sentry.Integrations;
using Sentry.Internal;
using Sentry.Internal.Extensions;
#if NET461
using Sentry.PlatformAbstractions;
#endif
#if HAS_DIAGNOSTIC_INTEGRATION
using Sentry.Internals.DiagnosticSource;
#endif
namespace Sentry
{
    /// <summary>
    /// SentryOptions extensions.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SentryOptionsExtensions
    {
        /// <summary>
        /// Disables the strategy to detect duplicate events.
        /// </summary>
        /// <remarks>
        /// In case a second event is being sent out from the same exception, that event will be discarded.
        /// It is possible the second event had in fact more data. In which case it'd be ideal to avoid the first
        /// event going out in the first place.
        /// </remarks>
        /// <param name="options">The SentryOptions to remove the processor from.</param>
        public static void DisableDuplicateEventDetection(this SentryOptions options)
            => options.EventProcessors =
                options.EventProcessors?.Where(p => p.GetType() != typeof(DuplicateEventDetectionEventProcessor)).ToArray();

        /// <summary>
        /// Disables the capture of errors through <see cref="AppDomain.UnhandledException"/>.
        /// </summary>
        /// <param name="options">The SentryOptions to remove the integration from.</param>
        public static void DisableAppDomainUnhandledExceptionCapture(this SentryOptions options) =>
            options.RemoveIntegration<AppDomainUnhandledExceptionIntegration>();

#if HAS_DIAGNOSTIC_INTEGRATION
        /// <summary>
        /// Disables the integrations with Diagnostic source.
        /// </summary>
        /// <param name="options">The SentryOptions to remove the integration from.</param>
        public static void DisableDiagnosticSourceIntegration(this SentryOptions options)
            => options.Integrations =
                options.Integrations?.Where(p => p.GetType() != typeof(SentryDiagnosticListenerIntegration)).ToArray();
#endif

        /// <summary>
        /// Disables the capture of errors through <see cref="TaskScheduler.UnobservedTaskException"/>.
        /// </summary>
        /// <param name="options">The SentryOptions to remove the integration from.</param>
        public static void DisableTaskUnobservedTaskExceptionCapture(this SentryOptions options) =>
            options.RemoveIntegration<TaskUnobservedTaskExceptionIntegration>();

#if NET461
        /// <summary>
        /// Disables the list addition of .Net Frameworks into events.
        /// </summary>
        /// <param name="options">The SentryOptions to remove the integration from.</param>
        public static void DisableNetFxInstallationsIntegration(this SentryOptions options)
        {
            options.EventProcessors =
                options.EventProcessors?.Where(p => p.GetType() != typeof(NetFxInstallationsEventProcessor)).ToArray();
            options.RemoveIntegration<NetFxInstallationsIntegration>();
        }
#endif

        /// <summary>
        /// By default, any queued events (i.e: captures errors) are flushed on <see cref="AppDomain.ProcessExit"/>.
        /// This method disables that behaviour.
        /// </summary>
        /// <param name="options">The SentryOptions to remove the integration from.</param>
        public static void DisableAppDomainProcessExitFlush(this SentryOptions options) =>
            options.RemoveIntegration<AppDomainProcessExitIntegration>();

        /// <summary>
        /// Add an integration
        /// </summary>
        /// <param name="options">The SentryOptions to hold the processor.</param>
        /// <param name="integration">The integration.</param>
        public static void AddIntegration(this SentryOptions options, ISdkIntegration integration)
            => options.Integrations = options.Integrations != null
                ? options.Integrations.Concat(new[] { integration }).ToArray()
                : new[] { integration };

        /// <summary>
        /// Removes all integrations of type <typeparamref name="TIntegration"/>.
        /// </summary>
        /// <typeparam name="TIntegration">The type of the integration(s) to remove.</typeparam>
        /// <param name="options">The SentryOptions to remove the integration(s) from.</param>
        public static void RemoveIntegration<TIntegration>(this SentryOptions options) where TIntegration : ISdkIntegration
            => options.Integrations = options.Integrations?.Where(p => p.GetType() != typeof(TIntegration)).ToArray();

        /// <summary>
        /// Add an exception filter.
        /// </summary>
        /// <param name="options">The SentryOptions to hold the processor.</param>
        /// <param name="exceptionFilter">The exception filter to add.</param>
        public static void AddExceptionFilter(this SentryOptions options, IExceptionFilter exceptionFilter)
            => options.ExceptionFilters = options.ExceptionFilters != null
                ? options.ExceptionFilters.Concat(new[] { exceptionFilter }).ToArray()
                : new[] { exceptionFilter };

        /// <summary>
        /// Ignore exception of type <typeparamref name="TException"/> or derived.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to ignore.</typeparam>
        /// <param name="options">The SentryOptions to store the exceptions type ignore.</param>
        public static void AddExceptionFilterForType<TException>(this SentryOptions options) where TException : Exception
            => options.AddExceptionFilter(new ExceptionTypeFilter<TException>());

        /// <summary>
        /// Add prefix to exclude from 'InApp' stacktrace list.
        /// </summary>
        /// <param name="options">The SentryOptions which holds the stacktrace list.</param>
        /// <param name="prefix">The string used to filter the stacktrace to be excluded from InApp.</param>
        /// <remarks>
        /// Sentry by default filters the stacktrace to display only application code.
        /// A user can optionally click to see all which will include framework and libraries.
        /// A <see cref="string.StartsWith(string)"/> is executed
        /// </remarks>
        /// <example>
        /// 'System.', 'Microsoft.'
        /// </example>
        public static void AddInAppExclude(this SentryOptions options, string prefix)
            => options.InAppExclude = options.InAppExclude != null
                ? options.InAppExclude.Concat(new[] { prefix }).ToArray()
                : new[] { prefix };

        /// <summary>
        /// Add prefix to include as in 'InApp' stacktrace.
        /// </summary>
        /// <param name="options">The SentryOptions which holds the stacktrace list.</param>
        /// <param name="prefix">The string used to filter the stacktrace to be included in InApp.</param>
        /// <remarks>
        /// Sentry by default filters the stacktrace to display only application code.
        /// A user can optionally click to see all which will include framework and libraries.
        /// A <see cref="string.StartsWith(string)"/> is executed
        /// </remarks>
        /// <example>
        /// 'System.CustomNamespace', 'Microsoft.Azure.App'
        /// </example>
        public static void AddInAppInclude(this SentryOptions options, string prefix)
            => options.InAppInclude = options.InAppInclude != null
                ? options.InAppInclude.Concat(new[] { prefix }).ToArray()
                : new[] { prefix };

        /// <summary>
        /// Add an exception processor.
        /// </summary>
        /// <param name="options">The SentryOptions to hold the processor.</param>
        /// <param name="processor">The exception processor.</param>
        public static void AddExceptionProcessor(this SentryOptions options, ISentryEventExceptionProcessor processor)
            => options.ExceptionProcessors = options.ExceptionProcessors != null
                ? options.ExceptionProcessors.Concat(new[] { processor }).ToArray()
                : new[] { processor };

        /// <summary>
        /// Add the exception processors.
        /// </summary>
        /// <param name="options">The SentryOptions to hold the processor.</param>
        /// <param name="processors">The exception processors.</param>
        public static void AddExceptionProcessors(this SentryOptions options, IEnumerable<ISentryEventExceptionProcessor> processors)
            => options.ExceptionProcessors = options.ExceptionProcessors != null
                ? options.ExceptionProcessors.Concat(processors).ToArray()
                : processors.ToArray();

        /// <summary>
        /// Adds an event processor which is invoked when creating a <see cref="SentryEvent"/>.
        /// </summary>
        /// <param name="options">The SentryOptions to hold the processor.</param>
        /// <param name="processor">The event processor.</param>
        public static void AddEventProcessor(this SentryOptions options, ISentryEventProcessor processor)
            => options.EventProcessors = options.EventProcessors != null
                ? options.EventProcessors.Concat(new[] { processor }).ToArray()
                : new[] { processor };

        /// <summary>
        /// Adds event processors which are invoked when creating a <see cref="SentryEvent"/>.
        /// </summary>
        /// <param name="options">The SentryOptions to hold the processor.</param>
        /// <param name="processors">The event processors.</param>
        public static void AddEventProcessors(this SentryOptions options, IEnumerable<ISentryEventProcessor> processors)
            => options.EventProcessors = options.EventProcessors != null
                ? options.EventProcessors.Concat(processors).ToArray()
                : processors.ToArray();

        /// <summary>
        /// Adds an event processor provider which is invoked when creating a <see cref="SentryEvent"/>.
        /// </summary>
        /// <param name="options">The SentryOptions to hold the processor provider.</param>
        /// <param name="processorProvider">The event processor provider.</param>
        public static void AddEventProcessorProvider(this SentryOptions options, Func<IEnumerable<ISentryEventProcessor>> processorProvider)
            => options.EventProcessorsProviders = options.EventProcessorsProviders != null
                ? options.EventProcessorsProviders.Concat(new[] { processorProvider }).ToArray()
                : new[] { processorProvider };

        /// <summary>
        /// Add the exception processor provider.
        /// </summary>
        /// <param name="options">The SentryOptions to hold the processor provider.</param>
        /// <param name="processorProvider">The exception processor provider.</param>
        public static void AddExceptionProcessorProvider(this SentryOptions options,
            Func<IEnumerable<ISentryEventExceptionProcessor>> processorProvider)
            => options.ExceptionProcessorsProviders = options.ExceptionProcessorsProviders != null
                ? options.ExceptionProcessorsProviders.Concat(new[] { processorProvider }).ToArray()
                : new[] { processorProvider };

        /// <summary>
        /// Invokes all event processor providers available.
        /// </summary>
        /// <param name="options">The SentryOptions which holds the processor providers.</param>
        public static IEnumerable<ISentryEventProcessor> GetAllEventProcessors(this SentryOptions options)
            => options.EventProcessorsProviders?.SelectMany(p => p()) ?? Enumerable.Empty<ISentryEventProcessor>();

        /// <summary>
        /// Invokes all exception processor providers available.
        /// </summary>
        /// <param name="options">The SentryOptions which holds the processor providers.</param>
        public static IEnumerable<ISentryEventExceptionProcessor> GetAllExceptionProcessors(this SentryOptions options)
            => options.ExceptionProcessorsProviders?.SelectMany(p => p()) ?? Enumerable.Empty<ISentryEventExceptionProcessor>();

        /// <summary>
        /// Use custom <see cref="ISentryStackTraceFactory" />.
        /// </summary>
        /// <param name="options">The SentryOptions to hold the processor provider.</param>
        /// <param name="sentryStackTraceFactory">The stack trace factory.</param>
        public static SentryOptions UseStackTraceFactory(this SentryOptions options, ISentryStackTraceFactory sentryStackTraceFactory)
        {
            options.SentryStackTraceFactory = sentryStackTraceFactory ?? throw new ArgumentNullException(nameof(sentryStackTraceFactory));

            return options;
        }

        /// <summary>
        /// Applies the default tags to an event without resetting existing tags.
        /// </summary>
        /// <param name="options">The options to read the default tags from.</param>
        /// <param name="hasTags">The event to apply the tags to.</param>
        public static void ApplyDefaultTags(this SentryOptions options, IHasTags hasTags)
        {
            foreach (var defaultTag in options.DefaultTags
                .Where(t => !hasTags.Tags.TryGetValue(t.Key, out _)))
            {
                hasTags.SetTag(defaultTag.Key, defaultTag.Value);
            }
        }

        internal static void SetupLogging(this SentryOptions options)
        {
            if (options.Debug)
            {
                if (options.DiagnosticLogger == null)
                {
                    options.DiagnosticLogger = new ConsoleDiagnosticLogger(options.DiagnosticLevel);
                    options.DiagnosticLogger.LogDebug("Logging enabled with ConsoleDiagnosticLogger and min level: {0}",
                        options.DiagnosticLevel);
                }
            }
            else
            {
                options.DiagnosticLogger = null;
            }
        }

        internal static string? TryGetDsnSpecificCacheDirectoryPath(this SentryOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.CacheDirectoryPath))
            {
                return null;
            }

            // DSN must be set to use caching
            var dsn = options.Dsn;
            if (string.IsNullOrWhiteSpace(dsn))
            {
                return null;
            }

            return Path.Combine(options.CacheDirectoryPath, "Sentry", dsn.GetHashString());
        }

        internal static string? TryGetProcessSpecificCacheDirectoryPath(this SentryOptions options)
        {
            // In the future, this will most likely contain process ID
            return options.TryGetDsnSpecificCacheDirectoryPath();
        }
    }
}
