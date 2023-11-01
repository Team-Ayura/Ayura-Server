using AutoMapper;
using Ayura.API.Configuration;
using Ayura.API.Features.MoodTracking.DTOs;
using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Ayura.API.Features.MoodTracking.Services;

public class MoodService : IMoodService
{
    private readonly IOptions<AppSettings> _appSettings;
    private readonly IMapper _mapper;
    private readonly IMongoCollection<User> _userCollection;

    public MoodService(IAppSettings appSettings, IAyuraDatabaseSettings settings, IMongoClient mongoClient,
        IOptions<AppSettings> appSettingsOptions)
    {
        // database and collections setup
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _userCollection = database.GetCollection<User>(settings.UserCollection);
        _appSettings = appSettingsOptions;

        // DTO to model mapping setup
        var mapperConfig = new MapperConfiguration(cfg => { cfg.CreateMap<User, AddMoodRequestDTO>().ReverseMap(); });

        _mapper = mapperConfig.CreateMapper();
    }

    public async Task<DailyMood> GetMoodsForDayAsync(string userId, DateTime date)
    {
        Console.WriteLine("Service called");

        var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

        Console.WriteLine("User found");
        if (user == null) return null; // Or throw an exception or return a specific error message

        if (user.MoodsHistories == null) return null;

        try
        {
            var moodData = user.MoodsHistories.FirstOrDefault(m => m.Date == date.ToUniversalTime());
            return moodData;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
        //
        // return user.MoodsHistories.FirstOrDefault(m => m.Date == date);
    }

    public async Task<string> AddMoodsForDayAsync(string userId, MoodEntry moodEntry, DateTime date)
    {
        try
        {
            var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
            Console.WriteLine($"User: {user.FirstName}");

            if (user == null) return "User not found";


            if (user.MoodsHistories == null)
            {
                user.MoodsHistories = new List<DailyMood>();
            }

            // check if mood data already exists for the specified date
            var existingMood = user.MoodsHistories.FirstOrDefault(m => m.Date == date.ToUniversalTime());


            // print attributes from existing mood data

            if (existingMood != null)
            {
                Console.WriteLine("There is already mood data for the specified date");

                // add to the moodEntry to the moodhistory list in user
                existingMood.MoodEntryList.Add(moodEntry);
                await _userCollection.ReplaceOneAsync(u => u.Id == userId, user);
                Console.WriteLine("Moood data updated successfully");
                return "Mood data updated successfully";
            }

            // if not, create a new mood data object and add it to the list
            var newMood = new DailyMood
            {
                Date = date,
                MoodEntryList = new List<MoodEntry> { moodEntry }
            };

            user.MoodsHistories.Add(newMood);
            await _userCollection.ReplaceOneAsync(u => u.Id == userId, user);
            return "Mood data added successfully";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public async Task<string> EditMoodsForDayAsync(string userId, DateTime date, List<MoodEntry> moodEntries)
    {
        var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

        if (user == null) return "User not found";

        var existingMood = user.MoodsHistories.FirstOrDefault(m => m.Date.Date == date.Date);


        if (existingMood == null) return "No mood data found for the specified date";

        existingMood.MoodEntryList = moodEntries;

        await _userCollection.ReplaceOneAsync(u => u.Id == userId, user);

        return "Moods updated successfully";
    }
}