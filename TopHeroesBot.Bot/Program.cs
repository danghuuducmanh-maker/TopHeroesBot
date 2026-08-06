using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TopHeroesBot.Bot.Services;
using TopHeroesBot.Infrastructure.Data;
using TopHeroesBot.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
var builder = Host.CreateApplicationBuilder(args);

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Discord Client
builder.Services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.Guilds
}));

// Interaction Service
builder.Services.AddSingleton(provider =>
{
    var client = provider.GetRequiredService<DiscordSocketClient>();

    return new InteractionService(client);
});
// Hosted Services
builder.Services.AddHostedService<DiscordBotService>();
builder.Services.AddHostedService<SchedulerService>();

var host = builder.Build();
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
host.Run();