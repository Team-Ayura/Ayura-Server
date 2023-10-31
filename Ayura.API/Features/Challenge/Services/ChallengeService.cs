using MongoDB.Driver;
using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using AutoMapper;
using Ayura.API.Features.Challenge.Services;
using MongoDB.Bson;

namespace Ayura.API.Services;

public class ChallengeService : IChallengeService
{
    private readonly IMongoCollection<Challenge> _challengeCollection;
    private readonly IMongoCollection<Community> _communityCollection;

    public ChallengeService(IAyuraDatabaseSettings settings, IMongoClient mongoClient)
    {
        //database and collections setup
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _challengeCollection = database.GetCollection<Challenge>(settings.ChallengeCollection);
        _communityCollection = database.GetCollection<Community>(settings.CommunityCollection);
    }
    
    //1. Get all challenges of a community
    public async Task<List<Challenge>> GetChallenges(string id)
    {
        var communityFilter = Builders<Community>.Filter.Eq(c => c.Id, id);
        var community = await _communityCollection.Find(communityFilter).SingleOrDefaultAsync();

        if (community == null)
        {
            // Handle the case where the community with the given ID doesn't exist.
            return new List<Challenge>(); // Or throw an exception or return null as needed.
        }

        var challengeIds = community.Challenges;

        // Use the list of Ids to retrieve the associated challenges.
        var challengeFilter = Builders<Challenge>.Filter.In(c => c.Id, challengeIds);
        var challenges = await _challengeCollection.Find(challengeFilter).ToListAsync();

        return challenges; 
    }
    
    //2. Get a challenge by Id
    public async Task<Challenge> GetChallengeById(string id)
    {
        return await _challengeCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
    }
    
    //3. Create a challenge
    public async Task<Challenge> CreateChallenge(Challenge challenge)
    {
        try
        {
            // Step 1: Insert the challenge into the database
            await _challengeCollection.InsertOneAsync(challenge);

            // Step 2: Retrieve the associated community by its ID
            var communityId = challenge.CommunityId;
            var communityFilter = Builders<Community>.Filter.Eq(c => c.Id, communityId);
            var community = await _communityCollection.Find(communityFilter).SingleOrDefaultAsync();

            if (community != null)
            {
                
                    community.Challenges.Add(challenge.Id);

                    // Step 4: Update the community document with an atomic $push
                    var communityUpdateFilter = Builders<Community>.Filter.Eq(c => c.Id, community.Id);
                    var pushUpdate = Builders<Community>.Update.Push(c => c.Challenges, challenge.Id);

                    await _communityCollection.UpdateOneAsync(communityUpdateFilter, pushUpdate);
                
            }

            // Return the created challenge
            return challenge;
        }
        catch (Exception ex)
        {
            // Handle and log the exception as needed
            Console.WriteLine(ex);
            // You can return an error response here if necessary
            throw; // Re-throw the exception to indicate the failure
        }
    }

    // 5. Update a Challenge
    // If the ID is matched the DB.challenge which is c , it will be replaced by the challenge
    public async Task UpdateChallenge(Challenge updatedChallenge)
    {
        var filter = Builders<Challenge>.Filter.Eq(c => c.Id, updatedChallenge.Id);

        var update = Builders<Challenge>.Update
            .Set(c => c.Name, updatedChallenge.Name)
            .Set(c => c.Description, updatedChallenge.Description)
            .Set(c => c.Duration, updatedChallenge.Duration)
            .Set(c => c.Distance, updatedChallenge.Distance)
            .Set(c => c.Type, updatedChallenge.Type)
            .Set(c => c.EndDate, updatedChallenge.EndDate);

        await _challengeCollection.UpdateOneAsync(filter, update);
    }
    
    //6. delete challenge
    public async Task DeleteChallenge(Challenge challenge)
    {
        
        
            // Step 1: Retrieve the associated community
            var communityId = challenge.CommunityId; 
            var communityFilter = Builders<Community>.Filter.Eq(c => c.Id, communityId);
            var community = await _communityCollection.Find(communityFilter).SingleOrDefaultAsync();

            if (community != null)
            {
                // Step 2: Remove the challenge ID from the community's list of challenges
                if (challenge.Id != null) community.Challenges.Remove(challenge.Id);

                // Step 3: Update the community document in the Community collection
                var communityUpdateFilter = Builders<Community>.Filter.Eq(c => c.Id, community.Id);
                var communityUpdate = Builders<Community>.Update.Set(c => c.Challenges, community.Challenges);

                await _communityCollection.UpdateOneAsync(communityUpdateFilter, communityUpdate);
            }

            // Step 4: Delete the challenge from the Challenge collection
            await _challengeCollection.DeleteOneAsync(c => c.Id == challenge.Id);
        
        
    }
   
    //7. start challenge
    public async Task StartChallenge(string challengeId, string memberId)
    {

        // Step 1: Retrieve the associated challenge by its ID
        var challengeFilter = Builders<Challenge>.Filter.Eq(c => c.Id, challengeId);
        var challenge = await _challengeCollection.Find(challengeFilter).SingleOrDefaultAsync();

        if (challenge != null)
        {
            // Create a new leaderboard entry with the member ID and an initial score of 0
            var newEntry = new LeaderboardEntry()
            {
                MemberId = memberId,
                Score = 0
            };

            // Fetch the existing leaderboard entries
            var existingEntries = challenge.LeaderBoard ?? new List<LeaderboardEntry>();

            // Add the new entry to the existing entries
            existingEntries.Add(newEntry);

            // Update the challenge's leaderboard with the new entries
            challenge.LeaderBoard = existingEntries;

            // Update the challenge document in the database
            var challengeUpdateFilter = Builders<Challenge>.Filter.Eq(c => c.Id, challengeId);
            var challengeUpdate = Builders<Challenge>.Update.Set(c => c.LeaderBoard, existingEntries);

            await _challengeCollection.UpdateOneAsync(challengeUpdateFilter, challengeUpdate);
        }

    }
}