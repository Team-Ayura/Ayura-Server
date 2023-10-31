namespace Ayura.API.Features.Challenge.Services;
using Ayura.API.Models;

public interface IChallengeService
{
    //1. Get all challenges of community
    Task<List<Challenge>> GetChallenges(string id);
    
    //2. Get a challenge by Id
    Task<Challenge> GetChallengeById(string id);
    
    //3. Create a Challenge
    Task<Challenge> CreateChallenge(Challenge challenge);
    
    // 4. Update a community
    Task UpdateChallenge(Challenge challenge);

    // 5. Delete a community
    Task DeleteChallenge(Challenge challenge);

    // 6. Start challenge
    Task StartChallenge(string challengeId, string memberId);

}