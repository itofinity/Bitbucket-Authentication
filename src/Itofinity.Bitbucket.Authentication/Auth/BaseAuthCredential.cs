using Itofinity.Bitbucket.Authentication.Helpers.Microsoft.Git.CredentialManager;

namespace Itofinity.Bitbucket.Authentication.Auth
{
    public class BaseAuthCredential : ExtendedCredential
    {
        public BaseAuthCredential(string userName, string password) : base(userName, password, Constants.Http.WwwAuthenticateBasicScheme)
        {
        }
    }
}