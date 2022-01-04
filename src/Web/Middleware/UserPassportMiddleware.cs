namespace WhMgr.Web.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    using WhMgr.Extensions;

    public class UserPassportMiddleware
    {
        private readonly RequestDelegate _next;

        public UserPassportMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var session = httpContext.Session;
            session.SetValue("passport", new UserPassport
            {
                IsValid = session.GetValue<bool>("is_valid"),
                UserId = session.GetValue<string>("user_id"),
                Email = session.GetValue<string>("email"),
                Username = session.GetValue<string>("username"),
                GuildIds = session.GetValue<List<string>>("guild_ids"),
                AvatarId = session.GetValue<string>("avatar_id"),
            });
            await _next(httpContext).ConfigureAwait(false);
        }
    }

    public class UserPassport
    {
        [JsonPropertyName("is_valid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("guild_ids")]
        public List<string> GuildIds { get; set; }

        [JsonPropertyName("avatar_id")]
        public string AvatarId { get; set; }
    }
}