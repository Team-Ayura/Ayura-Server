using System.Globalization;
using Ayura.API.Features.Activity.DTOs;
using Ayura.API.Features.Activity.Models;
using Ayura.API.Global.Constants;
using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Ayura.API.Features.Activity.Services;

public class WalkAndRunningService : IWalkAndRunningService
{
    private readonly IMongoCollection<User> _userCollection;

    public WalkAndRunningService(IAyuraDatabaseSettings settings, IMongoClient mongoClient)
    {
        // database and collections setup
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _userCollection = database.GetCollection<User>(settings.UserCollection);
    }

    // 1. Get walk and running data by a filter (day, week, month or year)
    public async Task<object> GetWalkAndRunningData(string userId, string filterType)
    {
        // define the result
        var response = new WalkAndRunningDataRespose();
        // identify the time duration.
        var activityfilter = (ChartFilterType)Enum.Parse(typeof(ChartFilterType), filterType);
        DateTime startingDate;
        var endingDate = DateTime.Today;
        var today = DateTime.Today;

        switch (activityfilter)
        {
            case ChartFilterType.Week:
                var diff = (int)today.DayOfWeek - (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
                if (diff < 0)
                    diff += 7;

                startingDate = today.AddDays(-diff + 1).Date;
                break;
            case ChartFilterType.Month:
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

        // fetch average distance covered in the above duration
        response.avgDistanceWalked = await _getAverageDistaceCovered(userId, startingDate, endingDate);

        // fetch average step count covered in the above duration
        response.avgStepCount = await _getAverageStepCount(userId, startingDate, endingDate);

        // fetch average movement minutes in the above duration
        response.avgMoveMinutes = await _getAverageMoveMinutes(userId, startingDate, endingDate);

        // fetch average calories burned in the above duration
        response.avgCaloriesBurned = await _getAverageCaloriesBurned(userId, startingDate, endingDate);

        // calculate the improvement (current average vs average in th period)
        response.improvement = await _getStepCountImprovement(userId, response.avgStepCount);

        // fetch the step count in the above duration
        response.steps = await _getSteps(userId, activityfilter, startingDate, endingDate);
        return response;
    }

    // 2. Add walk and running data each day at a spesific time
    public async Task<string> AddWalkAndRunningData(AddWalkAndRunnigRequest addWalkAndRunningRequest)
    {
        var today = DateTime.Today;

        var filter = Builders<User>.Filter.Eq(u => u.Id, addWalkAndRunningRequest.UserId);
        var update = Builders<User>.Update.Push<WalkAndRunningHistory>(u => u.MeasurableActivities.WalkAndRunning,
            new WalkAndRunningHistory
            {
                Date = DateTime.Now,
                StepCount = addWalkAndRunningRequest.StepCount,
                DistanceWalked = addWalkAndRunningRequest.DistanceWalked,
                MoveMinutes = addWalkAndRunningRequest.MoveMinutes,
                CaloriesBurned = addWalkAndRunningRequest.CaloriesBurned
            });

        await _userCollection.UpdateOneAsync(filter, update);

        return "success";
    }

    // 3. Get Today's improvement for step count
    public async Task<int> GetTodayImprovement(string userId, int todayStepCount)
    {
        return await _getStepCountImprovement(userId, todayStepCount);
    }

    // Private method which handles each individual task in the get request
    private async Task<double> _getAverageDistaceCovered(string id, DateTime startingDate, DateTime endingDate)
    {
        var pipeline = new BsonDocument[]
        {
            new("$match", new BsonDocument("_id", ObjectId.Parse(id))),
            new("$unwind", "$measurableActivities.walkAndRunning"),
            new("$match", new BsonDocument
            {
                {
                    "$and", new BsonArray
                    {
                        new BsonDocument("measurableActivities.walkAndRunning.date",
                            new BsonDocument("$gte", BsonDateTime.Create(startingDate))),
                        new BsonDocument("measurableActivities.walkAndRunning.date",
                            new BsonDocument("$lte", BsonDateTime.Create(endingDate)))
                    }
                }
            }),
            new("$group", new BsonDocument
            {
                { "_id", "null" },
                {
                    "averageDistaceWalked",
                    new BsonDocument("$avg", "$measurableActivities.walkAndRunning.distanceWalked")
                }
            })
        };


        var aggregationCursor = await _userCollection.AggregateAsync<BsonDocument>(pipeline);
        var result = await aggregationCursor.FirstOrDefaultAsync();

        if (result != null && result.TryGetValue("averageDistaceWalked", out var averageDistaceWalkedValue))
        {
            var averageDistaceWalked = averageDistaceWalkedValue.AsDouble;
            return averageDistaceWalked;
            // Console.WriteLine($"Average Step Count: {averageStepCount}");
        }

        return 0.0;
    }

    private async Task<int> _getAverageStepCount(string id, DateTime startingDate, DateTime endingDate)
    {
        var pipeline = new BsonDocument[]
        {
            new("$match", new BsonDocument("_id", ObjectId.Parse(id))),
            new("$unwind", "$measurableActivities.walkAndRunning"),
            new("$match", new BsonDocument
            {
                {
                    "$and", new BsonArray
                    {
                        new BsonDocument("measurableActivities.walkAndRunning.date",
                            new BsonDocument("$gte", BsonDateTime.Create(startingDate))),
                        new BsonDocument("measurableActivities.walkAndRunning.date",
                            new BsonDocument("$lte", BsonDateTime.Create(endingDate)))
                    }
                }
            }),
            new("$group", new BsonDocument
            {
                { "_id", "null" },
                { "averageStepCount", new BsonDocument("$avg", "$measurableActivities.walkAndRunning.stepCount") }
            })
        };


        var aggregationCursor = await _userCollection.AggregateAsync<BsonDocument>(pipeline);
        var result = await aggregationCursor.FirstOrDefaultAsync();

        if (result != null && result.TryGetValue("averageStepCount", out var averageStepCountValue))
        {
            var averageStepCount = (int)averageStepCountValue.AsDouble;
            return averageStepCount;
            // Console.WriteLine($"Average Step Count: {averageStepCount}");
        }

        return 0;
    }

    private async Task<int> _getAverageMoveMinutes(string id, DateTime startingDate, DateTime endingDate)
    {
        var pipeline = new BsonDocument[]
        {
            new("$match", new BsonDocument("_id", ObjectId.Parse(id))),
            new("$unwind", "$measurableActivities.walkAndRunning"),
            new("$match", new BsonDocument
            {
                {
                    "$and", new BsonArray
                    {
                        new BsonDocument("measurableActivities.walkAndRunning.date",
                            new BsonDocument("$gte", BsonDateTime.Create(startingDate))),
                        new BsonDocument("measurableActivities.walkAndRunning.date",
                            new BsonDocument("$lte", BsonDateTime.Create(endingDate)))
                    }
                }
            }),
            new("$group", new BsonDocument
            {
                { "_id", BsonNull.Value },
                {
                    "averageMoveMinutes",
                    new BsonDocument("$avg", "$measurableActivities.walkAndRunning.movementMinutes")
                }
            })
        };


        var aggregationCursor = await _userCollection.AggregateAsync<BsonDocument>(pipeline);
        var result = await aggregationCursor.FirstOrDefaultAsync();

        if (result != null && result.TryGetValue("averageMoveMinutes", out var averageMoveMinutesValue))
        {
            var averageMoveMinutes = (int)averageMoveMinutesValue.AsDouble;
            return averageMoveMinutes;
            // Console.WriteLine($"Average Step Count: {averageStepCount}");
        }

        return 0;
    }

    private async Task<int> _getAverageCaloriesBurned(string id, DateTime startingDate, DateTime endingDate)
    {
        var pipeline = new BsonDocument[]
        {
            new("$match", new BsonDocument("_id", ObjectId.Parse(id))),
            new("$unwind", "$measurableActivities.walkAndRunning"),
            new("$match", new BsonDocument
            {
                {
                    "$and", new BsonArray
                    {
                        new BsonDocument("measurableActivities.walkAndRunning.date",
                            new BsonDocument("$gte", BsonDateTime.Create(startingDate))),
                        new BsonDocument("measurableActivities.walkAndRunning.date",
                            new BsonDocument("$lte", BsonDateTime.Create(endingDate)))
                    }
                }
            }),
            new("$group", new BsonDocument
            {
                { "_id", "null" },
                {
                    "averageCaloriesBurned",
                    new BsonDocument("$avg", "$measurableActivities.walkAndRunning.caloriesBurned")
                }
            })
        };


        var aggregationCursor = await _userCollection.AggregateAsync<BsonDocument>(pipeline);
        var result = await aggregationCursor.FirstOrDefaultAsync();

        if (result != null && result.TryGetValue("averageCaloriesBurned", out var averageCaloriesBurnedValue))
        {
            var averageCaloriesBurned = (int)averageCaloriesBurnedValue.AsDouble;
            return averageCaloriesBurned;
            // Console.WriteLine($"Average Step Count: {averageStepCount}");
        }

        return 0;
    }

    private async Task<int> _getStepCountImprovement(string id, int currentAvgStepCount)
    {
        var pipeline = new BsonDocument[]
        {
            new("$match", new BsonDocument("_id", ObjectId.Parse(id))),
            new("$unwind", "$measurableActivities.walkAndRunning"),
            new("$group", new BsonDocument
            {
                { "_id", "null" },
                { "averageStepCount", new BsonDocument("$avg", "$measurableActivities.walkAndRunning.stepCount") }
            })
        };

        var aggregationCursor = await _userCollection.AggregateAsync<BsonDocument>(pipeline);
        var result = await aggregationCursor.FirstOrDefaultAsync();
        Console.WriteLine(result);

        if (result != null && result.TryGetValue("averageStepCount", out var averageStepCountValue))
        {
            var averageStepCount = averageStepCountValue.AsDouble;
            return (int)((currentAvgStepCount - averageStepCount) * 100 / averageStepCount);
        }

        return 0;
    }

    private async Task<List<int>> _getSteps(string id, ChartFilterType filterType, DateTime startingDate,
        DateTime endingDate)
    {
        var steps = GenerateZeroArray(filterType);
        var index = 0;
        var currentYear = DateTime.UtcNow.Year;
        var pipeline = Array.Empty<BsonDocument>();
        // pipeline for the filter type Year
        switch (filterType)
        {
            case ChartFilterType.Week:
                // aggregation pipline for the type week
                pipeline = new BsonDocument[]
                {
                    new("$match", new BsonDocument("_id", ObjectId.Parse(id))),
                    new("$unwind", "$measurableActivities.walkAndRunning"),
                    new("$match", new BsonDocument
                    {
                        {
                            "measurableActivities.walkAndRunning.date",
                            new BsonDocument
                            {
                                { "$gte", startingDate },
                                { "$lte", endingDate }
                            }
                        }
                    }),
                    new("$project", new BsonDocument
                    {
                        { "unit", new BsonDocument("$dayOfWeek", "$measurableActivities.walkAndRunning.date") },
                        { "stepCount", "$measurableActivities.walkAndRunning.stepCount" }
                    })
                };
                break;
            case ChartFilterType.Month:
                // aggregation pipline for the type month
                pipeline = new BsonDocument[]
                {
                    new("$match", new BsonDocument("_id", ObjectId.Parse(id))),
                    new("$unwind", "$measurableActivities.walkAndRunning"),
                    new("$match", new BsonDocument
                    {
                        {
                            "measurableActivities.walkAndRunning.date",
                            new BsonDocument
                            {
                                { "$gte", startingDate },
                                { "$lte", endingDate }
                            }
                        }
                    }),
                    new("$project", new BsonDocument
                    {
                        { "unit", new BsonDocument("$dayOfMonth", "$measurableActivities.walkAndRunning.date") },
                        { "stepCount", "$measurableActivities.walkAndRunning.stepCount" }
                    })
                };
                break;
            default:
                // aggregation pipline for the type year
                pipeline = new BsonDocument[]
                {
                    new("$match", new BsonDocument("_id", ObjectId.Parse(id))),
                    new("$unwind", "$measurableActivities.walkAndRunning"),
                    new("$match", new BsonDocument
                    {
                        {
                            "measurableActivities.walkAndRunning.date",
                            new BsonDocument
                            {
                                { "$gte", new DateTime(currentYear, 1, 1) },
                                { "$lte", new DateTime(currentYear, 12, 31, 23, 59, 59) }
                            }
                        }
                    }),
                    new("$group", new BsonDocument
                    {
                        {
                            "_id", new BsonDocument
                            {
                                { "month", new BsonDocument("$month", "$measurableActivities.walkAndRunning.date") }
                            }
                        },
                        { "stepCount", new BsonDocument("$avg", "$measurableActivities.walkAndRunning.stepCount") }
                    }),
                    new("$sort", new BsonDocument("_id.month", 1)),
                    new("$project", new BsonDocument
                    {
                        { "_id", 0 }, // Exclude the default _id field
                        { "unit", "$_id.month" }, // Create a new key "month"
                        { "stepCount", new BsonDocument("$toInt", "$stepCount") }
                    })
                };
                break;
        }


        var aggregationCursor = await _userCollection.AggregateAsync<BsonDocument>(pipeline);
        var results = await aggregationCursor.ToListAsync();
        foreach (var result in results)
        {
            Console.WriteLine(result);
            if (result != null &&
                result.TryGetValue("unit", out var month) &&
                result.TryGetValue("stepCount", out var stepCount))
            {
                while (index + 1 < (int)month) index++;

                if ((int)month == index + 1) steps[index] = stepCount.AsInt32;
            }

            index++;
        }

        return steps.ToList();
    }

    public int[] GenerateZeroArray(ChartFilterType filterType)
    {
        var length = 0;

        switch (filterType)
        {
            case ChartFilterType.Week:
                length = 7;
                break;
            case ChartFilterType.Month:
                length = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                break;
            case ChartFilterType.Year:
                length = 12;
                break;
            // Handle other cases if needed

            default:
                throw new ArgumentException("Unsupported filter type");
        }

        var zeroArray = new int[length];
        return zeroArray;
    }
}