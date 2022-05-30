using System.Security.Cryptography;
using AuthServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.WebEid;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AuthServer.Controllers;

[ApiController]
[Route("signin-web-eid")]
public class WebEidController : ControllerBase
{
    private const string ChallengeNonceKey = "eid.challenge";

    private TimeSpan ChallengeLifetime { get; } = TimeSpan.FromMinutes(5);

    private readonly WebEidValidationService _webEidValidationService;
    private readonly IMemoryCache _cache;

    public WebEidController(WebEidValidationService webEidValidationService, IMemoryCache cache)
    {
        _webEidValidationService = webEidValidationService;
        _cache = cache;
    }

    [HttpGet("challenge")]
    public async Task<IActionResult> GetChallenge()
    {
        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        var nonce = RandomNumberGenerator.GetBytes(64);

        _cache.Set(Convert.ToBase64String(nonce), true, ChallengeLifetime);
        HttpContext.Session.Set(ChallengeNonceKey, nonce);
        HttpContext.Session.Remove(WebEidDefaults.SessionCertKey);

        return Ok(new { nonce });
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromBody] WebIdAuthTokenResponse token)
    {
        // Obtain the challenge from session
        if (!HttpContext.Session.TryGetValue(ChallengeNonceKey, out var nonce) && nonce == null)
            return Unauthorized();

        // Check if token was not used before or expired
        var challenge = Convert.ToBase64String(nonce);
        if (!_cache.TryGetValue(challenge, out _))
            return Unauthorized();

        _cache.Remove(challenge);
        HttpContext.Session.Remove(ChallengeNonceKey);

        // Validate the certificate and signed challenge
        var validationResult = await _webEidValidationService.GetResult(token, nonce);
        if (!validationResult.Success)
            return Forbid();

        // Certificate is valid. Sign in the user
        HttpContext.Session.Set(WebEidDefaults.SessionCertKey, Convert.FromBase64String(token.UnverifiedCertificate));

        return Ok();
    }
}