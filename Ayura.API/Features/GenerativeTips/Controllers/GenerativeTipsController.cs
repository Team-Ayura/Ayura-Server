using System.CodeDom.Compiler;
using Ayura.API.Features.GPT.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Completions;

namespace Ayura.API.Features.GPT;

public class GenerativeTipsController : Controller
{
    [HttpPost]
    [Route("api/gpt")]
    public async Task<IActionResult> GetGptResponse([FromBody] TestPrompt testprompt)
    {
        var openAi = new OpenAIAPI("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");

        List<string> result = new List<string>();
        
        // Create a prompt using the testprompt.prompt string
        string Finalprompt = testprompt.prompt;
        
        Console.WriteLine(Finalprompt);

        CompletionRequest completionRequest = new CompletionRequest();
        completionRequest.Prompt = Finalprompt;
        completionRequest.MaxTokens = 1000;
        completionRequest.Model = OpenAI_API.Models.Model.DavinciText;

        var completions = openAi.Completions.CreateCompletionAsync(completionRequest);

        foreach (var comp in completions.Result.Completions)
        {
            Console.WriteLine(comp.Text);
            result.Add(comp.Text);
        }

        return Ok(result);

    }
} 


public class TestObjDTO
{
    public string name { get; set; }
    public int age { get; set; }
}

public class TestPrompt
{
    public string prompt { get; set; }
}