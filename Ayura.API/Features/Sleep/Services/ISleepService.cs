using Ayura.API.Features.Sleep.DTOs;

namespace Ayura.API.Features.Sleep.Services;

public interface ISleepService
{
    Task<string> AddSleepData(AddSleepDataDto addSleepDataDto);

    Task<object> GetSleepingData(string userId, string filterType);
}