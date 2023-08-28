using Ayura.API.Features.Community.DTOs;
using Ayura.API.Models;
using MongoDB.Driver;
using Ayura.API.Models.Configuration;
using AutoMapper;
using MongoDB.Bson;


namespace Ayura.API.Services;

public class CommunityService : ICommunityService
{
    private readonly IMongoCollection<Community> _communityCollection;
    private readonly IMapper _mapper;

    public CommunityService(IAyuraDatabaseSettings settings, IMongoClient mongoClient)
    {
        // database and collections setup
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _communityCollection = database.GetCollection<Community>(settings.CommunityCollection);

        // DTO to model mapping setup
        var mapperConfig = new MapperConfiguration(cfg => { cfg.CreateMap<CommunityDto, Community>(); });

        _mapper = mapperConfig.CreateMapper();
    }

    // 1. Get All Communities - Note : Need to use userId to filter out the communities only User joined
    public async Task<List<Community>> GetCommunities() => await _communityCollection.Find(_ => true).ToListAsync();

    // 2. Get a community by Id
    public async Task<Community> GetCommunities(string id) =>
        await _communityCollection.Find(c => c.Id == id).FirstOrDefaultAsync();

    // 3. Create a Community
    public async Task<Community> CreateCommunity(Community community)
    {
        // Insert the community into the database
        await _communityCollection.InsertOneAsync(community);
        // Return the created community
        return community;
    }

    // 4. Update a Community
    // If the ID is matched the DB.community which is c , it will be replaced by the community
    public async Task UpdateCommunity(Community updatedCommunity)
    {
        // Retrieve the community by its ID
        var filter = Builders<Community>.Filter.Eq(c => c.Id, updatedCommunity.Id);
        var existingCommunity = await _communityCollection.Find(filter).FirstOrDefaultAsync();
        // => await _communityCollection.ReplaceOneAsync(c => c.Id == community.Id, community);
        
        if (existingCommunity != null)
        {
            // Preserve the existing Members list in the updatedCommunity
            updatedCommunity.Members = existingCommunity.Members;
            
            // Update the community in the database
            await _communityCollection.ReplaceOneAsync(filter, updatedCommunity);
        }
        
    }

    // 5. Delete a Community
    public async Task DeleteCommunity(string id) =>
        await _communityCollection.DeleteOneAsync(c => c.Id == id);

    // 6. Add a member/user to the community
    public async Task AddMember(string communityId, string userId)
    {
        // Retrieve the community by its ID
        var filter = Builders<Community>.Filter.Eq(c => c.Id, communityId);
        var community = await _communityCollection.Find(filter).FirstOrDefaultAsync();

       var userIdObjectId = ObjectId.Parse(userId);

        community.Members.Add(userIdObjectId);

        // Update the community object in the MongoDB collection
        var update = Builders<Community>.Update.Set(c => c.Members, community.Members);
        await _communityCollection.UpdateOneAsync(filter, update);
    }
    
}