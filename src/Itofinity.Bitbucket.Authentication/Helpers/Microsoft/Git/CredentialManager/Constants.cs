using System.Diagnostics;
using System.Reflection;

namespace Itofinity.Bitbucket.Authentication.Helpers.Microsoft.Git.CredentialManager
{
    // Lifted from Microsoft.Git.CredentialManager.Constants
    public static class Constants {
        
        public static class Http
        {
            public const string WwwAuthenticateBasicScheme     = "Basic";
            public const string WwwAuthenticateBearerScheme    = "Bearer";
            public const string WwwAuthenticateNegotiateScheme = "Negotiate";
            public const string WwwAuthenticateNtlmScheme      = "NTLM";

            public const string MimeTypeJson = "application/json";
        }
        public static class GitConfiguration
        {
            public static class Credential
            {
                public const string SectionName = "credential";
                public const string Helper      = "helper";
                public const string Provider    = "provider";
                public const string Authority   = "authority";
                public const string AllowWia    = "allowWindowsAuth";
                public const string HttpProxy   = "httpProxy";
                public const string HttpsProxy  = "httpsProxy";
                public const string UseHttpPath = "useHttpPath";
                public const string AuthGuiHelperPaths = "guiHelperPaths";
            }

            public static class Http
            {
                public const string SectionName = "http";
                public const string Proxy = "proxy";
                public const string SslVerify = "sslVerify";
            }
        }

        public static class HelpUrls
        {
            public const string GcmProjectUrl          = "https://aka.ms/gcmcore";
            public const string GcmAuthorityDeprecated = "https://aka.ms/gcmcore-authority";
            public const string GcmHttpProxyGuide      = "https://aka.ms/gcmcore-httpproxy";
            public const string GcmTlsVerification     = "https://aka.ms/gcmcore-tlsverify";
        }

        private static string _gcmVersion;

        /// <summary>
        /// The current version of Git Credential Manager.
        /// </summary>
        public static string GcmVersion
        {
            get
            {
                if (_gcmVersion is null)
                {
                    _gcmVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
                }

                return _gcmVersion;
            }
        }

        private static string _gcmName;

        /// <summary>
        /// The current version of Git Credential Manager.
        /// </summary>
        public static string GcmName
        {
            get
            {
                if (_gcmName is null)
                {
                    _gcmName = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName;
                }

                return _gcmName;
            }
        }

        /// <summary>
        /// Get the HTTP user-agent for Git Credential Manager.
        /// </summary>
        /// <returns>User-agent string for HTTP requests.</returns>
        public static string GetHttpUserAgent()
        {
            //PlatformInformation info = PlatformUtils.GetPlatformInformation();
            //string osType     = info.OperatingSystemType;
            //string cpuArch    = info.CpuArchitecture;
            //string clrVersion = info.ClrVersion;e
            return string.Format($"{GcmName}/{GcmVersion}");
        }
    }
}