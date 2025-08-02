using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

// See https://aka.ms/new-console-template for more information

Console.WriteLine("Semantic Kernel AI Agent (Gemini)");
Console.WriteLine("Type your question (or 'exit' to quit):");

// Load configuration from appsettings.json
var config = new ConfigurationBuilder().AddJsonFile("C:\\Users\\vishn\\RiderProjects\\AgenticSKK\\AgenticSKK\\appsettings.json").Build();
string serpApiKey = config["SerpApiKey"];
string geminiApiKey = config["GeminiApiKey"];

// Initialize the kernel
var builder = Kernel.CreateBuilder();
builder.AddGoogleAIGeminiChatCompletion("gemini-2.5-flash", geminiApiKey);
var kernel = builder.Build();

HttpClient httpClient = new HttpClient();

// Agent 1: Google Search Agent
async Task<string> GoogleSearchAsync(string query)
{
    var serpApiUrl = $"https://serpapi.com/search.json?q={Uri.EscapeDataString(query)}&api_key={serpApiKey}&num=5";
    var response = await httpClient.GetAsync(serpApiUrl);
    var json = await response.Content.ReadAsStringAsync();
    using var doc = JsonDocument.Parse(json);
    var snippets = doc.RootElement.GetProperty("organic_results").EnumerateArray()
        .Select(r => r.GetProperty("snippet").GetString())
        .Where(s => !string.IsNullOrEmpty(s)).ToList();
    return string.Join("\n", snippets);
}

// Agent 2: Summarizer Agent
async Task<string> SummarizeAsync(string query, string searchResults)
{
    var prompt = $"Summarize the following search results for '{query}':\n{searchResults}";
    var result = await kernel.InvokePromptAsync(prompt);
    return result.ToString();
}

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();
    if (input == null || input.ToLower() == "exit") break;

    // Use Agent 1 to get search results
    var searchResults = await GoogleSearchAsync(input);

    // Use Agent 2 to summarize
    var summary = await SummarizeAsync(input, searchResults);
    Console.WriteLine($"Agent: {summary}");
}
