using Ayura.API.Features.MoodTracking.DTOs;

namespace Ayura.API.Features.MoodTracking.Services;

public interface IMoodService
{
    Task<String> AddMoodData(AddMoodRequestDTO addMoodRequestDto, string userId);
    
}