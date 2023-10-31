using System.Globalization;
using AutoMapper;
using Ayura.API.Features.Activity.DTOs;
using Ayura.API.Features.Activity.Models;
using Ayura.API.Features.Sleep.DTOs;
using Ayura.API.Features.Sleep.Models;
using Ayura.API.Global.Constants;
using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using MimeKit.Encodings;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Ayura.API.Features.Sleep.Services;

public class SleepService : ISleepService
{
    private readonly IMongoCollection<User> _userCollection;
    private IMapper _mapper;
    
    //constructor
    public SleepService(IAyuraDatabaseSettings settings, IMongoClient mongoClient)
    {
        // database and collections setup
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _userCollection = database.GetCollection<User>(settings.UserCollection);
        
        // DTO to model mapping setup
        var mapperConfig = new MapperConfiguration(cfg => { cfg.CreateMap<AddCyclingRequest, CyclingHistory>(); });

        _mapper = mapperConfig.CreateMapper();
    }
    
    // 1. Add sleep data each day 
    public async Task<string> AddSleepData(AddSleepDataDto addSleepDataDto)
    {
        
        // map sleep request to a user
        var oneSleepData = new SleepHistory
        {
        
            Id = ObjectId.GenerateNewId().ToString(),
            BedTime = addSleepDataDto.BedTime,
            WakeupTime = addSleepDataDto.WakeupTime,
            Duration = addSleepDataDto.Duration,
            Quality = addSleepDataDto.Quality,
            BeforeSleepAffect = addSleepDataDto.BeforeSleepAffect,
            AfterSleepAffect = addSleepDataDto.AfterSleepAffect,
            
        };
        
        var filter = Builders<User>.Filter.Eq(u => u.Id, addSleepDataDto.UserId);
        var update = Builders<User>.Update.Push<SleepHistory>(u => u.SleepHistories, oneSleepData);

        await _userCollection.UpdateOneAsync(filter, update);

        return "success";
    }
    
     // 2. Get sleeping data by a filter (day, week, month or year)
    public async Task<object> GetSleepingData(string userId, string filterType)
    {
        // define the result(DTO)
        var response = new SleepDataResponse();
        // identify the time duration.
        var activityfilter = (SleepChartFilterType)Enum.Parse(typeof(SleepChartFilterType), filterType);
        DateTime startingDate;
        var endingDate = DateTime.Today;
        var today = DateTime.Today;

        switch (activityfilter)
        {

            case SleepChartFilterType.Week:
                var diff = (int)today.DayOfWeek - (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
                if (diff < 0)
                    diff += 7;

                startingDate = today.AddDays(-diff + 1).Date;
                break;
            case SleepChartFilterType.BiWeek:
                var daysInWeek = 7;
                 // Calculate the starting date for the current week
                    var currentWeekStartDate = today.AddDays(-(int)today.DayOfWeek); 
                    // Calculate the starting date for the previous week
                    var previousWeekStartDate = currentWeekStartDate.AddDays(-7);
                    // Combine the previous week and the current week
                    startingDate = previousWeekStartDate;
                    break;
            case SleepChartFilterType.Month:
                startingDate = new DateTime(today.Year, today.Month, 1);
                break;
            default:
                startingDate = new DateTime(today.Year, 1, 1);
                break;
        }

        Console.WriteLine(startingDate);
        Console.WriteLine(endingDate);
        // set the timeperiod
        response.timePeriod = $"{startingDate.ToString("MMM dd")} - {endingDate.ToString("MMM dd")}";


        // fetch average movement minutes in the above duration
        response.avgSleepTime = await _getAverageSleepTime(userId, startingDate, endingDate);

        // // fetch the sleep qualities of each day in selected time duration
        // response.sleepQualities = await _getDistances(userId, activityfilter, startingDate, endingDate);
        //
        
        // fetch the sleep hours(durations) of each day in selected time duration
        response.sleepingHours = await _getSleepHours(userId, activityfilter, startingDate, endingDate);
        
        // fetch the sleep history of each day in selected time duration
        response.sleepHistory = await _getSleepHistory(userId, startingDate, endingDate);
        
       
        return response;
    }
    
    
    private async Task<int> _getAverageSleepTime(string id, DateTime startingDate, DateTime endingDate)
    {
        var pipeline = new BsonDocument[]
        {
            new BsonDocument("$match", new BsonDocument("_id", ObjectId.Parse(id))),
            new BsonDocument("$unwind", "$sleepHistories"),
            new BsonDocument("$match", new BsonDocument
            {
                {
                    "$and", new BsonArray
                    {
                        new BsonDocument("sleepHistories.bedTime", new BsonDocument("$gte", BsonDateTime.Create(startingDate))),
                        new BsonDocument("sleepHistories.wakeupTime", new BsonDocument("$lte", BsonDateTime.Create(endingDate)))
                    }
                }
            }),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", new BsonDocument
                    {
                        {
                            "$dateToString", new BsonDocument
                            {
                                { "format", "%Y-%m-%d" },
                                { "date", "$sleepHistories.bedTime" }
                            }
                        }
                    }
                },
                { "averageDurationSlept", new BsonDocument("$avg", "$sleepHistories.duration") }
            })
        };


        var aggregationCursor = await _userCollection.AggregateAsync<BsonDocument>(pipeline);
        var result = await aggregationCursor.FirstOrDefaultAsync();

        if (result != null && result.TryGetValue("averageDurationSlept", out var averageSleepMinutesValue))
        {
            var averageSleepMinutes = (int)averageSleepMinutesValue.AsDouble;
            Console.WriteLine($"Average Sleep Minutes: {averageSleepMinutes}");
            return averageSleepMinutes;
        }

        return 0;

    }
    
     private async Task<List<int>> _getSleepHours(string id, SleepChartFilterType filterType, DateTime startingDate, DateTime endingDate)
    {
        var duration = GenerateZeroArray(filterType);
        var index = 0;
        var currentYear = DateTime.UtcNow.Year;
        var pipeline = Array.Empty<BsonDocument>();
        // pipeline for the filter type Year
        switch (filterType)
        {
            
            case SleepChartFilterType.Week:
                // aggregation pipline for the type week
                pipeline = new BsonDocument[]
                {
                    new BsonDocument("$match", new BsonDocument("_id", ObjectId.Parse(id))),
                    new BsonDocument("$unwind", "$sleepHistories"),
                    new BsonDocument("$match", new BsonDocument
                    {
                        {
                            "sleepHistories.bedTime",
                            new BsonDocument
                            {
                                { "$gte", startingDate },
                                { "$lte", endingDate }
                            }
                        }
                    }),
                    new BsonDocument("$project", new BsonDocument
                    {
                        { "dayOfWeek", new BsonDocument("$dayOfWeek", "$sleepHistories.bedTime") },
                        { "date", new BsonDocument("$dateToString", new BsonDocument
                            {
                                { "format", "%Y-%m-%d" },
                                { "date", "$sleepHistories.bedTime" }
                            })
                        },
                        { "duration", "$sleepHistories.duration" }
                    }),
                    new BsonDocument("$group", new BsonDocument
                    {
                        { "_id", new BsonDocument
                            {
                                { "dayOfWeek", "$dayOfWeek" },
                                { "date", "$date" }
                            }
                        },
                        { "totalDuration", new BsonDocument("$sum", "$duration") }
                    }),
                    new BsonDocument("$project", new BsonDocument
                    {
                        { "unit", "$_id.dayOfWeek" },
                        { "duration", "$totalDuration" }
                    })
                };

                break;
                
         case SleepChartFilterType.BiWeek:
                         // aggregation pipline for the type week
                         pipeline = new BsonDocument[]
                         {
                             new BsonDocument("$match", new BsonDocument("_id", ObjectId.Parse(id))),
                             new BsonDocument("$unwind", "$sleepHistories"),
                             new BsonDocument("$match", new BsonDocument
                             {
                                 {
                                     "sleepHistories.bedTime",
                                     new BsonDocument
                                     {
                                         { "$gte", startingDate },
                                         { "$lte", endingDate }
                                     }
                                 }
                             }),
                             new BsonDocument("$project", new BsonDocument
                             {
                                 { "dayOfWeek", new BsonDocument("$dayOfWeek", "$sleepHistories.bedTime") },
                                 { "date", new BsonDocument("$dateToString", new BsonDocument
                                     {
                                         { "format", "%Y-%m-%d" },
                                         { "date", "$sleepHistories.bedTime" }
                                     })
                                 },
                                 { "duration", "$sleepHistories.duration" }
                             }),
                             new BsonDocument("$group", new BsonDocument
                             {
                                 { "_id", new BsonDocument
                                     {
                                         { "dayOfWeek", "$dayOfWeek" },
                                         { "date", "$date" }
                                     }
                                 },
                                 { "totalDuration", new BsonDocument("$sum", "$duration") }
                             }),
                             new BsonDocument("$project", new BsonDocument
                             {
                                 { "unit", "$_id.dayOfWeek" },
                                 { "duration", "$totalDuration" }
                             })
                         };
         
                         break;
            case SleepChartFilterType.Month:
                // aggregation pipline for the type month
                pipeline = new BsonDocument[]
                {
                    new BsonDocument("$match", new BsonDocument("_id", ObjectId.Parse(id))),
                    new BsonDocument("$unwind", "$sleepHistories"),
                    new BsonDocument("$match", new BsonDocument
                    {
                        {
                            "sleepHistories.bedTime",
                            new BsonDocument
                            {
                                { "$gte", startingDate },
                                { "$lte", endingDate }
                            }
                        }
                    }),
                    new BsonDocument("$project", new BsonDocument
                    {
                        { "dayOfMonth", new BsonDocument("$dayOfMonth", "$sleepHistories.bedTime") },
                        { "date", new BsonDocument("$dateToString", new BsonDocument
                            {
                                { "format", "%Y-%m-%d" },
                                { "date", "$sleepHistories.bedTime" }
                            })
                        },
                        { "duration", "$sleepHistories.duration" }
                    }),
                    new BsonDocument("$group", new BsonDocument
                    {
                        { "_id", new BsonDocument
                            {
                                { "dayOfMonth", "$dayOfMonth" },
                                { "date", "$date" }
                            }
                        },
                        { "totalDuration", new BsonDocument("$sum", "$duration") }
                    }),
                    new BsonDocument("$project", new BsonDocument
                    {
                        { "unit", "$_id.dayOfMonth" },
                        { "duration", "$totalDuration" }
                    })
                };

                break;
            default:
                // aggregation pipline for the type year
                pipeline = new BsonDocument[]
                {
                    new BsonDocument("$match", new BsonDocument("_id", ObjectId.Parse(id))),
                    new BsonDocument("$unwind", "$sleepHistories"),
                    new BsonDocument("$match", new BsonDocument
                    {
                        {
                            "sleepHistories.bedTime",
                            new BsonDocument
                            {
                                { "$gte", startingDate },
                                { "$lte", endingDate }
                            }
                        }
                    }),
                    new BsonDocument("$project", new BsonDocument
                    {
                        { "monthOfYear", new BsonDocument("$month", "$sleepHistories.bedTime") },
                        { "dayOfMonth", new BsonDocument("$dayOfMonth", "$sleepHistories.bedTime") },
                        { "duration", "$sleepHistories.duration" }
                    }),
                    new BsonDocument("$group", new BsonDocument
                    {
                        { "_id", new BsonDocument
                            {
                                { "monthOfYear", "$monthOfYear" },
                                { "dayOfMonth", "$dayOfMonth" }
                            }
                        },
                        { "dailyTotalDuration", new BsonDocument("$sum", "$duration") },
                        { "count", new BsonDocument("$sum", 1) } // Count the number of entries for each day
                    }),
                    new BsonDocument("$group", new BsonDocument
                    {
                        { "_id", "$_id.monthOfYear" }, // Group by month
                        { "averageDuration", new BsonDocument("$avg", "$dailyTotalDuration") }
                    }),
                    new BsonDocument("$project", new BsonDocument
                    {
                        { "unit", "$_id" }, // Rename _id as unit
                        { "duration", "$averageDuration" }
                    })
                };


                break;
        }
        
        
        var aggregationCursor = await _userCollection.AggregateAsync<BsonDocument>(pipeline);
        var results = await aggregationCursor.ToListAsync();
        foreach (var result in results)
        {
            // Console.WriteLine(result);
            if (result != null &&
                result.TryGetValue("unit", out var month) &&
                result.TryGetValue("duration", out var durationslept))
            {
                while (index+1 < (int)month)
                {
                    index++;
                }
                
                if ((int)month == index + 1)
                {
                    duration[index] = (int)durationslept.AsInt32;
                }
            }

            index++;
        }
        Console.WriteLine(duration);
        return duration.ToList();

    }

    
    
    private async Task<List<SleepHistory>> _getSleepHistory(string id, DateTime startingDate, DateTime endingDate)
    {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Id, id), // Filter by user ID
            Builders<User>.Filter.ElemMatch(u => u.SleepHistories, sh =>
                sh.BedTime >= startingDate && sh.WakeupTime<= endingDate) // Filter cycling trips within time range
        );

        var user = await _userCollection.Find(filter).FirstOrDefaultAsync();

        if (user != null)
        {
            var sleepingData = user.SleepHistories.Take(5).ToList();
            Console.WriteLine(sleepingData);
            return sleepingData;
            // cyclingTrips now contains a maximum of 5 cycling trips within the specified time range.
        }

        return new List<SleepHistory>();
    }
    
    public int[] GenerateZeroArray(SleepChartFilterType filterType)
    {
        int length = 0;

        switch (filterType)
        {
            case SleepChartFilterType.Week:
                length = 7;
                break;
            case SleepChartFilterType.BiWeek:
                length = 14;
                break;
            case SleepChartFilterType.Month:
                length = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                break;
           
            // Handle other cases if needed

            default:
                throw new ArgumentException("Unsupported filter type");
        }

        int[] zeroArray = new int[length];
        return zeroArray;
    }
    
    
    
    
    }



