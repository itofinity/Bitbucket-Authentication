using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Itofinity.Bitbucket.Authentication.Auth;
using Itofinity.Bitbucket.Authentication.Helpers.Microsoft.Git.CredentialManager;

namespace Itofinity.Bitbucket.Authentication.OAuth.v1
{
    public class OAuthAuthenticator : IOAuthAuthenticator
    {
        public OAuthAuthenticator(string consumerKey, string consumerSecret)
        {
        }

        public Task<AuthenticationResult> AcquireTokenAsync(Uri targetUri, IEnumerable<string> scopes, IExtendedCredential credentials)
        {
            throw new Exception();
        }

        public Task<AuthenticationResult> RefreshAuthAsync(Uri targetUri, string refreshToken, CancellationToken cancellationToken)
        {
            throw new Exception();
        }

        public Task<AuthenticationResult> GetAuthAsync(Uri targetUri, IEnumerable<string> scopes, CancellationToken cancellationToken)
        {
            throw new Exception();
        }

        public Task<AuthenticationResult> Authenticate(string restRootUrl, Uri targetUri, GitCredential credentials, IEnumerable<string> scopes)
        {
            throw new Exception();
        }
    }
}