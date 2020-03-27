using Itofinity.Bitbucket.Authentication.Helpers.Microsoft.Git.CredentialManager;

namespace Itofinity.Bitbucket.Authentication.Auth
{
    public interface IExtendedCredential : ICredential
    {
         string Scheme { get; }

        string Token { get; }

        string AuthenticationHeaderValue { get; }
    }
}