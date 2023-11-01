using AutoMapper;
using Ayura.API.Features.Community.DTOs;
using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Ayura.API.Services;

public class PostService : IPostService
{
    private readonly IMongoCollection<Post> _postCollection;
    private readonly IMongoCollection<User> _userCollection;
    private readonly IMapper _mapper;

    public PostService(IAyuraDatabaseSettings settings, IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _postCollection = database.GetCollection<Post>(settings.PostCollection);
        _userCollection = database.GetCollection<User>(settings.UserCollection);

        var mapperConfig = new MapperConfiguration(cfg => { cfg.CreateMap<PostRequest, Post>(); });
        _mapper = mapperConfig.CreateMapper();
    }

    // public async Task<List<PostRequest>> GetPosts(string communityId)
    // {
    //     var filter = Builders<Post>.Filter.Eq("CommunityId", communityId);
    //
    //     var posts = await _postCollection.Find(filter).ToListAsync();
    //
    //     var user = GetUserById();
    //     return posts;
    // }

    public async Task<List<PostRequest>> GetPosts(string communityId)
    {
       var postRequestsDtos = new List<PostRequest>(); // List of POST REQUEST DTOs
        
        // Fetch Posts from Posts Collection
        var filter = Builders<Post>.Filter.Eq("CommunityId", communityId);
        var posts = await _postCollection.Find(filter).ToListAsync();

        // Add Post author name and create a dto list
        foreach (var post in posts)
        {
            // Retrieve the author's information from the userCollection using AuthorId
            var author = await _userCollection.Find(u => u.Id == post.AuthorId).FirstOrDefaultAsync();
            
            var authorName = $"{author.FirstName} {author.LastName}";
            
            var postDto = new PostRequest
            {
                PostId = post.Id,
                CommunityId = post.CommunityId,
                AuthorId = post.AuthorId,
                AuthorName = authorName, 
                Caption = post.Caption,
                ImageUrl = post.ImageUrl,
                CreatedAt = post.CreatedAt,
                Comments = post.Comments
            };
            
            postRequestsDtos.Add(postDto);
        }
        
        return postRequestsDtos;
    }


    // 2. Get a post by Id
    public async Task<Post> GetPost(string id) =>
        await _postCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
    
    // public async Task<List<Post>> GetPosts(string communityId)
    // {
    //     var filter = Builders<Post>.Filter.Eq("CommunityId", communityId);
    //     return await _postCollection.Find(filter).ToListAsync();
    // }
    
    public async Task<Post> CreatePost(Post post)
    {
        post.CreatedAt = DateTime.UtcNow; // Set the timestamp to current UTC time
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

    public async Task DeletePost(string id)
    {
        await _postCollection.DeleteOneAsync(c => c.Id == id);

    }

}