using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;

namespace TopHeroesBot.Bot.Services;

public class DiscordBotService : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;

    public DiscordBotService(
        DiscordSocketClient client,
        InteractionService interactions,
        IServiceProvider services,
        IConfiguration configuration)
    {
        _client = client;
        _interactions = interactions;
        _services = services;
        _configuration = configuration;
    }

    //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //{
    //    _client.Log += LogAsync;
    //    _interactions.Log += LogAsync;

    //    _client.Ready += ReadyAsync;
    //    _client.InteractionCreated += InteractionCreated;

    //    await _interactions.AddModulesAsync(
    //        Assembly.GetExecutingAssembly(),
    //        _services);


    //    string token = _configuration["Discord:Token"]!;

    //    Console.WriteLine("4");

    //    await _client.LoginAsync(TokenType.Bot, token);

    //    Console.WriteLine("5");

    //    await _client.StartAsync();

    //    Console.WriteLine("6");

    //    await Task.Delay(Timeout.Infinite, stoppingToken);
    //}
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.Log += LogAsync;
        _interactions.Log += LogAsync;

        _client.Ready += ReadyAsync;
        _client.InteractionCreated += InteractionCreated;

        await _interactions.AddModulesAsync(
            Assembly.GetExecutingAssembly(),
            _services);

        string token = Environment.GetEnvironmentVariable("DISCORD_TOKEN")
               ?? _configuration["Discord:Token"]
               ?? throw new InvalidOperationException("Discord token not found.");

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    //private async Task ReadyAsync()
    //{
    //    Console.WriteLine("READY");

    //    ulong guildId = 1425855503145369715;

    //    await _interactions.RegisterCommandsToGuildAsync(guildId);

    //    Console.WriteLine("COMMAND REGISTERED");

    //    Console.WriteLine($"Bot online: {_client.CurrentUser}");
    //}
    private async Task ReadyAsync()
    {
        ulong guildId = 1425855503145369715;

        await _interactions.RegisterCommandsToGuildAsync(guildId);

        Console.WriteLine($"Bot online: {_client.CurrentUser}");
    }

    private async Task InteractionCreated(SocketInteraction interaction)
    {
        var context = new SocketInteractionContext(_client, interaction);

        await _interactions.ExecuteCommandAsync(
            context,
            _services);
    }
    //private async Task InteractionCreated(SocketInteraction interaction)
    //{
    //    Console.WriteLine($"Interaction: {interaction.Type}");

    //    var context = new SocketInteractionContext(_client, interaction);

    //    var result = await _interactions.ExecuteCommandAsync(
    //        context,
    //        _services);

    //    Console.WriteLine(result.IsSuccess
    //        ? "Command OK"
    //        : result.ErrorReason);
    //}
    private Task LogAsync(LogMessage message)
    {
        Console.WriteLine(message);

        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();

        await base.StopAsync(cancellationToken);
    }
}