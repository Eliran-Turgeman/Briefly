using Briefly.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Briefly.Data;
using Briefly.Services.Summarization;
using Briefly.Services.Publishing;
using Prometheus;
using Microsoft.AspNetCore.Mvc;
using Briefly.Services.Summarization.ContentFetchers.Strategies;
using Briefly.Services.Summarization.ContentFetchers;
using Briefly.Services.Summarization.SummarizationProviders;
using Briefly.Services.Publishing.Publishers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContextFactory<BrieflyContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BrieflyContext") ?? throw new InvalidOperationException("Connection string 'BrieflyContext' not found.")));

builder.Services.AddQuickGridEntityFrameworkAdapter();
builder.Services.AddControllers();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()); // Automatically validate anti-forgery tokens on all POST requests
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add services to the container.
builder.Services.AddRazorComponents()
.AddInteractiveServerComponents();

builder.Services.AddSingleton<IContentFetchStrategy, SmartReaderFetchStrategy>();
builder.Services.AddSingleton<IContentFetchStrategy, HttpFetchStrategy>();
builder.Services.AddSingleton<IContentFetcher, ContentFetcher>();
builder.Services.AddSingleton<ISummarizationService, SummarizationService>();
builder.Services.AddSingleton<IPublishingService, PublishingService>();

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
    var logger = provider.GetRequiredService<ILogger<TelegramPublisher>>();

    return new TelegramPublisher(apiId!, apiHash!, phoneNumber!, channelId, logger);
});

builder.Services.AddHostedService<SummarizationBackgroundService>();
builder.Services.AddHostedService<PublishingBackgroundService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseMigrationsEndPoint();
}
app.UseHttpMetrics(); // Collect HTTP metrics

app.UseRouting();

app.UseAuthentication();
app.UseAntiforgery();
app.UseAuthorization();


app.UseEndpoints(endpoints =>
{
    endpoints.MapMetrics(); // Expose metrics at /metrics endpoint
    endpoints.MapControllers();
});

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
