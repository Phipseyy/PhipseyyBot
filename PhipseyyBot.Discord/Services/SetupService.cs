﻿#nullable disable
using Discord;
using Discord.WebSocket;
using PhipseyyBot.Common.Db;
using PhipseyyBot.Common.Db.Extensions;

namespace PhipseyyBot.Discord.Services;

public static class SetupService
{
    private const string Owner = "Philted#3787";

    public static async Task InitializeChannels(PhipseyyDbContext context, SocketGuild socketGuild)
    {
        var logRequest = socketGuild.TextChannels.FirstOrDefault(channel => channel.Name.Contains("log"));
        var logChannel = logRequest ?? await CreatePrivateTextChannelAsync(socketGuild, "log");
        
        var liveRequest = socketGuild.TextChannels.FirstOrDefault(channel => channel.Name.Contains("stream"));
        var liveChannel = liveRequest ?? await CreatePrivateTextChannelAsync(socketGuild, "stream-notifications");

        var welcomeMessage = new EmbedBuilder
        {
            Color = new Color(176, 11, 105),
            Title = "H0i",
            Description =
                $"Thank you for using PhipseyyBot! \nIf you encounter any bugs or errors, please contact {Owner}",
            ImageUrl = "https://media.giphy.com/media/8U8LDibipKRDq/giphy.gif",
            Footer = new EmbedFooterBuilder
            {
                Text = "PhipseyyBot"
            }
        };
        
        context.AddGuildToDb(socketGuild.Id, logChannel.Id, liveChannel.Id);
        await logChannel.SendMessageAsync(embed: welcomeMessage.Build());
    }
    
    private static async Task<SocketTextChannel> CreatePrivateTextChannelAsync(
        SocketGuild socketGuild,
        string channelName)
    {
        var channel = await socketGuild.CreateTextChannelAsync(channelName);
        await channel.AddPermissionOverwriteAsync(
            socketGuild.Roles.FirstOrDefault(role => role.Name == "@everyone"),
            OverwritePermissions.DenyAll(channel));
        
        return socketGuild.GetTextChannel(channel.Id);
    }


    /// <summary>
    /// Checks if the channels in the DB exist on the current guild
    /// If they don't, it will create them again and override the data in the DB
    /// </summary>
    /// <param name="context"></param>
    /// <param name="socketGuild"></param>
    public static void VerifyChannels(PhipseyyDbContext context, SocketGuild socketGuild)
    {
        var config = context.GetGuildConfig(socketGuild.Id);
        
        var logRequest = socketGuild.TextChannels.FirstOrDefault(x => x.Id == config.LogChannel, null);
        var logChannel = logRequest ?? CreatePrivateTextChannelAsync(socketGuild, "log").Result;

        var liveRequest = socketGuild.TextChannels.FirstOrDefault(x => x.Id == config.LiveChannel, null);
        var liveChannel = liveRequest ?? CreatePrivateTextChannelAsync(socketGuild, "stream-notifications").Result;
        
        
        if (logChannel.Id != config.LogChannel || liveChannel.Id != config.LiveChannel)
            context.AddGuildToDb(socketGuild.Id, logChannel.Id, liveChannel.Id);
    }

}