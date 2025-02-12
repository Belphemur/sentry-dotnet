namespace Sentry.Internal
{
    /// <summary>
    /// Internal Constant Values.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// Sentry DSN environment variable.
        /// </summary>
        public const string DsnEnvironmentVariable = "SENTRY_DSN";

        /// <summary>
        /// Sentry release environment variable.
        /// </summary>
        public const string ReleaseEnvironmentVariable = "SENTRY_RELEASE";

        /// <summary>
        /// Sentry environment, environment variable.
        /// </summary>
        public const string EnvironmentEnvironmentVariable = "SENTRY_ENVIRONMENT";

        /// <summary>
        /// Default Sentry environment setting.
        /// </summary>
        /// <remarks>Best Sentry practice is to always try and have a value for this setting.</remarks>
        public const string ProductionEnvironmentSetting = "production";

        public const string StagingEnvironmentSetting = "staging";

        public const string DevelopmentEnvironmentSetting = "development";

        public const string DebugEnvironmentSetting = "debug";

        // See: https://github.com/getsentry/sentry-release-registry
#if ANDROID
        public const string SdkName = "sentry.dotnet.android";
#else
        public const string SdkName = "sentry.dotnet";
#endif
    }
}
