#nullable disable
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using NHttp;
using PhipseyyBot.Common.Db.Extensions;
using PhipseyyBot.Common.Embeds;
using PhipseyyBot.Common.Exceptions;
using PhipseyyBot.Common.Modules;
using PhipseyyBot.Common.Services;
using PhipseyyBot.Discord.Services.PubSub;
using Serilog;
using TwitchLib.Api;
using TwitchLib.Api.Core.Enums;


namespace PhipseyyBot.Discord.Modules.Commands;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[RequireUserPermission(GuildPermission.Administrator)]
[Group("twitch", "Twitch Commands")]
[EnabledInDm(false)]
public class TwitchCommands : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("follow", "Activates notifications for a certain Twitch Stream")]
    public async Task FollowStreamCommand(string twitchName)
    {
        var dbContext = DbService.GetDbContext();
        var follows = await dbContext.GetGuildFollowedStreamsAsync(Context.Guild.Id);
        var duplicate = follows.FirstOrDefault(config => config.Username.ToLower().Equals(twitchName.ToLower()));
        if (duplicate == null)
        {
            await dbContext.FollowStreamAsync(Context.Guild.Id, twitchName);
            PubSubService.FollowStream(twitchName);
            await RespondAsync(embed: SuccessEmbed.GetSuccessEmbed(Context.Client, $"Followed {twitchName}",
                    $"This server will now receive notifications when ``{twitchName}`` is live"),
                ephemeral: true);
        }
        else
        {
            await RespondAsync(embed: Context.Client.GetErrorEmbed($"Already following {twitchName}",
                    $"Cannot follow ``{twitchName}`` since this Server already follows them"),
                ephemeral: true);
        }
    }

    [SlashCommand("unfollow", "Removes notifications for a certain Twitch Stream")]
    public async Task UnfollowStreamCommand(string twitchName)
    {
        var dbContext = DbService.GetDbContext();

        if (!await dbContext.IsFollowingStreamAsync(Context.Guild.Id, twitchName))
        {
            await RespondAsync(embed: Context.Client.GetErrorEmbed("Streamer Not Found",
                    $"Cannot unfollow ``{twitchName}`` since this guild does not follow them in the first place"),
                ephemeral: true);
            return;
        }

        var mainStream = await dbContext.GetMainStreamOfGuildAsync(Context.Guild);
        if (mainStream != null && twitchName.Equals(mainStream.Username))
        {
            await RespondAsync(embed: Context.Client.GetErrorEmbed("Cannot unfollow Main Streamer",
                    $"Cannot unfollow ``{twitchName}`` since they are the main streamer of this guild"),
                ephemeral: true);
        }
        else
        {
            await dbContext.UnfollowStreamAsync(Context.Guild.Id, twitchName);
            await RespondAsync(embed: SuccessEmbed.GetSuccessEmbed(Context.Client, $"Unfollowed {twitchName}",
                    $"This server will no longer receive notifications when ``{twitchName}`` is live"),
                ephemeral: true);
        }
    }


    [Group("set", "Set stuff")]
    public class SetSettings : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("main-streamer", "Sets the main Stream for this Server")]
        public async Task SetStreamCommand(string twitchName)
        {
            var dbContext = DbService.GetDbContext();
            try
            {
                await dbContext.SetMainStreamAsync(Context.Guild.Id, twitchName);
                await RespondAsync(
                    embed: SuccessEmbed.GetSuccessEmbed(Context.Client, "Main Streamer Set",
                        $"Main streamer has been set to ``{twitchName}``"), ephemeral: true);
            }
            catch (IsAlreadyMainStreamOnAnotherServerException e)
            {
                Log.Debug(e, "Error setting main streamer");
                await RespondAsync(embed: Context.Client.GetErrorEmbed("Could not set main streamer",
                    $"{e.Message}"), ephemeral: true);
            }
            catch (Exception e)
            {
                Log.Debug(e, "Error setting main streamer");
                await RespondAsync(embed: Context.Client.GetErrorEmbed("Error setting main streamer",
                    $"{e.Message}"), ephemeral: true);
            }
        }

        [SlashCommand("sr-reward", "Adds a Twitch Reward for the Song Requests")]
        public async Task SetSongRequestRewardCommand()
        {
            var creds = new BotCredsProvider().GetCreds();
            var dbContext = DbService.GetDbContext();
            var config = await dbContext.GetMainStreamOfGuildAsync(Context.Guild);

            var api = new TwitchAPI
            {
                Settings =
                {
                    ClientId = creds.TwitchAppClientId,
                    Secret = creds.TwitchAppClientSecret,
                    Scopes = new List<AuthScopes>
                        { AuthScopes.Helix_Channel_Manage_Redemptions, AuthScopes.Helix_Channel_Read_Redemptions }
                }
            };

            var server = new HttpServer();
            server.EndPoint = new IPEndPoint(IPAddress.Loopback, 9000);

            var authUrl = api.Auth.GetAuthorizationCodeUrl($"https://{creds.ServerIp}/callback/twitch",
                new[] { AuthScopes.Helix_Channel_Manage_Redemptions, AuthScopes.Helix_Channel_Read_Redemptions });


            await RespondAsync(embed: Context.Client.GetAuthEmbed(authUrl), ephemeral: true);

            server.RequestReceived += async (_, args) =>
            {
                if (!args.Request.QueryString.AllKeys.Any("code".Contains!)) return;
                var authCode = args.Request.QueryString["code"];
                var authToken = await api.Auth.GetAccessTokenFromCodeAsync(authCode, creds.TwitchAppClientSecret,
                    $"https://{creds.ServerIp}/callback/twitch", creds.TwitchAppClientId);
                var rewardsList =
                    await api.Helix.ChannelPoints.GetCustomRewardAsync(config.ChannelId,
                        accessToken: authToken.AccessToken);

                var fittingRewards = rewardsList.Data.Where(reward => reward.IsUserInputRequired).ToArray();

                var menuBuilder = new SelectMenuBuilder()
                    .WithPlaceholder("Rewards")
                    .WithCustomId("rew-menu");

                foreach (var reward in fittingRewards)
                {
                    var option = new SelectMenuOptionBuilder
                    {
                        Label = reward.Title,
                        Value = reward.Id
                    };
                    menuBuilder.AddOption(option);
                }

                if (fittingRewards.Length > 0)
                {
                    var builder = new ComponentBuilder().WithSelectMenu(menuBuilder);
                    var name = fittingRewards[0].BroadcasterName;
                    await ReplyAsync($"Fitting rewards for ``{name}``:", components: builder.Build());
                }
                else
                {
                    await ReplyAsync("No fitting rewards were found!\n" +
                                     "Make sure to have at least 1 custom reward with an text-input!");
                }

                server.Stop();
            };
            server.Start();
        }
    }

    [Group("notification", "Set stuff")]
    public class SetNotificationSettings : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("main-channel", "Changes the message for main live notifications")]
        public async Task SetLiveChannelMessageCommand(string message)
        {
            var dbService = DbService.GetDbContext();
            await dbService.SetMainStreamNotificationAsync(Context.Guild, message);

            await dbService.SaveChangesAsync();
            await RespondAsync(
                text: "Changed the Live Notification Message!", ephemeral: true);
        }

        [SlashCommand("partner-channel", "Changes the message for partner live notifications")]
        public async Task SetPartnerChannelMessageCommand(string message)
        {
            var dbService = DbService.GetDbContext();
            await dbService.SetPartnerStreamNotificationAsync(Context.Guild, message);

            await dbService.SaveChangesAsync();
            await RespondAsync(
                text: "Changed the Live Notification Message!", ephemeral: true);
        }
    }

    [SlashCommand("post-live", "Re-posts the live notification in case the bot missed it")]
    public async Task TwitchDebugMainEmbedCommand()
    {
        var creds = new BotCredsProvider().GetCreds();
        var dbContext = DbService.GetDbContext();
        var notificationChannel = await dbContext.GetLiveChannelAsync(Context.Guild);
        var mainStream = await dbContext.GetMainStreamOfGuildAsync(Context.Guild);

        var api = new TwitchAPI
        {
            Settings =
            {
                AccessToken = creds.TwitchAccessToken,
                ClientId = creds.TwitchClientId
            }
        };

        var guildConfig = await dbContext.GetGuildConfigAsync(Context.Guild);

        var usersData = api.Helix.Channels.GetChannelInformationAsync(mainStream.ChannelId, creds.TwitchAccessToken)
            .Result.Data
            .SingleOrDefault(x => x.BroadcasterId == mainStream.ChannelId);
        var user = api.Helix.Search.SearchChannelsAsync(usersData!.BroadcasterName).Result.Channels
            .SingleOrDefault(x => x.DisplayName == usersData.BroadcasterName);
        var twitchData = new TwitchStreamData(user!.DisplayName,
            user.Id,
            user.Title,
            user.ThumbnailUrl,
            user.GameName,
            user.StartedAt);

        var channel = Context.Client.GetChannel(notificationChannel.Id) as SocketTextChannel;

        if (channel == null)
        {
            await RespondAsync("Could not find the notification channel");
            return;
        }

        await RespondAsync("Fetched Data. Posting now");
        await DeleteOriginalResponseAsync();
        await channel.SendMessageAsync(
            text: TwitchStringHelper.ParseTwitchNotification(guildConfig.MainStreamNotification, twitchData),
            embed: twitchData.GetDiscordEmbed());
    }

    [RequireOwner]
    [Group("debug", "Debugging")]
    public class ResetSettings : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("main-notification", "[Owner] Debug twitch embed")]
        public async Task TwitchDebugMainEmbedCommand(string name)
        {
            var creds = new BotCredsProvider().GetCreds();
            var dbContext = DbService.GetDbContext();
            var id = TwitchConverter.GetTwitchIdFromName(name);

            var api = new TwitchAPI
            {
                Settings =
                {
                    AccessToken = creds.TwitchAccessToken,
                    ClientId = creds.TwitchClientId
                }
            };

            var guildConfig = await dbContext.GetGuildConfigAsync(Context.Guild);

            var usersData = api.Helix.Channels.GetChannelInformationAsync(id, creds.TwitchAccessToken).Result.Data
                .SingleOrDefault(x => x.BroadcasterId == id);
            var user = api.Helix.Search.SearchChannelsAsync(usersData!.BroadcasterName).Result.Channels
                .SingleOrDefault(x => x.DisplayName == usersData.BroadcasterName);
            var twitchData = new TwitchStreamData(user!.DisplayName,
                user.Id,
                user.Title,
                user.ThumbnailUrl,
                user.GameName,
                user.StartedAt);

            await RespondAsync("Done");
            await DeleteOriginalResponseAsync();
            await ReplyAsync(
                text: TwitchStringHelper.ParseTwitchNotification(guildConfig.MainStreamNotification, twitchData),
                embed: twitchData.GetDiscordEmbed());
        }

        [SlashCommand("partner-notification", "[Owner] Debug twitch embed")]
        public async Task TwitchDebugPartnerEmbedCommand(string name)
        {
            var creds = new BotCredsProvider().GetCreds();
            var dbContext = DbService.GetDbContext();
            var id = TwitchConverter.GetTwitchIdFromName(name);

            var api = new TwitchAPI
            {
                Settings =
                {
                    AccessToken = creds.TwitchAccessToken,
                    ClientId = creds.TwitchClientId
                }
            };

            var guildConfig = await dbContext.GetGuildConfigAsync(Context.Guild);

            var usersData = api.Helix.Channels.GetChannelInformationAsync(id, creds.TwitchAccessToken).Result.Data
                .SingleOrDefault(x => x.BroadcasterId == id);
            var user = api.Helix.Search.SearchChannelsAsync(usersData!.BroadcasterName).Result.Channels
                .SingleOrDefault(x => x.DisplayName == usersData.BroadcasterName);
            var twitchData = new TwitchStreamData(user!.DisplayName,
                user.Id,
                user.Title,
                user.ThumbnailUrl,
                user.GameName,
                user.StartedAt);

            await RespondAsync("Done");
            await DeleteOriginalResponseAsync();
            await ReplyAsync(
                text: TwitchStringHelper.ParseTwitchNotification(guildConfig.PartnerStreamNotification, twitchData),
                embed: twitchData.GetDiscordEmbed());
        }
    }
}