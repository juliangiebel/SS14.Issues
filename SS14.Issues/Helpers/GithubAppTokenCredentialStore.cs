using GitHubJwt;
using Octokit;

namespace SS14.Issues.Helpers;

/// <summary>
/// Github App credential store that automatically renews the credentials if they have expired.
/// </summary>
public sealed class GithubAppTokenCredentialStore : ICredentialStore
{
    private const int TokenExpiration = 500;
    
    private readonly GitHubJwtFactory _tokenGenerator;

    private string? _token;
    private DateTime? _tokenCreation;

    public GithubAppTokenCredentialStore(string keyFileLocation, int appId)
    {
        _tokenGenerator = new GitHubJwtFactory(
            new FilePrivateKeySource(keyFileLocation),
            new GitHubJwtFactoryOptions()
            {
                AppIntegrationId = appId,
                ExpirationSeconds = TokenExpiration
            }
        );
    }

    public async Task<Credentials> GetCredentials()
    {
        return await CreateCredentials();
    }

    private async Task<Credentials> CreateCredentials()
    {
        if (_token != null && _tokenCreation.HasValue && _tokenCreation.Value.AddSeconds(TokenExpiration).CompareTo(DateTime.Now) > 0)
            return new Credentials(_token, AuthenticationType.Bearer);

        _token = _tokenGenerator.CreateEncodedJwtToken();
        _tokenCreation = DateTime.Now;

        return new Credentials(_token, AuthenticationType.Bearer);
    }
}