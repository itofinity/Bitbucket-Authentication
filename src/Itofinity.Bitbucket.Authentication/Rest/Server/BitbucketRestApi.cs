using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Itofinity.Bitbucket.Authentication.Auth;
using static System.StringComparer;

using Itofinity.Bitbucket.Authentication.Helpers.Microsoft.Git.CredentialManager;

namespace Itofinity.Bitbucket.Authentication.Rest.Server
{
    public class BitbucketRestApi : IBitbucketRestApi
    {
        /// <summary>
        /// The maximum wait time for a network request before timing out
        /// </summary>
        private const int RequestTimeout = 15 * 1000; // 15 second limit
        internal static readonly Regex UsernameRegex = new Regex(@"\s*""username""\s*:\s*""([^""]+)""\s*", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private ITrace _trace;
        private IHttpClientFactory _httpClientFactory;

        public BitbucketRestApi(ITrace trace, IHttpClientFactory httpClientFactory)
        {
            EnsureArgument.NotNull(trace, nameof(trace));
            EnsureArgument.NotNull(httpClientFactory, nameof(httpClientFactory));

            this._trace = trace;
            this._httpClientFactory = httpClientFactory;
        }


        public async Task<AuthenticationResult> AcquireTokenAsync(Uri targetUri, string username, string password, string authenticationCode, IEnumerable<string> scopes)
        {
            return await AcquireTokenAsync(targetUri, new ExtendedCredential(username, password, Constants.Http.WwwAuthenticateBasicScheme), scopes);
        }

        public async Task<AuthenticationResult> AcquireTokenAsync(Uri targetUri, IExtendedCredential credentials, IEnumerable<string> scopes)
        {
            Uri requestUri = GetAuthenticationRequestUri(targetUri, credentials.UserName);

            _trace.WriteLine($"HTTP: POST {requestUri}");
            using (HttpContent content = GetTokenJsonContent(targetUri, scopes))
            using (var request = new HttpRequestMessage(HttpMethod.Put, requestUri))
            {
                // set content
                request.Content = content;
                // Set the auth header
                request.Headers.Authorization = new AuthenticationHeaderValue(credentials.Scheme, credentials.Token);
                request.Headers.Add("Accept", "*/*");

                using (var response = await HttpClient.SendAsync(request))
                {
                    // TODO logging Trace.WriteLine($"server responded with {response.StatusCode}.");

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                        case HttpStatusCode.Created:
                            return await ParseSuccessResponseAsync(targetUri, response);
                        case HttpStatusCode.Forbidden:
                            {
                                // Bitbucket Cloud
                                // A 403/Forbidden response indicates the username/password are
                                // recognized and good but 2FA is on in which case we want to
                                // indicate that with the TwoFactor result
                                // TODO logging Trace.WriteLine("two-factor app authentication code required");
                                return new AuthenticationResult(AuthenticationResultType.TwoFactor);
                            }
                        case HttpStatusCode.Unauthorized:
                            {
                                // username or password are wrong.
                                // TODO logging Trace.WriteLine("authentication unauthorized");
                                return new AuthenticationResult(AuthenticationResultType.Failure);
                            }

                        default:
                            // any unexpected result can be treated as a failure.
                            // TODO logging Trace.WriteLine("authentication failed");
                            string responseText = await response.Content.ReadAsStringAsync();
                            return new AuthenticationResult(AuthenticationResultType.Failure);
                    }
                }
            }
        }

        private async Task<AuthenticationResult> ParseSuccessResponseAsync(Uri targetUri, HttpResponseMessage response)
        {
            IExtendedCredential token = null;
            string responseText = await response.Content.ReadAsStringAsync();

            Match tokenMatch;
            // TODO use compiled regex, switch regex based on Uri Bbc vs BbS
            if ((tokenMatch = Regex.Match(responseText, BitbucketServerConstants.PersonalAccessTokenRegexCommand
            ,
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)).Success
                && tokenMatch.Groups.Count > 2)
            {
                var userName = tokenMatch.Groups[1].Value;
                string tokenText = tokenMatch.Groups[2].Value;
                token = new BearerCredential(userName, tokenText);
            }

            if (token == null)
            {
                _trace.WriteLine($"Authentication for '{targetUri}' failed.");
                return new AuthenticationResult(AuthenticationResultType.Failure);
            }
            else
            {
                _trace.WriteLine($"Authentication success: new personal access token for '{targetUri}' created.");
                return new AuthenticationResult(AuthenticationResultType.Success, token);
            }
        }

        private HttpContent GetTokenJsonContent(Uri targetUri, IEnumerable<string> scopes)
        {
            const string HttpJsonContentType = "application/json";
            const string JsonContentFormat = @"{{ ""name"": ""git: {1} on {2} at {3:dd-MMM-yyyy HH:mm}"", ""permissions"": {0} }}";

            var quotedScopes = scopes.Select(x => $"\"{x}\"");
            string scopesJson = $"[{string.Join(", ", quotedScopes)}]";

            string jsonContent = string.Format(JsonContentFormat, scopesJson, targetUri, Environment.MachineName, DateTime.Now);

            return new StringContent(jsonContent, Encoding.UTF8, HttpJsonContentType);
        }

        private Uri GetAuthenticationRequestUri(Uri targetUri, string userName)
        {
            if (targetUri.DnsSafeHost.Equals(BitbucketConstants.BitbucketBaseUrlHost, StringComparison.OrdinalIgnoreCase))
            {
                return new Uri("https://api.github.com/authorizations");
            }
            else
            {
                // If we're here, it's Bitbucket Server via a configured provider/authority
                var baseUrl = targetUri.AbsoluteUri; // TODO ? targetUri.GetLeftPart(UriPartial.Authority);
                return new Uri(baseUrl + $"/rest/access-tokens/1.0/users/{userName}");
            }
        }

        private async Task<string> GetContentAsString(HttpContent content)
        {
            if (content is null)
                throw new ArgumentNullException(nameof(content));

            string asString = null;

            if (content.Headers.ContentType?.MediaType != null
                && (content.Headers.ContentType.MediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
                    || content.Headers.ContentType.MediaType.EndsWith("/json", StringComparison.OrdinalIgnoreCase)))
            {


                if (content.Headers.ContentEncoding.Any(e => OrdinalIgnoreCase.Equals("gzip", e)))
                {
                    using (var stream = await content.ReadAsStreamAsync())
                    using (var inflate = new GZipStream(stream, CompressionMode.Decompress))
                    using (var reader = new StreamReader(inflate, Encoding.UTF8))
                    {
                        asString = reader.ReadToEnd();
                    }
                }
                else if (content.Headers.ContentEncoding.Any(e => OrdinalIgnoreCase.Equals("deflate", e)))
                {
                    using (var stream = await content.ReadAsStreamAsync())
                    using (var inflate = new DeflateStream(stream, CompressionMode.Decompress))
                    using (var reader = new StreamReader(inflate, Encoding.UTF8))
                    {
                        asString = reader.ReadToEnd();
                    }
                }
                else
                {
                    asString = await content.ReadAsStringAsync();
                }
            }

            return asString;
        }

        #region private
        private HttpClient _httpClient;
        private HttpClient HttpClient
        {
            get
            {
                if (_httpClient is null)
                {
                    _httpClient = _httpClientFactory.CreateClient();

                    // Set the common headers and timeout
                    _httpClient.Timeout = TimeSpan.FromMilliseconds(RequestTimeout);
                    _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(BitbucketConstants.BitbucketApiAcceptsHeaderValue));
                }

                return _httpClient;
            }
        }

        public Task<AuthenticationResult> AcquireUserDetailsAsync(Uri targetUri, string token)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
        
        #endregion
    }
}