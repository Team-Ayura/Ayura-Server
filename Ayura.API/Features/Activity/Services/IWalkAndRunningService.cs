using Ayura.API.Features.Activity.DTOs;

namespace Ayura.API.Features.Activity.Services;

public interface IWalkAndRunningService
{
    Task<object> GetWalkAndRunningData(string userId, string filterType);
    Task<int> GetTodayImprovement(string userId, int todayStepCount);
    Task<string> AddWalkAndRunningData(AddWalkAndRunnigRequest addWalkAndRunningRequest);
}