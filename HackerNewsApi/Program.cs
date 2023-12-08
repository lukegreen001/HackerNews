using HackerNewsApi.Models;
using HackerNewsApi.Services.TopStoriesService;
using HackerNewsApi.Static;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();
builder.Services.AddTransient<ITopStoriesService, TopStoriesService>();

builder.Services.AddHttpClient(HackerNewsHttpFactoryNames.TopStories, c => c.BaseAddress = new Uri(builder.Configuration["Urls:TopStoriesUrl"].ToString()));
builder.Services.AddHttpClient(HackerNewsHttpFactoryNames.IndividualStories, c => c.BaseAddress = new Uri(builder.Configuration["Urls:IndividualStoriesUrl"].ToString()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/{number}", async (int number, IMemoryCache memoryCache, ITopStoriesService topStoriesService, CancellationToken cancellationToken) =>
{
    try
    {
        var topStories = new List<Story>();

        if (number <= 0)
        {
            return Results.Ok(topStories.ToList());
        }

        // Get the top stories from the cache first
        memoryCache.TryGetValue(MemoryCacheNames.TOP_STORIES_CACHE_KEY, out topStories);

        // First use the memory cache. If there is nothing in there or the number of stories being requested
        // is greater than what is cached then make the call to the service 

        if (topStories != null && topStories.Count > 0  && topStories.Count >= number )
        {
            return Results.Ok(topStories.Take(number).ToList());
        }
        else
        {
            var result = await topStoriesService.GetTopStoriesAsync(number, cancellationToken);

            return Results.Ok(result.ToList());
        }                
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }    
})
.WithName("GetTopHackerNewsStories")
.WithOpenApi();

app.Run();