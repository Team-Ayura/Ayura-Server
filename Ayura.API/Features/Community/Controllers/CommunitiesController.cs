using Ayura.API.Features.Community.DTOs;
using Ayura.API.Services;
using Microsoft.AspNetCore.Mvc;
using CommunityModel = Ayura.API.Models.Community; // Create an alias for the type
using PostModel = Ayura.API.Models.Post; // Create an alias for the type
using CommentModel = Ayura.API.Models.Comment; // Create an alias for the type

namespace Ayura.API.Features.Community.Controllers;

[ApiController]
[Route("api/communities")]
public class CommunitiesController : Controller
{
    private readonly CommunityService _communityService; // Getting Service
    private readonly PostService _postService;
    private readonly CommentService _commentService;

    // Community Service is injected on to the controller
    // This is Constructor as an arrow function
    public CommunitiesController(CommunityService communityService, PostService postService, CommentService commentService) {
        _communityService = communityService;
        _postService = postService;
        _commentService = commentService;
        
    }


    // From here onwards methods
    // 1. GET ALL PUBLIC Communities
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var allCommunities = await _communityService.GetPublicCommunities();
        if (allCommunities.Any()) //Check whether there are any drivers in the collection
        {
            return Ok(allCommunities);
        }

        return NotFound(new { Message = "No Communities Found" });
    }

    // 2. GET a community by ID 
    [HttpGet("{communityId:length(24)}")] // Constraint to check whether it has 24 chars
    public async Task<IActionResult> Get(string communityId)
    {
        var existingCommunity = await _communityService.GetCommunityById(communityId);
        if (existingCommunity is null)
        {
            return NotFound(new { Message = "Community not found." });
        }

        return Ok(existingCommunity);
    }

    // 3.  Get user joined communities 
    [HttpGet("joined/{userId:length(24)}")] // Constraint to check whether it has 24 chars
    public async Task<IActionResult> GetJoinedCommunities(string userId)
    {
        var joinedCommunities = await _communityService.GetJoinedCommunities(userId);
        if (joinedCommunities.Any())
        {
            return Ok(new
            {
                Message = "Community user joined",
                communities = joinedCommunities
            });
        }

        // Handle the case when the user is not found or has no joined communities.
        return NotFound(new
        {
            Message = "there are no joined communities or the user is not found."
        });
    }

    // 3. Create a Community
    [HttpPost("create")]
    public async Task<IActionResult> CreateCommunity(CommunityModel community)
    {
        var createdCommunity = await _communityService.CreateCommunity(community);
        return CreatedAtAction("Get", new { id = createdCommunity.Id }, createdCommunity);
    }

    // 4. Update a Community
    [HttpPut("{communityId:length(24)}")]
    public async Task<IActionResult> UpdateCommunity(string communityId, CommunityModel updatedCommunity)
    {
        //Get the community from DB
        var existingCommunity = await _communityService.GetCommunityById(communityId);

        if (existingCommunity is null)
        {
            return NotFound(new { Message = "Community not found." });
        }

        updatedCommunity.Id = existingCommunity.Id;
        // Since this call Company Model the MembersList will be an empty String

        // Preserve old MembersList
        updatedCommunity.Members = existingCommunity.Members;
        await _communityService.UpdateCommunity(updatedCommunity);

        // Create the response object
        var response = new
        {
            Message = "Community updated successfully.",
            Community = updatedCommunity // Include the updated community
        };

        return Ok(updatedCommunity);
    }

    // 5. Delete a community
    [HttpDelete("{communityId:length(24)}")]
    public async Task<IActionResult> Delete(string communityId)
    {
        var existingCommunity = await _communityService.GetCommunityById(communityId);

        if (existingCommunity is null)
        {
            return NotFound(new { Message = "Community not found." });
        }

        await _communityService.DeleteCommunity(existingCommunity);
        return Ok(new
        {
            Message = "Community deleted successfully.",
        });
    }

    //6. Adding a member to Community
    [HttpPut("addMember")]
    public async Task<IActionResult> AddMember([FromBody] MemberRequest memberRequest)
    {
        var community = await _communityService.AddMember(memberRequest.CommunityId, memberRequest.UserId);

        return community.Id == null
            ? NotFound(new { Message = "Member is already added" })
            : Ok(new { Message = "Member Added successfully.", Community = community });
    }
    
    //7. Get all posts of a community
    [HttpGet("posts/{communityId:length(24)}")]
    public async Task<IActionResult> GetCommunityPosts(string communityId)
    {
        try
        {
            var existingCommunity = await _communityService.GetCommunities(communityId);
            if (existingCommunity is null)
            {
                return NotFound(new { Message = "Community not found." });
            }

            var allPosts = await _postService.GetPosts(communityId);
            return Ok(allPosts);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            // Log the exception and return an appropriate error response
            return StatusCode(500, new { Message = "An error occurred while processing your request." });
        }
    }
    
    // 2. GET a post by ID
    [HttpGet("post/{id:length(24)}")] // Constraint to check whether it has 24 chars
    public async Task<IActionResult> GetPost(string id)
    {
        var existingPost = await _postService.GetPost(id);
        if (existingPost is null)
        {
            return NotFound(new { Message = "Post not found." });
        }

        return Ok(existingPost);
    }
    
    //8.Adding a post to community
    [HttpPost("post")]
    public async Task<IActionResult> CreatePost(PostModel post)
    {
        try{
            var existingCommunity = await _communityService.GetCommunities(post.CommunityId);
            if (existingCommunity is null)
            {
                return NotFound(new { Message = "Community not found." });
            }

            var createdPost = await _postService.CreatePost(post);
            return CreatedAtAction("Get", new { id = createdPost.Id }, createdPost);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            // Log the exception and return an appropriate error response
            return StatusCode(500, new { Message = "An error occurred while processing your request." });
        }
    }

    //9.Edit post
    [HttpPut("post/{id:length(24)}")]
    public async Task<IActionResult> UpdatePost(string id, PostModel updatedPost)
    {
        //Get the post from DB
        var existingPost = await _postService.GetPost(id);

        if (existingPost is null)
        {
            return NotFound(new { Message = "Post not found." });
        }

        updatedPost.Id = existingPost.Id;
        // Since this call Company Model the CommentList will be an empty String
        // Preserve old MembersList
        updatedPost.Comments = existingPost.Comments;
        await _postService.UpdatePost(updatedPost);
        
        // Create the response object
        var response = new
        {
            Message = "Post updated successfully.",
            Post = updatedPost // Include the updated community
        };

        return Ok(response);
    }
    
    //10. delete post 
    [HttpDelete("post/{id:length(24)}")]
    public async Task<IActionResult> DeletePost(string id)
    {
       
        try
        {
            var existingPost = await _postService.GetPost(id);
        
            if (existingPost == null)
            {
                return NotFound("Post not found.");
            }

            await _postService.DeletePost(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred");
        }
    }
    
    //11. create comment
    [HttpPost("comment")]
    public async Task<IActionResult> AddComment(CommentModel comment)
    {
        var createdComment = await _commentService.CreateComment(comment);
        return CreatedAtAction("Get", new { id = createdComment.Id }, createdComment);
    }
    
    //12. edit comment
    [HttpPut("comment")]
    public async Task<IActionResult> EditComment(CommentModel updatedComment)
    {
        try
        {
            await _commentService.UpdateComment(updatedComment);
            return Ok("Comment updated successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
    
    //13. delete comment 
    [HttpDelete("comment/{id:length(24)}")]
    public async Task<IActionResult> DeleteComment(string id)
    {
       
        try
        {
            var existingComment = await _commentService.GetComment(id);
        
            if (existingComment == null)
            {
                return NotFound("Comment not found.");
            }

            await _commentService.DeleteComment(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred");
        }
    }
    
}