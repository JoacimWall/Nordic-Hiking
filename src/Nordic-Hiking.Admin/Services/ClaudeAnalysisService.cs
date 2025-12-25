using System.Text.Json;
using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using NordicHiking.Core.Interfaces;

namespace NordicHiking.Admin.Services;

public class ClaudeAnalysisService : IAiAnalysisService
{
    private readonly AnthropicClient _client;

    public ClaudeAnalysisService(IConfiguration configuration)
    {
        var apiKey = configuration["Claude:ApiKey"]
            ?? throw new InvalidOperationException("Claude API key not configured");
        _client = new AnthropicClient(apiKey);
    }

    public async Task<HikeAnalysisResult> AnalyzeVideoAsync(string title, string description, string? transcript)
    {
        var prompt = $@"Analysera följande video om vandring:
Titel: {title}
Beskrivning: {description}
Transkript: {transcript ?? "Ej tillgängligt"}

Extrahera följande information som JSON:
- places: Lista med platser som nämns (namn, region, land). Första platsen försöker du hämta från titeln och beskrivningen. Om inga platser nämns, lämna listan tom.
- summary: Kort sammanfattning av vandringen (max 100 ord)
- difficulty: Uppskattad svårighetsgrad (lätt/medel/svår)
- duration: Om vandringens längd nämns (annars null)
- confidence: Hur säker du är på platserna (hög/medel/låg)

Svara ENDAST med JSON, ingen annan text.";

        var messages = new List<Message>
        {
            new Message(RoleType.User, prompt)
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            Model = "claude-sonnet-4-20250514",
            MaxTokens = 2000,
            Temperature = 0.3m
        };

        var response = await _client.Messages.GetClaudeMessageAsync(parameters);
        var jsonContent = response.Content.OfType<TextContent>().First().Text;

        // Ta bort markdown code blocks om de finns
        jsonContent = jsonContent.Trim();
        if (jsonContent.StartsWith("```json"))
        {
            jsonContent = jsonContent.Substring(7); // Ta bort ```json
        }
        else if (jsonContent.StartsWith("```"))
        {
            jsonContent = jsonContent.Substring(3); // Ta bort ```
        }

        if (jsonContent.EndsWith("```"))
        {
            jsonContent = jsonContent.Substring(0, jsonContent.Length - 3); // Ta bort ```
        }

        jsonContent = jsonContent.Trim();

        var result = JsonSerializer.Deserialize<HikeAnalysisResult>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result ?? new HikeAnalysisResult();
    }

    public async Task<string> SummarizeTranscriptAsync(string transcript)
    {
        var prompt = $@"Sammanfatta vad personerna pratar om i följande transkript från en vandringsvideo. 
Fokusera på:
- Vad de diskuterar under vandringen
- Intressanta observationer och kommentarer
- Tips och råd som nämns
- Stämningen och upplevelsen

Transkript:
{transcript}

Skriv en sammanfattning på svenska, max 300 ord. Var konkret och informativ. se till att dela upp de olika punkterna i separata stycken. med rubrik för varje stycken.";

        var messages = new List<Message>
        {
            new Message(RoleType.User, prompt)
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            Model = "claude-sonnet-4-20250514",
            MaxTokens = 1000,
            Temperature = 0.5m
        };

        var response = await _client.Messages.GetClaudeMessageAsync(parameters);
        return response.Content.OfType<TextContent>().First().Text.Trim();
    }
}
