namespace WhMgr.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using WhMgr.Auth.Models.Discord;
    using WhMgr.Configuration;
    using WhMgr.Extensions;

    [ApiController]
    [Route("/auth/discord")]
    public class DiscordAuthController : ControllerBase
    {
        private readonly ILogger<DiscordAuthController> _logger;

        private readonly bool _enabled;
        private readonly ulong _ownerId;
        private readonly ulong _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;
        private readonly IEnumerable<ulong> _userIds;

        private const string AuthorizationEndpoint = "https://discordapp.com/api/oauth2/authorize";
        private const string TokenEndpoint = "https://discordapp.com/api/oauth2/token";
        private const string UserInformationEndpoint = "https://discordapp.com/api/users/@me";
        private const string UserGuildsInformationEndpoint = "https://discordapp.com/api/users/@me/guilds";
        private const string DefaultScope = "guilds%20identify%20email";

        public DiscordAuthController(ILogger<DiscordAuthController> logger)
        {
            _logger = logger;

            // Load settings from Discord auth config
            var discordAuthConfig = Strings.DiscordAuthFilePath.LoadFromFile<DiscordAuthConfig>();
            _enabled = discordAuthConfig.Enabled;
            _ownerId = discordAuthConfig.OwnerId;
            _clientId = discordAuthConfig?.ClientId ?? 0;
            _clientSecret = discordAuthConfig?.ClientSecret;
            _redirectUri = discordAuthConfig?.RedirectUri;
            _userIds = discordAuthConfig?.UserIds;
        }

        #region Routes

        [HttpGet("login")]
        public IActionResult LoginAsync()
        {
            if (!IsEnabled())
            {
                return Redirect("/dashboard");
            }
            var url = $"{AuthorizationEndpoint}?client_id={_clientId}&scope={DefaultScope}&response_type=code&redirect_uri={_redirectUri}";
            return Redirect(url);
        }

        [HttpGet("logout")]
        public IActionResult LogoutAsync()
        {
            if (!IsEnabled())
            {
                return Redirect("/dashboard");
            }
            HttpContext.Session.Clear();
            HttpContext.Session = null;
            // TODO: Fix destroying sessions
            return Redirect("/discord/login");
        }

        [HttpGet("callback")]
        public IActionResult CallbackAsync()
        {
            var code = Request.Query["code"].ToString();
            if (!IsEnabled())
            {
                return Redirect("/dashboard");
            }
            if (string.IsNullOrEmpty(code))
            {
                // Error
                _logger.LogError($"Authentication code is empty");
                return null;
            }

            var response = SendAuthorize(code);
            if (response == null)
            {
                // Error authorizing
                _logger.LogError($"Failed to authenticate with Discord");
                return null;
            }

            // Successful
            var user = GetUser(response.TokenType, response.AccessToken);
            if (user == null)
            {
                // Failed to get user
                _logger.LogError($"Failed to get user information");
                return null;
            }
            var guilds = GetUserGuilds(response.TokenType, response.AccessToken);
            if (guilds == null)
            {
                // Failed to get user guilds
                _logger.LogError($"Failed to get guilds for user {user.Username} ({user.Id})");
                return null;
            }
            // TODO: Check users table for permissions
            // Validate user is in guild or user id matches
            if (!ulong.TryParse(user.Id, out var userId))
            {
                _logger.Error($"Failed to parse user id: {user.Id}");
                return null;
            }
            var isValid = _userIds.Contains(userId) || _ownerId == userId;
            if (!isValid)
            {
                _logger.LogError($"Unauthorized user tried to authenticate {user.Username} ({user.Id}");
                return Redirect("/auth/discord/login");
            }
            // User authenticated successfully
            _logger.LogInformation($"User {user.Username} ({user.Id}) authenticated successfully");
            HttpContext.Session.SetValue("is_valid", isValid);
            HttpContext.Session.SetValue("user_id", user.Id);
            HttpContext.Session.SetValue("email", user.Email);
            HttpContext.Session.SetValue("username", $"{user.Username}#{user.Discriminator}");
            HttpContext.Session.SetValue("guild_ids", guilds.Select(x => x.Id));
            HttpContext.Session.SetValue("avatar_id", user.Avatar);
            // Check previous page saved if we should redirect to it or the home page
            var redirect = HttpContext.Session.GetValue<string>("last_redirect");
            HttpContext.Session.Remove("last_redirect");
            return Redirect(string.IsNullOrEmpty(redirect)
                ? "/dashboard"
                : redirect
            );
        }

        #endregion

        #region OAuth

        private DiscordAuthResponse SendAuthorize(string authorizationCode)
        {
            using (var wc = new WebClient())
            {
                wc.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                try
                {
                    var result = wc.UploadValues(TokenEndpoint, new NameValueCollection
                    {
                        { "client_id", _clientId.ToString() },
                        { "client_secret", _clientSecret },
                        { "grant_type", "authorization_code" },
                        { "code", authorizationCode },
                        { "redirect_uri", _redirectUri },
                        { "scope", DefaultScope },
                    });
                    var responseJson = Encoding.UTF8.GetString(result);
                    var response = responseJson.FromJson<DiscordAuthResponse>();
                    return response;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        private DiscordUserInfo GetUser(string tokenType, string token)
        {
            var response = SendRequest(UserInformationEndpoint, tokenType, token);
            if (string.IsNullOrEmpty(response))
            {
                _logger.Error($"Failed to get Discord user response");
                return null;
            }
            var user = response.FromJson<DiscordUserInfo>();
            return user;
        }

        private List<DiscordGuildInfo> GetUserGuilds(string tokenType, string token)
        {
            var response = SendRequest(UserGuildsInformationEndpoint, tokenType, token);
            if (string.IsNullOrEmpty(response))
            {
                _logger.Error($"Failed to get Discord user guilds response");
                return null;
            }
            var guilds = response.FromJson<List<DiscordGuildInfo>>();
            return guilds;
        }

        private static string SendRequest(string url, string tokenType, string token)
        {
            // TODO: Retry request x amount of times before failing
            using var wc = new WebClient();
            wc.Proxy = null;
            wc.Headers[HttpRequestHeader.ContentType] = "application/json";
            wc.Headers[HttpRequestHeader.Authorization] = $"{tokenType} {token} ";
            return wc.DownloadString(url);
        }

        #endregion

        private bool IsEnabled()
        {
            return _enabled;/* &&
                _clientId > 0 &&
                !string.IsNullOrEmpty(_clientSecret) &&
                !string.IsNullOrEmpty(_redirectUri);
            */
        }
    }
}