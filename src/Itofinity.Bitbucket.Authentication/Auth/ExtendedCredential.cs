

using Itofinity.Bitbucket.Authentication.Helpers.Microsoft.Git.CredentialManager;

namespace Itofinity.Bitbucket.Authentication.Auth
{
    public class ExtendedCredential : GitCredential, IExtendedCredential
    {
        public ExtendedCredential(string userName, string password, string scheme) : base(userName, password)
        {
            Scheme = scheme;
        }

        public string Scheme { get; }

        public string Token => Constants.Http.WwwAuthenticateBearerScheme.Equals(Scheme) ? Password : this.ToBase64String();

        public string AuthenticationHeaderValue => $"{Scheme} {Token}";
    }
}