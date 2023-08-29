using AutoMapper;
using Ayura.API.Features.Community.DTOs;
using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using MongoDB.Driver;

namespace Ayura.API.Services;

public class PostService : IPostService
{
    private readonly IMongoCollection<Post> _postCollection;
    private readonly IMapper _mapper;

    public PostService(IAyuraDatabaseSettings settings, IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _postCollection = database.GetCollection<Post>(settings.PostCollection);

        var mapperConfig = new MapperConfiguration(cfg => { cfg.CreateMap<PostDto, Post>(); });
        _mapper = mapperConfig.CreateMapper();
    }

    public async Task<List<Post>> GetPosts(string communityId)
    {
        var filter = Builders<Post>.Filter.Eq("CommunityId", communityId);
        return await _postCollection.Find(filter).ToListAsync();
    }
    
    // 2. Get a post by Id
    public async Task<Post> GetPost(string id) =>
        await _postCollection.Find(c => c.Id == id).FirstOrDefaultAsync();

    
    public async Task<Post> CreatePost(Post post)
    {
        await _postCollection.InsertOneAsync(post);
        return post;
    }

    public async Task UpdatePost(Post updatedPost)
    {
        var filter = Builders<Post>.Filter.Eq(c => c.Id, updatedPost.Id);
        var existingPost = await _postCollection.Find(filter).FirstOrDefaultAsync();
        if (existingPost != null)
        {
            await _postCollection.ReplaceOneAsync(filter, updatedPost);
        }
        
    }

    public async Task DeletePost(string id) =>
        await _postCollection.DeleteOneAsync(c => c.Id == id);

}