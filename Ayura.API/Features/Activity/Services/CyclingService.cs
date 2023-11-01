using System.Globalization;
using AutoMapper;
using Ayura.API.Features.Activity.DTOs;
using Ayura.API.Features.Activity.Models;
using Ayura.API.Global.Constants;
using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Ayura.API.Features.Activity.Services;

public class CyclingService : ICyclingService
{
    private readonly IMapper _mapper;
    private readonly IMongoCollection<User> _userCollection;

    public CyclingService(IAyuraDatabaseSettings settings, IMongoClient mongoClient)
    {
        // database and collections setup
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _userCollection = database.GetCollection<User>(settings.UserCollection);

        // DTO to model mapping setup
        var mapperConfig = new MapperConfiguration(cfg => { cfg.CreateMap<AddCyclingRequest, CyclingHistory>(); });

        _mapper = mapperConfig.CreateMapper();
    }

    // 1. Get walk and running data by a filter (day, week, month or year)
    public async Task<object> GetCyclingData(string userId, string filterType)
    {
        // define the result
        var response = new CyclingDataRespose();
        // identify the time duration.
        var activityfilter = (ChartFilterType)Enum.Parse(typeof(ChartFilterType), filterType);
        DateTime startingDate;
        var endingDate = DateTime.Today;
        var today = DateTime.Today;

        switch (activityfilter)
        {
            case ChartFilterType.Day:
                startingDate = new DateTime(today.Year, today.Month, today.Day);
                break;
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
        if (activityfilter == ChartFilterType.Day) response.timePeriod = $"{startingDate.ToString("MMM dd")}";

        // fetch average distance covered in the above duration
        response.avgDistanceCycled = await _getAverageDistaceCovered(userId, startingDate, endingDate);

        // fetch average step count covered in the above duration
        // response.avgStepCount = await _getAverageStepCount(userId, startingDate, endingDate);

        // fetch average movement minutes in the above duration
        response.avgDuration = await _getAverageMoveMinutes(userId, startingDate, endingDate);

        // fetch average calories burned in the above duration
        response.avgCaloriesBurned = await _getAverageCaloriesBurned(userId, startingDate, endingDate);

        // calculate the improvement (current average vs average in th period)
        response.improvement = await _getDistanceImprovement(userId, response.avgDistanceCycled);

        // fetch the step count in the above duration
        response.distances = await _getDistances(userId, activityfilter, startingDate, endingDate);

        response.cyclingHistory = await _getCyclingHistory(userId, startingDate, endingDate);
        return response;
    }

    // 2. Add cycling data each day at a spesific time
    public async Task<string> AddCyclingData(AddCyclingRequest addCyclingRequest)
    {
        var today = DateTime.Today;

        // map signin request to a user
        var cyclingTrip = new CyclingHistory
        {
            Id = ObjectId.GenerateNewId().ToString(),
            StartTime = addCyclingRequest.StartTime,
            EndTime = addCyclingRequest.EndTime,
            Distance = addCyclingRequest.Distance,
            Duration = addCyclingRequest.Duration,
            CaloriesBurned = addCyclingRequest.CaloriesBurned,
            Path = addCyclingRequest.Path,
            Images = addCyclingRequest.Images
        };

        var filter = Builders<User>.Filter.Eq(u => u.Id, addCyclingRequest.UserId);
        var update = Builders<User>.Update.Push<CyclingHistory>(u => u.MeasurableActivities.Cycling, cyclingTrip);

        await _userCollection.UpdateOneAsync(filter, update);

        return "success";
    }

    // 3. Get Today's improvement for distance
    public async Task<int> GetTodayImprovement(string userId, int todayStepCount)
    {
        return await _getDistanceImprovement(userId, todayStepCount);
    }

    // Private method which handles each individual task in the get request
    private async Task<double> _getAverageDistaceCovered(string id, DateTime startingDate, DateTime endingDate)
    {
        var pipeline = new BsonDocument[]
        {
            new("$match", new BsonDocument("_id", ObjectId.Parse(id))),
            new("$unwind", "$measurableActivities.cycling"),
            new("$match", new BsonDocument
            {
                {
                    "$and", new BsonArray
                    {
                        new BsonDocument("measurableActivities.cycling.startTime",
                            new BsonDocument("$gte", BsonDateTime.Create(startingDate))),
                        new BsonDocument("measurableActivities.cycling.endTime",
                            new BsonDocument("$lte", BsonDateTime.Create(endingDate)))
                    }
                }
            }),
            new("$group", new BsonDocument
            {
                {
                    "_id", new BsonDocument
                    {
                        {
                            "$dateToString", new BsonDocument
                            {
                                { "format", "%Y-%m-%d" },
                                { "date", "$measurableActivities.cycling.startTime" }
                            }
                        }
                    }
                },
                { "averageDistaceCycled", new BsonDocument("$avg", "$measurableActivities.cycling.distance") }
            })
        };


        var aggregationCursor = await _userCollection.AggregateAsync<BsonDocument>(pipeline);
        var result = await aggregationCursor.FirstOrDefaultAsync();

        if (result != null && result.TryGetValue("averageDistaceCycled", out var averageDistaceCycledValue))
        {
            var averageDistaceCycled = averageDistaceCycledValue.AsDouble;

            Console.WriteLine($"Average Step Count: {averageDistaceCycled}");
            return averageDistaceCycled;
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
            new("$unwind", "$measurableActivities.cycling"),
            new("$match", new BsonDocument
            {
                {
                    "$and", new BsonArray
                    {
                        new BsonDocument("measurableActivities.cycling.startTime",
                            new BsonDocument("$gte", BsonDateTime.Create(startingDate))),
                        new BsonDocument("measurableActivities.cycling.endTime",
                            new BsonDocument("$lte", BsonDateTime.Create(endingDate)))
                    }
                }
            }),
            new("$group", new BsonDocument
            {
                {
                    "_id", new BsonDocument
                    {
                        {
                            "$dateToString", new BsonDocument
                            {
                                { "format", "%Y-%m-%d" },
                                { "date", "$measurableActivities.cycling.startTime" }
                            }
                        }
                    }
                },
                { "averageDurationCycled", new BsonDocument("$avg", "$measurableActivities.cycling.duration") }
            })
        };


        var aggregationCursor = await _userCollection.AggregateAsync<BsonDocument>(pipeline);
        var result = await aggregationCursor.FirstOrDefaultAsync();

        if (result != null && result.TryGetValue("averageDurationCycled", out var averageMoveMinutesValue))
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
            new("$unwind", "$measurableActivities.cycling"),
            new("$match", new BsonDocument
            {
                {
                    "$and", new BsonArray
                    {
                        new BsonDocument("measurableActivities.cycling.startTime",
                            new BsonDocument("$gte", BsonDateTime.Create(startingDate))),
                        new BsonDocument("measurableActivities.cycling.endTime",
                            new BsonDocument("$lte", BsonDateTime.Create(endingDate)))
                    }
                }
            }),
            new("$group", new BsonDocument
            {
                {
                    "_id", new BsonDocument
                    {
                        {
                            "$dateToString", new BsonDocument
                            {
                                { "format", "%Y-%m-%d" },
                                { "date", "$measurableActivities.cycling.startTime" }
                            }
                        }
                    }
                },
                { "averageCaloriesBurned", new BsonDocument("$avg", "$measurableActivities.cycling.caloriesBurned") }
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

    private async Task<int> _getDistanceImprovement(string id, double currentAvgDistanceCycled)
    {
        var pipeline = new BsonDocument[]
        {
            new("$match", new BsonDocument("_id", ObjectId.Parse(id))),
            new("$unwind", "$measurableActivities.cycling"),
            new("$group", new BsonDocument
            {
                {
                    "_id", new BsonDocument
                    {
                        {
                            "$dateToString", new BsonDocument
                            {
                                { "format", "%Y-%m-%d" },
                                { "date", "$measurableActivities.cycling.startTime" }
                            }
                        }
                    }
                },
                { "averageDistaceCycled", new BsonDocument("$avg", "$measurableActivities.cycling.distance") }
            })
        };

        var aggregationCursor = await _userCollection.AggregateAsync<BsonDocument>(pipeline);
        var result = await aggregationCursor.FirstOrDefaultAsync();
        Console.WriteLine("Check below line");
        Console.WriteLine(result);

        if (result != null && result.TryGetValue("averageDistaceCycled", out var averageDistanceValue))
        {
            var averageDistance = averageDistanceValue.AsDouble;
            return (int)((currentAvgDistanceCycled - averageDistance) * 100 / averageDistance);
        }

        return 0;
    }

    private async Task<List<int>> _getDistances(string id, ChartFilterType filterType, DateTime startingDate,
        DateTime endingDate)
    {
        var distance = GenerateZeroArray(filterType);
        var index = 0;
        var currentYear = DateTime.UtcNow.Year;
        var pipeline = Array.Empty<BsonDocument>();
        // pipeline for the filter type Year
        switch (filterType)
        {
            case ChartFilterType.Day:
                // aggregation pipline for the type week
                pipeline = new BsonDocument[]
                {
                    new("$match", new BsonDocument("_id", ObjectId.Parse(id))),
                    new("$unwind", "$measurableActivities.cycling"),
                    new("$match", new BsonDocument
                    {
                        {
                            "measurableActivities.cycling.startTime",
                            new BsonDocument
                            {
                                { "$gte", startingDate },
                                { "$lte", endingDate }
                            }
                        }
                    }),
                    new("$project", new BsonDocument
                    {
                        { "hourOfDay", new BsonDocument("$hour", "$measurableActivities.cycling.startTime") },
                        { "distance", "$measurableActivities.cycling.distance" }
                    }),
                    new("$group", new BsonDocument
                    {
                        { "_id", "$hourOfDay" },
                        { "distance", new BsonDocument("$sum", "$distance") }
                    })
                };


                break;
            case ChartFilterType.Week:
                // aggregation pipline for the type week
                pipeline = new BsonDocument[]
                {
                    new("$match", new BsonDocument("_id", ObjectId.Parse(id))),
                    new("$unwind", "$measurableActivities.cycling"),
                    new("$match", new BsonDocument
                    {
                        {
                            "measurableActivities.cycling.startTime",
                            new BsonDocument
                            {
                                { "$gte", startingDate },
                                { "$lte", endingDate }
                            }
                        }
                    }),
                    new("$project", new BsonDocument
                    {
                        { "dayOfWeek", new BsonDocument("$dayOfWeek", "$measurableActivities.cycling.startTime") },
                        {
                            "date", new BsonDocument("$dateToString", new BsonDocument
                            {
                                { "format", "%Y-%m-%d" },
                                { "date", "$measurableActivities.cycling.startTime" }
                            })
                        },
                        { "distance", "$measurableActivities.cycling.distance" }
                    }),
                    new("$group", new BsonDocument
                    {
                        {
                            "_id", new BsonDocument
                            {
                                { "dayOfWeek", "$dayOfWeek" },
                                { "date", "$date" }
                            }
                        },
                        { "totalDistance", new BsonDocument("$sum", "$distance") }
                    }),
                    new("$project", new BsonDocument
                    {
                        { "unit", "$_id.dayOfWeek" },
                        { "distance", "$totalDistance" }
                    })
                };

                break;
            case ChartFilterType.Month:
                // aggregation pipline for the type month
                pipeline = new BsonDocument[]
                {
                    new("$match", new BsonDocument("_id", ObjectId.Parse(id))),
                    new("$unwind", "$measurableActivities.cycling"),
                    new("$match", new BsonDocument
                    {
                        {
                            "measurableActivities.cycling.startTime",
                            new BsonDocument
                            {
                                { "$gte", startingDate },
                                { "$lte", endingDate }
                            }
                        }
                    }),
                    new("$project", new BsonDocument
                    {
                        { "dayOfMonth", new BsonDocument("$dayOfMonth", "$measurableActivities.cycling.startTime") },
                        {
                            "date", new BsonDocument("$dateToString", new BsonDocument
                            {
                                { "format", "%Y-%m-%d" },
                                { "date", "$measurableActivities.cycling.startTime" }
                            })
                        },
                        { "distance", "$measurableActivities.cycling.distance" }
                    }),
                    new("$group", new BsonDocument
                    {
                        {
                            "_id", new BsonDocument
                            {
                                { "dayOfMonth", "$dayOfMonth" },
                                { "date", "$date" }
                            }
                        },
                        { "totalDistance", new BsonDocument("$sum", "$distance") }
                    }),
                    new("$project", new BsonDocument
                    {
                        { "unit", "$_id.dayOfMonth" },
                        { "distance", "$totalDistance" }
                    })
                };

                break;
            default:
                // aggregation pipline for the type year
                pipeline = new BsonDocument[]
                {
                    new("$match", new BsonDocument("_id", ObjectId.Parse(id))),
                    new("$unwind", "$measurableActivities.cycling"),
                    new("$match", new BsonDocument
                    {
                        {
                            "measurableActivities.cycling.startTime",
                            new BsonDocument
                            {
                                { "$gte", startingDate },
                                { "$lte", endingDate }
                            }
                        }
                    }),
                    new("$project", new BsonDocument
                    {
                        { "monthOfYear", new BsonDocument("$month", "$measurableActivities.cycling.startTime") },
                        { "dayOfMonth", new BsonDocument("$dayOfMonth", "$measurableActivities.cycling.startTime") },
                        { "distance", "$measurableActivities.cycling.distance" }
                    }),
                    new("$group", new BsonDocument
                    {
                        {
                            "_id", new BsonDocument
                            {
                                { "monthOfYear", "$monthOfYear" },
                                { "dayOfMonth", "$dayOfMonth" }
                            }
                        },
                        { "dailyTotalDistance", new BsonDocument("$sum", "$distance") },
                        { "count", new BsonDocument("$sum", 1) } // Count the number of entries for each day
                    }),
                    new("$group", new BsonDocument
                    {
                        { "_id", "$_id.monthOfYear" }, // Group by month
                        { "averageDistance", new BsonDocument("$avg", "$dailyTotalDistance") }
                    }),
                    new("$project", new BsonDocument
                    {
                        { "unit", "$_id" }, // Rename _id as unit
                        { "distance", "$averageDistance" }
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
                result.TryGetValue("distance", out var distancecycled))
            {
                while (index + 1 < (int)month) index++;

                if ((int)month == index + 1) distance[index] = (int)distancecycled.AsDouble;
            }

            index++;
        }

        Console.WriteLine(distance);
        return distance.ToList();
    }

    private async Task<List<CyclingHistory>> _getCyclingHistory(string id, DateTime startingDate, DateTime endingDate)
    {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Id, id), // Filter by user ID
            Builders<User>.Filter.ElemMatch(u => u.MeasurableActivities.Cycling, ch =>
                ch.StartTime >= startingDate && ch.EndTime <= endingDate) // Filter cycling trips within time range
        );

        var user = await _userCollection.Find(filter).FirstOrDefaultAsync();

        if (user != null)
        {
            var cyclingTrips = user.MeasurableActivities.Cycling.Take(5).ToList();
            return cyclingTrips;
            // cyclingTrips now contains a maximum of 5 cycling trips within the specified time range.
        }

        return new List<CyclingHistory>();
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
            case ChartFilterType.Day:
                length = 24;
                break;
            // Handle other cases if needed

            default:
                throw new ArgumentException("Unsupported filter type");
        }

        var zeroArray = new int[length];
        return zeroArray;
    }
}