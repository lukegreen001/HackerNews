using HackerNewsApi.Models;
using HackerNewsApi.Static;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace HackerNewsApi.Services.TopStoriesService
{
    public class TopStoriesService : ITopStoriesService
    {
        public readonly IHttpClientFactory _httpClientFactory;
        public readonly IMemoryCache _memoryCache;
        public readonly IConfiguration _configuration;

        public TopStoriesService(
            IMemoryCache memoryCache, 
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration) { 
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
            _configuration = configuration;
        }

        public async Task<List<Story>> GetTopStoriesAsync(int number, CancellationToken cancellationToken)
        {            
            // TODO LG:  add cancellation token

            var httpClientTopStories = _httpClientFactory.CreateClient(HackerNewsHttpFactoryNames.TopStories);

            var topStoryIds = new List<int>();

            // Get the list of best story Ids from the hacker news API
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

            if (!topStoryIds.Any())
            {
                return storyList;
            }

            // Loop through all the IDs and retrieve the story data and store in a list
            foreach (var topStoryId in topStoryIds.Take(number))
            {
                var createGetStoryTask = Task.Run(() => CreateGetStoryTask(storyList, topStoryId, cancellationToken));

                getStoryTaskList.Add(createGetStoryTask);
            }

            try {
                await Task.WhenAll(getStoryTaskList);
            }
            catch (Exception ex){
                throw new Exception("Error getting individual stories", ex);
            }

            var sortedStoryList = storyList.OrderByDescending(story => story.score).ThenBy(story =>story.title).ToList();

            // Save the list in the memory cache
            var hackerStoriesCacheExpirySeconds = Convert.ToInt32(_configuration["HackerStoriesCacheExpirySeconds"]);

            _memoryCache.Set(MemoryCacheNames.TOP_STORIES_CACHE_KEY, sortedStoryList, DateTime.Now.AddSeconds(hackerStoriesCacheExpirySeconds));

            return sortedStoryList;
        }

        /// <summary>
        /// Create task to fetch the story data for the given API and add the story to the list 
        /// </summary>
        /// <param name="storyList"></param>
        /// <param name="storyId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task CreateGetStoryTask(List<Story> storyList, int storyId, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

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
