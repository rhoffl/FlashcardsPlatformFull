
using FlashcardsPlatformFull.Models;

namespace FlashcardsPlatformFull.Services;

public interface IOpenAiFlashcardGenerator
{
    Task<List<Flashcard>> GenerateAsync(string topic, int count, int deckId);
}
