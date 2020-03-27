using System;

namespace Itofinity.Bitbucket.Authentication.Helpers.Microsoft.Git.CredentialManager
{
    public class Settings : DisposableObject, ISettings
    {
        public Settings(Uri remoteUri, Uri proxyUri, bool isCertificateVerificationEnabled, string oAuthConsumerKey, string oAuthConsumerSecret, string traceConfig)
        {
            Init(remoteUri, proxyUri, isCertificateVerificationEnabled, oAuthConsumerKey, oAuthConsumerSecret, traceConfig);
        }

        public Settings(string remoteUrl, string proxyUrl, string isCertificateVerificationEnabled, string oAuthConsumerKey, string oAuthConsumerSecret, string traceConfig)
        {
            Init(new Uri(remoteUrl), null == proxyUrl ? null : new Uri(proxyUrl), Boolean.Parse(isCertificateVerificationEnabled), oAuthConsumerKey, oAuthConsumerSecret, traceConfig);
        }

        private void Init(Uri remoteUri, Uri proxyUri, bool isCertificateVerificationEnabled, string oAuthConsumerKey, string oAuthConsumerSecret, string traceConfig)
        {
            EnsureArgument.NotNull(remoteUri, nameof(remoteUri));
            RemoteUri = remoteUri;

            ProxyUri = proxyUri;

            IsCertificateVerificationEnabled = isCertificateVerificationEnabled;

            OAuthConsumerKey = oAuthConsumerKey;
            OAuthConsumerSecret = oAuthConsumerSecret;

            Trace = traceConfig;
        }

        public bool GetTracingEnabled(out string traceValue)
        {
            traceValue = Trace;
            return null != traceValue;
        }

        public Uri RemoteUri { get; private set; }
        public Uri ProxyUri { get; private set; }
        public bool IsCertificateVerificationEnabled { get; private set; }

        public string OAuthConsumerKey { get; private set; }

        public string OAuthConsumerSecret { get; private set; }

        public string Trace { get; private set; }
    }
}