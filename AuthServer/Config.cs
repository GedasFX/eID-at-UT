using Duende.IdentityServer.Models;

namespace AuthServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<Client> GetClients(IEnumerable<AppClient> clients) =>
        clients.Select(c => new Client
        {
            ClientId = c.ClientId,
            ClientSecrets = { new Secret(c.ClientSecret.Sha256()) },

            AllowedGrantTypes = GrantTypes.Code,

            RedirectUris = { c.RedirectUri },

            AllowedScopes = { "openid", "profile" },
        });
}

public record AppClient
{
    public string ClientId { get; init; } = null!;
    public string ClientSecret { get; init; } = null!;

    public string RedirectUri { get; init; } = null!;
}