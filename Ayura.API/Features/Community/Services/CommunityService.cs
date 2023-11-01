using AutoMapper;
using Ayura.API.Features.Community.DTOs;
using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Ayura.API.Services;

public class CommunityService : ICommunityService
{
    private readonly IMongoCollection<Community> _communityCollection;
    private readonly IMapper _mapper;
    private readonly IMongoCollection<User> _userCollection;

    public CommunityService(IAyuraDatabaseSettings settings, IMongoClient mongoClient)
    {
        // database and collections setup
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _communityCollection = database.GetCollection<Community>(settings.CommunityCollection);
        _userCollection = database.GetCollection<User>(settings.UserCollection);
        // DTO to model mapping setup
        var mapperConfig = new MapperConfiguration(cfg => { cfg.CreateMap<CommunityDto, Community>(); });

        _mapper = mapperConfig.CreateMapper();
    }

    // 1. Get All PUBLIC Communities 
    public async Task<List<Community>> GetPublicCommunities(string userId)
    {
        var filter = Builders<Community>.Filter.Eq(c => c.IsPublic, true);
        var publicCommunities = await _communityCollection.Find(filter).ToListAsync();

        // Filter out communities that the user has joined
        var communitiesNotJoinedByUser = publicCommunities.Where(c => !c.Members.Contains(userId)).ToList();
        return communitiesNotJoinedByUser;
    }

    // 2. Get a community by Id
    public async Task<Community> GetCommunityById(string id)
    {
        return await _communityCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
    }


    // 3. Get all the user Joined communities
    public async Task<List<Community>> GetJoinedCommunities(string userId)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var user = await _userCollection.Find(filter).FirstOrDefaultAsync();

        if (user == null)
        {
            // [IN PROGRESS] Handle the case when the user is not found or has no joined communities
            return new List<Community>();
        }

        //Object Ids of joined Communities
        var joinedCommunityIds = user.JoinedCommunities;

        // Convert Community Ids to strings
        var joinedCommunityIdStrings = joinedCommunityIds.Select(id => id.ToString());

        var communityFilter = Builders<Community>.Filter.In(c => c.Id, joinedCommunityIdStrings);

        var joinedCommunities = await _communityCollection.Find(communityFilter).ToListAsync();

        return joinedCommunities;
    }


    // 4. Create a Community
    public async Task<Community> CreateCommunity(Community community)
    {
        // Insert the community into the database
        await _communityCollection.InsertOneAsync(community);
        // Return the created community
        return community;
    }

    // 5. Update a Community
    // If the ID is matched the DB.community which is c , it will be replaced by the community
    public async Task UpdateCommunity(Community updatedCommunity)
    {
        // Retrieve the community by its ID
        var filter = Builders<Community>.Filter.Eq(c => c.Id, updatedCommunity.Id);
        var existingCommunity = await _communityCollection.Find(filter).FirstOrDefaultAsync();
        if (existingCommunity != null)
        {
            // Preserve the existing Members list in the updatedCommunity
            updatedCommunity.Members = existingCommunity.Members;

            // Update the community in the database
            await _communityCollection.ReplaceOneAsync(filter, updatedCommunity);
        }
    }

    // 6. Delete a Community
    public async Task DeleteCommunity(Community community)
    {
        // Step 1 - Find users who have joined the community using members[] in community document
        var usersWhoJoinedCommunity = await _userCollection
            .Find(u => u.JoinedCommunities.Contains(community.Id))
            .ToListAsync();

        // Step 2 - Remove the communityId from the joinedCommunities[] of each user document
        foreach (var user in usersWhoJoinedCommunity)
        {
            user.JoinedCommunities.Remove(community.Id);
            await _userCollection.ReplaceOneAsync(u => u.Id == user.Id, user);
        }

        // Step 3 - Delete the community
        await _communityCollection.DeleteOneAsync(c => c.Id == community.Id);
    }


    // 7. Add a member/user to the community
    public async Task<Community> AddMember(string communityId, string userId)
    {
        // Note - Must check whether user is already added from both community and user pov
        // Retrieve the community by its ID
        var communityFilter = Builders<Community>.Filter.Eq(c => c.Id, communityId);
        var community = await _communityCollection.Find(communityFilter).FirstOrDefaultAsync();

        // Retrieve the user by its ID
        var userFilter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var user = await _userCollection.Find(userFilter).FirstOrDefaultAsync();

        // Check if the user is not already a member of the community
        if (!community.Members.Contains(userId))
        {
            // Add the userId to the Members list of the community document
            community.Members.Add(userId);
            // Adding community ObjectId the user document
            user.JoinedCommunities.Add(communityId);

            // Update the community  in the MongoDB collection
            var communityUpdate = Builders<Community>.Update.Set(c => c.Members, community.Members);
            await _communityCollection.UpdateOneAsync(communityFilter, communityUpdate);

            // Update the user joined communities in the MongoDB collection
            var userUpdate = Builders<User>.Update.Set(u => u.JoinedCommunities, user.JoinedCommunities);
            await _userCollection.UpdateOneAsync(userFilter, userUpdate);

            return community;
        }

        // User is already added to the community
        return new Community();
    }


    // 8. Get user by Email
    public async Task<User> GetUserByEmail(string userEmail)
    {
        // Retrieve the user by its ID
        var userFilter = Builders<User>.Filter.Eq(u => u.Email, userEmail);
        var user = await _userCollection.Find(userFilter).FirstOrDefaultAsync();

        return user ?? new User();
    }

    public async Task<List<User>> GetCommunityMembers(string communityId)
    {
        // Retrieve the community by its ID
        var communityFilter = Builders<Community>.Filter.Eq(c => c.Id, communityId);
        var community = await _communityCollection.Find(communityFilter).FirstOrDefaultAsync();
        var memberIds = community.Members; // Get the Member Ids from the community

        // Query the user collection
        var userFilter = Builders<User>.Filter.In(u => u.Id, memberIds);

        var users = await _userCollection.Find(userFilter).ToListAsync();

        return users;
    }
}