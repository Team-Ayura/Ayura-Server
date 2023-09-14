using Ayura.API.Features.Activity.DTOs;

namespace Ayura.API.Features.Activity.Services;

public interface ICyclingService
{
    Task<object> GetCyclingData(string userId, string filterType);
    Task<int> GetTodayImprovement(string userId, int todayStepCount);
    Task<string> AddCyclingData(AddCyclingRequest addCyclingRequest);
}