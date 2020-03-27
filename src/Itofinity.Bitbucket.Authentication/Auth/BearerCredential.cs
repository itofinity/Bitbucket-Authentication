using Itofinity.Bitbucket.Authentication.Helpers.Microsoft.Git.CredentialManager;

namespace Itofinity.Bitbucket.Authentication.Auth
{
    public class BearerCredential : ExtendedCredential
    {
        public BearerCredential(string userName, string password) : base(userName, password, Constants.Http.WwwAuthenticateBearerScheme)
        {
        }
    }
}