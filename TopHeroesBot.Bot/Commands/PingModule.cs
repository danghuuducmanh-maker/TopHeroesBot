using Discord.Interactions;

public class PingModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Kiểm tra bot còn hoạt động")]
    public async Task Ping()
    {
        await RespondAsync("🏓 Pong!");
    }
}