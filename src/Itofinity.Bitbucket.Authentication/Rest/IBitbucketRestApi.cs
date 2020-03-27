using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Itofinity.Bitbucket.Authentication.Auth;

namespace Itofinity.Bitbucket.Authentication.Rest
{
    public interface IBitbucketRestApi
    {
        Task<AuthenticationResult> AcquireTokenAsync(
            Uri targetUri,
            string username,
            string password,
            string authenticationCode,
            IEnumerable<string> scopes);
            
        Task<AuthenticationResult> AcquireTokenAsync(
            Uri targetUri,
            IExtendedCredential credentials,
            IEnumerable<string> scopes);

        Task<AuthenticationResult> AcquireUserDetailsAsync(Uri targetUri, string token);
    }
}