namespace Itofinity.Bitbucket.Authentication
{
    public static class BitbucketConstants
    {
        public const string BitbucketBaseUrlHost = "bitbucket.org";
        public const string BitbucketBaseUrl = "https://bitbucket.org";

        public const string AuthHelperName = "Atlassian.Authentication.Helper";

        /// <summary>
        /// The Bitbucket required HTTP accepts header value
        /// </summary>
        public const string BitbucketApiAcceptsHeaderValue = "application/vnd.github.v3+json";
        public const string BitbucketOptHeader = "X-GitHub-OTP";

        public static class TokenScopes
        {
            // TODO needed?
            public const string SnippetWrite = "snippet:write";
            public const string RepositoryWrite = "repository:write";
        }

        public static readonly string[] BitbucketCredentialScopes =
        {
            BitbucketConstants.TokenScopes.SnippetWrite,
            BitbucketConstants.TokenScopes.RepositoryWrite
        };

        public const string UserNameFromJsonRegexCommand = @"\s*""username"":\s*""([A-Za-z]+)"".*";
        
    }
}
