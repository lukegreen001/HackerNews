# HackerNews

Solution:

Using ASP.NET Core 8.0 with minimal API I use a list of tasks to fetch the stories via httpClient. And once they are retreived they are sorted and stored in a memory cache. 
if there were going to be more than once instance of this server I would use redis server as a centralised memory store.

The only problem with this solution is that anytime a user want to get more stories that are listed in the cache it will have to make a new call and not use the cache.

The other solution I was thinking of was to have a background service that fetches all the stories and sorts them on an an interval. That way all
Stories would be available to users and they would have the performance. This would always fetch stories though regardless of whether anyone was the API Solution
it was deemed inefficient.

If I had more time I would implement some logging around the exceptions and calls.

To Run:

This solution was created in Visual Studio. So if you open it up there and run it you can use the SwaggerUI to initiate a call to the rest API.


