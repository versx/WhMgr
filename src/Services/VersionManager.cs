namespace WhMgr.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.Json.Serialization;

    using WhMgr.Extensions;

    public class VersionManager
    {
        private const string BaseUrl = "https://github.com";
        private const string ApiBaseUrl = "https://api.github.com";

        public string AuthorRepository { get; set; }

        public string Version { get; private set; }

        public string Commit { get; private set; }

        public string Url { get; private set; }

        public IReadOnlyList<TagsResponse> Tags { get; private set; }

        // UserAgent

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authorRepo">author/repository_name</param>
        public VersionManager(string authorRepo)
        {
            AuthorRepository = authorRepo;
            Initialize();
        }

        public VersionManager(string authorRepo, string commit, string url, string version)
        {
            AuthorRepository = authorRepo;
            Commit = commit;
            Url = url;
            Version = version;
        }

        public void Initialize()
        {
            var sha = string.Empty;
            var version = string.Empty;
            var pullRequest = string.Empty;
            try
            {
                var shaFilePath = Path.Combine(Directory.GetCurrentDirectory(), "../.gitsha");
                sha = File.ReadAllLines(shaFilePath).FirstOrDefault().Trim(' ');
            }
            catch (Exception ex)
            {
                sha = "?";
                Console.WriteLine($"[Error] Failed to read .gitsha: {ex}");
            }
            try
            {
                var refFile = Path.Combine(Directory.GetCurrentDirectory(), "../.gitref");
                var refData = File.ReadAllLines(refFile).FirstOrDefault().Trim(' ');
                if (refData.StartsWith("refs/pull/") && refData.Contains("/merge"))
                {
                    pullRequest = refData.Replace("refs/pull/", null)
                                         .Replace("/merge", null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to read .gitref: {ex}");
            }
            if (string.IsNullOrEmpty(pullRequest))
            {
                var url = $"{ApiBaseUrl}/repos/{AuthorRepository}/tags";
                using (var wc = new WebClient())
                {
                    wc.Proxy = null;
                    wc.Headers.Add(HttpRequestHeader.UserAgent, "VersionManager");
                    try
                    {
                        var response = wc.DownloadString(url);
                        if (string.IsNullOrEmpty(response))
                        {
                            // Failed to get tags
                            return;
                        }
                        Tags = response.FromJson<List<TagsResponse>>();
                        var tag = Tags.FirstOrDefault(tag => string.Compare(tag.Commit.Sha, sha, true) == 0);
                        version = tag != null ? $"Version {tag.Name}" : "?";
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex}");
                    }
                }
                Url = $"{BaseUrl}/{AuthorRepository}/releases";
            }
            else
            {
                version = $"Pull Request #{pullRequest}";
                Url = $"{BaseUrl}/{AuthorRepository}/pull/{pullRequest}";
            }

            Version = version;
            Commit = sha;

            Console.WriteLine($"[VersionManager] {version} ({sha})");
        }

        public VersionManager GetVersion()
        {
            var sha = string.Empty;
            var version = string.Empty;
            var pullRequest = string.Empty;
            try
            {
                var shaFilePath = Path.Combine(Directory.GetCurrentDirectory(), "../.gitsha");
                sha = File.ReadAllLines(shaFilePath).FirstOrDefault().Trim(' ');
            }
            catch (Exception)
            {
                sha = "?";
                Console.WriteLine($"[Error] Failed to read .gitsha");
            }
            try
            {
                var refFile = Path.Combine(Directory.GetCurrentDirectory(), "../.gitref");
                var refData = File.ReadAllLines(refFile).FirstOrDefault().Trim(' ');
                if (refData.StartsWith("refs/pull/") && refData.Contains("/merge"))
                {
                    pullRequest = refData.Replace("refs/pull/", null)
                                         .Replace("/merge", null);
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"[Error] Failed to read .gitref");
            }
            return new VersionManager(AuthorRepository)
            {
                Commit = sha,
                Url = pullRequest,
                Version = version,
            };
        }
    }

    public class RepositoryOwner
    {
        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        [JsonPropertyName("login")]
        public string Login { get; set; }

        [JsonPropertyName("node_id")]
        public string NodeId { get; set; }

        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonPropertyName("gravatar_id")]
        public string GravatarId { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; }

        [JsonPropertyName("followers_url")]
        public string FollowersUrl { get; set; }

        [JsonPropertyName("gists_url")]
        public string GistsUrl { get; set; }

        [JsonPropertyName("starred_url")]
        public string StarredUrl { get; set; }

        [JsonPropertyName("subscriptions_url")]
        public string SubscriptionsUrl { get; set; }

        [JsonPropertyName("organizations_url")]
        public string OrganizationsUrl { get; set; }

        [JsonPropertyName("repos_url")]
        public string RepositoriesUrl { get; set; }

        [JsonPropertyName("events_url")]
        public string EventsUrl { get; set; }

        [JsonPropertyName("received_events_url")]
        public string ReceivedEventsUrl { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("site_admin")]
        public string IsSiteAdmin { get; set; }
    }

    public class RepositoryResponse
    {
        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        [JsonPropertyName("node_id")]
        public string NodeId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("private")]
        public bool IsRepositoryPrivate { get; set; }

        [JsonPropertyName("owner")]
        public RepositoryOwner Owner { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("fork")]
        public bool IsFork { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("forks_url")]
        public string ForksUrl { get; set; }

        [JsonPropertyName("keys_url")]
        public string KeysUrl { get; set; }

        [JsonPropertyName("collaborators_url")]
        public string CollaboratorsUrl { get; set; }

        [JsonPropertyName("teams_url")]
        public string TeamsUrl { get; set; }

        [JsonPropertyName("hooks_url")]
        public string HooksUrl { get; set; }
    }

    /*
{
  "id": 153759581,
  "node_id": "MDEwOlJlcG9zaXRvcnkxNTM3NTk1ODE=",
  "name": "WhMgr",
  "full_name": "versx/WhMgr",
  "private": false,
  "owner": {
    "login": "versx",
    "id": 1327440,
    "node_id": "MDQ6VXNlcjEzMjc0NDA=",
    "avatar_url": "https://avatars.githubusercontent.com/u/1327440?v=4",
    "gravatar_id": "",
    "url": "https://api.github.com/users/versx",
    "html_url": "https://github.com/versx",
    "followers_url": "https://api.github.com/users/versx/followers",
    "following_url": "https://api.github.com/users/versx/following{/other_user}",
    "gists_url": "https://api.github.com/users/versx/gists{/gist_id}",
    "starred_url": "https://api.github.com/users/versx/starred{/owner}{/repo}",
    "subscriptions_url": "https://api.github.com/users/versx/subscriptions",
    "organizations_url": "https://api.github.com/users/versx/orgs",
    "repos_url": "https://api.github.com/users/versx/repos",
    "events_url": "https://api.github.com/users/versx/events{/privacy}",
    "received_events_url": "https://api.github.com/users/versx/received_events",
    "type": "User",
    "site_admin": false
  },
  "html_url": "https://github.com/versx/WhMgr",
  "description": "Discord notification system that works with RealDeviceMap and reports Pokemon, Raids, Eggs, Quests, Pokestop Lures, Team Rocket Invasions, Gym team changes, and Weather as embed messages. Discord users can also subscribe to custom Pokemon, Raid, Quest, Team Rocket Invasion, and Pokestop Lure notifications via direct message (DM) with predefined requirements.",
  "fork": false,
  "url": "https://api.github.com/repos/versx/WhMgr",
  "forks_url": "https://api.github.com/repos/versx/WhMgr/forks",
  "keys_url": "https://api.github.com/repos/versx/WhMgr/keys{/key_id}",
  "collaborators_url": "https://api.github.com/repos/versx/WhMgr/collaborators{/collaborator}",
  "teams_url": "https://api.github.com/repos/versx/WhMgr/teams",
  "hooks_url": "https://api.github.com/repos/versx/WhMgr/hooks",
  "issue_events_url": "https://api.github.com/repos/versx/WhMgr/issues/events{/number}",
  "events_url": "https://api.github.com/repos/versx/WhMgr/events",
  "assignees_url": "https://api.github.com/repos/versx/WhMgr/assignees{/user}",
  "branches_url": "https://api.github.com/repos/versx/WhMgr/branches{/branch}",
  "tags_url": "https://api.github.com/repos/versx/WhMgr/tags",
  "blobs_url": "https://api.github.com/repos/versx/WhMgr/git/blobs{/sha}",
  "git_tags_url": "https://api.github.com/repos/versx/WhMgr/git/tags{/sha}",
  "git_refs_url": "https://api.github.com/repos/versx/WhMgr/git/refs{/sha}",
  "trees_url": "https://api.github.com/repos/versx/WhMgr/git/trees{/sha}",
  "statuses_url": "https://api.github.com/repos/versx/WhMgr/statuses/{sha}",
  "languages_url": "https://api.github.com/repos/versx/WhMgr/languages",
  "stargazers_url": "https://api.github.com/repos/versx/WhMgr/stargazers",
  "contributors_url": "https://api.github.com/repos/versx/WhMgr/contributors",
  "subscribers_url": "https://api.github.com/repos/versx/WhMgr/subscribers",
  "subscription_url": "https://api.github.com/repos/versx/WhMgr/subscription",
  "commits_url": "https://api.github.com/repos/versx/WhMgr/commits{/sha}",
  "git_commits_url": "https://api.github.com/repos/versx/WhMgr/git/commits{/sha}",
  "comments_url": "https://api.github.com/repos/versx/WhMgr/comments{/number}",
  "issue_comment_url": "https://api.github.com/repos/versx/WhMgr/issues/comments{/number}",
  "contents_url": "https://api.github.com/repos/versx/WhMgr/contents/{+path}",
  "compare_url": "https://api.github.com/repos/versx/WhMgr/compare/{base}...{head}",
  "merges_url": "https://api.github.com/repos/versx/WhMgr/merges",
  "archive_url": "https://api.github.com/repos/versx/WhMgr/{archive_format}{/ref}",
  "downloads_url": "https://api.github.com/repos/versx/WhMgr/downloads",
  "issues_url": "https://api.github.com/repos/versx/WhMgr/issues{/number}",
  "pulls_url": "https://api.github.com/repos/versx/WhMgr/pulls{/number}",
  "milestones_url": "https://api.github.com/repos/versx/WhMgr/milestones{/number}",
  "notifications_url": "https://api.github.com/repos/versx/WhMgr/notifications{?since,all,participating}",
  "labels_url": "https://api.github.com/repos/versx/WhMgr/labels{/name}",
  "releases_url": "https://api.github.com/repos/versx/WhMgr/releases{/id}",
  "deployments_url": "https://api.github.com/repos/versx/WhMgr/deployments",
  "created_at": "2018-10-19T09:34:02Z",
  "updated_at": "2021-08-27T00:01:57Z",
  "pushed_at": "2021-10-11T14:49:05Z",
  "git_url": "git://github.com/versx/WhMgr.git",
  "ssh_url": "git@github.com:versx/WhMgr.git",
  "clone_url": "https://github.com/versx/WhMgr.git",
  "svn_url": "https://github.com/versx/WhMgr",
  "homepage": "",
  "size": 11018,
  "stargazers_count": 24,
  "watchers_count": 24,
  "language": "C#",
  "has_issues": true,
  "has_projects": true,
  "has_downloads": true,
  "has_wiki": true,
  "has_pages": false,
  "forks_count": 26,
  "mirror_url": null,
  "archived": false,
  "disabled": false,
  "open_issues_count": 32,
  "license": null,
  "allow_forking": true,
  "is_template": false,
  "topics": [

  ],
  "visibility": "public",
  "forks": 26,
  "open_issues": 32,
  "watchers": 24,
  "default_branch": "master",
  "temp_clone_token": null,
  "network_count": 26,
  "subscribers_count": 6
}
     */

    public class TagsResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("commit")]
        public Commit Commit { get; set; }

        [JsonPropertyName("node_id")]
        public string NodeId { get; set; }

        [JsonPropertyName("zipball_url")]
        public string ZipballUrl { get; set; }

        [JsonPropertyName("tarball_url")]
        public string TarballUrl { get; set; }
    }

    public class Commit
    {
        [JsonPropertyName("sha")]
        public string Sha { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}