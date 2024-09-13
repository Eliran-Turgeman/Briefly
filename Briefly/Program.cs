using Briefly.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Briefly.Data;
using Briefly.Services.Summarization;
using Briefly.Services.Publishing;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContextFactory<BrieflyContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BrieflyContext") ?? throw new InvalidOperationException("Connection string 'BrieflyContext' not found.")));

builder.Services.AddQuickGridEntityFrameworkAdapter();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<ITextSummaryProvider>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var apiKey = configuration["OpenAI:ApiKey"];
    var logger = provider.GetRequiredService<ILogger<OpenAiTextSummaryProvider>>();

    return new OpenAiTextSummaryProvider(apiKey!, logger);
});

builder.Services.AddSingleton<IPublisher>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();

    var apiId = configuration["Telegram:ApiId"];
    var apiHash = configuration["Telegram:ApiHash"];
    var phoneNumber = configuration["Telegram:PhoneNumber"];
    var channelId = long.Parse(configuration["Telegram:ChannelId"]!);

    return new TelegramPublisher(apiId!, apiHash!, phoneNumber!, channelId);
});

builder.Services.AddHostedService<SummarizationService>();
builder.Services.AddHostedService<PublishingService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
