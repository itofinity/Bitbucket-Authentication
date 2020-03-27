using System;

namespace Itofinity.Bitbucket.Authentication.Helpers.Microsoft.Git.CredentialManager
{
    public interface ISettings
    {
        Uri RemoteUri { get; }
        Uri ProxyUri { get; }
        bool IsCertificateVerificationEnabled { get; }
        string OAuthConsumerKey { get; }
        string OAuthConsumerSecret { get; }

        bool GetTracingEnabled(out string traceValue);
    }
}