using Ayura.API.Models;

namespace Ayura.API.Services;

public interface ICommentService
{
    //get by id
    Task<Comment> GetComment(string id);
    //create comment
    Task<Comment> CreateComment(Comment comment);
    
    //update comment 
    Task UpdateComment(string commentContent, string commentId);
    
    //delete comment
    Task DeleteComment(string id);

}