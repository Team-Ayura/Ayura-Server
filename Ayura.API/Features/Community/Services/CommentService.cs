using AutoMapper;
using Ayura.API.Features.Community.DTOs;
using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
namespace Ayura.API.Services;

public class CommentService : ICommentService
{
    private readonly IMongoCollection<Comment> _commentCollection;
    private readonly IMongoCollection<Post> _postCollection;
    private readonly IMapper _mapper;
    
    public CommentService(IAyuraDatabaseSettings settings, IMongoClient mongoClient)
    {
        // database and collections setup
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _commentCollection = database.GetCollection<Comment>(settings.CommentCollection);
        _postCollection = database.GetCollection<Post>(settings.PostCollection);

        // DTO to model mapping setup
        var mapperConfig = new MapperConfiguration(cfg => { cfg.CreateMap<CommentDto, Comment>(); });

        _mapper = mapperConfig.CreateMapper();
    }
    //get by id 
    public async Task<Comment> GetComment(string id) => await _commentCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
    
    
    //create a comment
    public async Task<Comment> CreateComment(Comment comment)
    {
        await _commentCollection.InsertOneAsync(comment);

        var postFilter = Builders<Post>.Filter.Eq(p => p.Id, comment.PostId);
        var update = Builders<Post>.Update.Push(p => p.Comments, comment);
    
        await _postCollection.UpdateOneAsync(postFilter, update);

        return comment;
    }
    
    //edit comment
    public async Task UpdateComment(Comment updatedComment)
    {
        var commentFilter = Builders<Comment>.Filter.Eq(c => c.Id, updatedComment.Id);
        var existingcomment = await _commentCollection.Find(commentFilter).FirstOrDefaultAsync();

        if (existingcomment != null)
        {
            var update = Builders<Comment>.Update
                .Set(c => c.Content, updatedComment.Content);

            await _commentCollection.UpdateOneAsync(commentFilter, update);
            
            // Update the corresponding comment in the Post collection
            var postFilter = Builders<Post>.Filter.And(
                Builders<Post>.Filter.Eq(p => p.Id, updatedComment.PostId),
                Builders<Post>.Filter.ElemMatch(p => p.Comments, c => c.Id == updatedComment.Id)
            );

            var postUpdate = Builders<Post>.Update.Set("comments.$.content", updatedComment.Content);

            await _postCollection.UpdateOneAsync(postFilter, postUpdate);
            
        }

       
    }


    
    //delete comment
    public async Task DeleteComment(string id) => await _commentCollection.DeleteOneAsync(c => c.Id == id);
}