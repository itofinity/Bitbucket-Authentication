using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Itofinity.Bitbucket.Authentication.Auth
{
    public interface IAuthenticator
    {    
        Task<AuthenticationResult> AcquireTokenAsync(Uri targetUri, IEnumerable<string> scopes, IExtendedCredential credentials);    
    }
}