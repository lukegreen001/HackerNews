using HackerNewsApi.Models;
using HackerNewsApi.Static;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace HackerNewsApi.Services.TopStoriesService
{
    public class TopStoriesService : ITopStoriesService
    {
        public readonly IHttpClientFactory _httpClientFactory;
        public readonly IMemoryCache _memoryCache;

        public TopStoriesService(IMemoryCache memoryCache, IHttpClientFactory httpClientFactory) { 
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
        }

        public async Task<List<Story>> GetTopStoriesAsync(int number = 0)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            // TODO LG:  add cancellation token

            var httpClientTopStories = _httpClientFactory.CreateClient(HackerNewsHttpFactoryNames.TopStories);

            var topStoryIds = new List<int>();

            try {
                var task = await httpClientTopStories.GetAsync("");

                var jsonString = await task.Content.ReadAsStringAsync();

                topStoryIds = JsonConvert.DeserializeObject<List<int>>(jsonString);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting hacker news top stories", ex);
            }

            var storyList = new List<Story>();

            var getStoryTaskList = new List<Task>();

            // TODO LG: Check number here if greater than existing
            // Need logic if exising memcache number is greater and asking number is less - what to do

            foreach (var topStoryId in topStoryIds.Take(number))
            {
                var createGetStoryTask = Task.Run(() => CreateGetStoryTask(storyList, topStoryId));

                getStoryTaskList.Add(createGetStoryTask);
            }

            try {
                await Task.WhenAll(getStoryTaskList);
            }
            catch (Exception ex){
                throw new Exception("Error getting individual stories", ex);
            }

            stopwatch.Stop();

            Console.WriteLine(stopwatch.ElapsedMilliseconds);

            return storyList.OrderByDescending(story => story.score).ThenBy(story =>story.title).ToList();
        }

        private async Task CreateGetStoryTask(List<Story> storyList, int storyId)
        {
            try
            {
                var httpClientIndividualStory = _httpClientFactory.CreateClient(HackerNewsHttpFactoryNames.IndividualStories);

                var task = await httpClientIndividualStory.GetAsync($"{storyId}.json");

                var jsonString = await task.Content.ReadAsStringAsync();

                var story = JsonConvert.DeserializeObject<Story>(jsonString);

                if (story != null)
                {
                    storyList.Add(story);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting story {storyId}", ex);
            }
            
        }
    }
}
