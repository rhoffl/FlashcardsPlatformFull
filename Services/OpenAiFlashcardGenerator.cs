
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FlashcardsPlatformFull.Models;

namespace FlashcardsPlatformFull.Services;

// NOTE: This is a stub that returns deterministic sample cards if no API key is configured.
// If you add your API key and implement parsing of the model response, you can generate real cards.
public class OpenAiFlashcardGenerator : IOpenAiFlashcardGenerator
{
    private readonly HttpClient _http;
    private readonly IConfiguration _cfg;

    public OpenAiFlashcardGenerator(HttpClient http, IConfiguration cfg)
    {
        _http = http;
        _cfg = cfg;
    }

    public async Task<List<Flashcard>> GenerateAsync(string topic, int count, int deckId)
    {
        var apiKey = _cfg["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            // Fallback: sample generated content
            var list = new List<Flashcard>();
            for (int i = 1; i <= Math.Max(1, count); i++)
            {
                list.Add(new Flashcard
                {
                    DeckId = deckId,
                    Question = $"{topic}: Sample question {i}?",
                    Answer = $"{topic}: Sample answer {i}.",
                    Category = "Generated",
                    Tags = "generated"
                });
            }
            return list;
        }

        // Real API call is intentionally minimal here; adapt to your OpenAI model choice.
        // You can also return JSON-only responses for easier parsing.
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var prompt = $"Generate {count} flashcards about: {topic}. Return ONLY valid JSON array, each object with question, answer, category, tags.";
        var body = new
        {
            model = "gpt-4.1-mini",
            messages = new[] { new { role = "user", content = prompt } },
            temperature = 0.3
        };

        var resp = await _http.PostAsync(
            "https://api.openai.com/v1/chat/completions",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        );
        resp.EnsureSuccessStatusCode();

        var raw = await resp.Content.ReadAsStringAsync();
        // TODO: parse choices[0].message.content into JSON array
        // For safety, we return fallback here.
        return new List<Flashcard>
        {
            new Flashcard { DeckId = deckId, Question = $"{topic}: Generated question?", Answer = "Implement parsing to use real output.", Category = "Generated", Tags = "generated" }
        };
    }
}
