namespace backend_AI.DTO.Ai.Response
{
    /// <summary>
    /// Simple response DTO used for text generation responses.
    /// </summary>
    public sealed record GenerateTextResponse(string Response)
    {
        public static GenerateTextResponse FromString(string? value)
            => new GenerateTextResponse(value ?? string.Empty);

        public override string ToString() => Response;
    }
}

 