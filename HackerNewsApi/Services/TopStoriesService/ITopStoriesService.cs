using HackerNewsApi.Models;
using System.Collections.Concurrent;

namespace HackerNewsApi.Services.TopStoriesService
{
    public interface ITopStoriesService
    {
        Task<List<Story>> GetTopStoriesAsync(int number = 0);
    }
}
