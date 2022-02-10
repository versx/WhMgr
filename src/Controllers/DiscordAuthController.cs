namespace WhMgr.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Web.Auth.Discord.Models;

    [ApiController]
    [Route("/auth/discord/")]
    public class DiscordAuthController : ControllerBase
    {
        private readonly ILogger<DiscordAuthController> _logger;

        private readonly ulong _ownerId;
        private readonly ulong _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;
        private readonly IEnumerable<ulong> _userIds;

        private const string BaseEndpoint = "https://discordapp.com/api";
        private const string AuthorizationEndpoint = BaseEndpoint + "/oauth2/authorize";
        private const string TokenEndpoint = BaseEndpoint + "/oauth2/token";
        private const string UserEndpoint = BaseEndpoint + "/users/@me";
        private const string UserGuildsEndpoint = BaseEndpoint + "/users/@me/guilds";
        private const string UserGuildMemberEndpoint = BaseEndpoint + "/guilds/{0}/members/{1}";
        private const string DefaultScope = "guilds%20identify%20email";

        public DiscordAuthController(ILogger<DiscordAuthController> logger)
        {
            _logger = logger;

            // Load settings from Discord auth config
            var discordAuthConfig = Strings.DiscordAuthFilePath.LoadFromFile<DiscordAuthConfig>();
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
            var url = $"{AuthorizationEndpoint}?client_id={_clientId}&scope={DefaultScope}&response_type=code&redirect_uri={_redirectUri}";
            return Redirect(url);
        }

        [HttpGet("logout")]
        public IActionResult LogoutAsync()
        {
            HttpContext.Session.Clear();
            HttpContext.Session = null;
            // TODO: Fix destroying sessions
            return Redirect("/auth/discord/login");
        }

        [HttpGet("callback")]
        public async Task<IActionResult> CallbackAsync()
        {
            var code = Request.Query["code"].ToString();
            if (string.IsNullOrEmpty(code))
            {
                // Error
                _logger.LogError($"Authentication code is empty");
                return null;
            }

            var response = await SendAuthorize(code);
            if (response == null)
            {
                // Error authorizing
                _logger.LogError($"Failed to authenticate with Discord");
                return null;
            }

            // Successful
            var user = await GetUser(response.TokenType, response.AccessToken);
            if (user == null)
            {
                // Failed to get user
                _logger.LogError($"Failed to get user information");
                return null;
            }
            var guilds = await GetUserGuilds(response.TokenType, response.AccessToken);
            if (guilds == null)
            {
                // Failed to get user guilds
                _logger.LogError($"Failed to get guilds for user {user.Username} ({user.Id})");
                return null;
            }
            foreach (var guild in guilds)
            {
                // TODO: var guildMember = GetGuildMember(response.TokenType, response.AccessToken, guild.Id, user.Id);
                //Console.WriteLine($"Guild member: {guildMember}");
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

        private async Task<DiscordAuthResponse> SendAuthorize(string authorizationCode)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");
                var payload = new
                {
                    client_id = _clientId.ToString(),
                    client_secret = _clientSecret,
                    grant_type = "authorization_code",
                    code = authorizationCode,
                    redirect_uri = _redirectUri,
                    scope = DefaultScope,
                };
                var json = payload.ToJson();
                var response = await client.PostAsync(TokenEndpoint, new StringContent(json));
                var responseString = await response.Content.ReadAsStringAsync();
                return responseString.FromJson<DiscordAuthResponse>();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<DiscordUserInfo> GetUser(string tokenType, string token)
        {
            var response = await SendRequest(UserEndpoint, tokenType, token);
            if (string.IsNullOrEmpty(response))
            {
                _logger.Error($"Failed to get Discord user response");
                return null;
            }
            var user = response.FromJson<DiscordUserInfo>();
            return user;
        }

        private async Task<List<DiscordGuildInfo>> GetUserGuilds(string tokenType, string token)
        {
            var response = await SendRequest(UserGuildsEndpoint, tokenType, token);
            if (string.IsNullOrEmpty(response))
            {
                _logger.Error($"Failed to get Discord user guilds response");
                return null;
            }
            var guilds = response.FromJson<List<DiscordGuildInfo>>();
            // TODO: Loop guilds, call GetGuildMember, return roles list
            return guilds;
        }

        private async Task<DiscordGuildMemberInfo> GetGuildMember(string tokenType, string token, string guildId, string userId)
        {
            var url = string.Format(UserGuildMemberEndpoint, guildId, userId);
            var response = await SendRequest(url, tokenType, token);
            if (string.IsNullOrEmpty(response))
            {
                _logger.Error($"Failed to get Discord member response");
                return null;
            }
            var member = response.FromJson<DiscordGuildMemberInfo>();
            return member;
        }

        private static async Task<string> SendRequest(string url, string tokenType, string token)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Conent-Type", "application/json");
            client.DefaultRequestHeaders.Add("Authorization", $"{tokenType} {token}");
            var response = await client.GetStringAsync(url);
            return response;

            // TODO: Retry request x amount of times before failing
            /*
            using var wc = new WebClient();
            wc.Proxy = null;
            wc.Headers[HttpRequestHeader.ContentType] = "application/json";
            wc.Headers[HttpRequestHeader.Authorization] = $"{tokenType} {token} ";
            return wc.DownloadString(url);
            */
        }

        #endregion
    }
}