namespace backend_AI.Services.Abstractions
{
    /// <summary>
    /// Abstraction for AI chat/generation operations.
    /// </summary>
    public interface IAiChatService
    {
        Task<string> GenerateAsync(CancellationToken cancellationToken = default);
        Task<string> GenerateWithPromptAsync(string prompt, CancellationToken cancellationToken = default);
    }
}


