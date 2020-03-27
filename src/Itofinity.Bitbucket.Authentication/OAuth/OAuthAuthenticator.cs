using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Itofinity.Bitbucket.Authentication.Auth;
using Itofinity.Bitbucket.Authentication.Rest;
using GitCredCfg  = Itofinity.Bitbucket.Authentication.Helpers.Microsoft.Git.CredentialManager.Constants.GitConfiguration.Credential;
using Itofinity.Bitbucket.Authentication.Helpers.Microsoft.Git.CredentialManager;

namespace Itofinity.Bitbucket.Authentication.OAuth
{
    public class OAuthAuthenticator : IAuthenticator, IBitbucket
    {
        private readonly IBitbucketRestApi _bitbucketServerApi;
        private readonly IBitbucketRestApi _bitbucketApi;
        private readonly ISettings _settings;
        private readonly ITrace _trace;
        private readonly IHttpClientFactory _httpClientFactory;

        public OAuthAuthenticator(ITrace trace, IHttpClientFactory httpClientFactory, ISettings settings)
        {
            Helpers.Microsoft.Git.CredentialManager.EnsureArgument.NotNull(trace, nameof(trace));
            Helpers.Microsoft.Git.CredentialManager.EnsureArgument.NotNull(settings, nameof(settings));
            Helpers.Microsoft.Git.CredentialManager.EnsureArgument.NotNull(httpClientFactory, nameof(httpClientFactory));

            _bitbucketServerApi = new Itofinity.Bitbucket.Authentication.Rest.Server.BitbucketRestApi(trace, httpClientFactory);

            _bitbucketApi = new Itofinity.Bitbucket.Authentication.Rest.Cloud.BitbucketRestApi(trace, httpClientFactory);

            _settings = settings;
            _trace = trace;
            _httpClientFactory = httpClientFactory;

        }

        private IOAuthAuthenticator GetAuthenticator()
        {
            if (IsCloud)
            {
                // bitbucket.org
                return new v2.OAuthAuthenticator(_trace, _httpClientFactory);
            }
            else
            {
                return new v1.OAuthAuthenticator(/*_context,*/ _settings.OAuthConsumerKey, _settings.OAuthConsumerSecret);
            }
        }


        public async Task<AuthenticationResult> AcquireTokenAsync(Uri targetUri, IEnumerable<string> scopes, IExtendedCredential credentials)
        {
            var oauth = GetAuthenticator();
            try
            {
                return await oauth.AcquireTokenAsync(targetUri, scopes, credentials);
            }
            catch (Exception ex)
            {
                _trace.WriteLine($"oauth authentication failed [{ex.Message}]");
                return new AuthenticationResult(AuthenticationResultType.Failure);
            }
        }
        
    
        public bool IsCloud => RemoteUrl.Contains("bitbucket.org");

        public string RemoteUrl => _settings.RemoteUri.AbsoluteUri;

        public async Task<AuthenticationResult> AquireUserDetailsAsync(Uri targetUri, string token)
        {
            if(IsCloud)
            {
                return await _bitbucketApi.AcquireUserDetailsAsync(targetUri, token);
            }
            else
            {
                return await _bitbucketServerApi.AcquireUserDetailsAsync(targetUri, token);
            }
        }
    }
}
