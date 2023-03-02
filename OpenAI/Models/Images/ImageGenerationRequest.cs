namespace OpenAI.Models.Images;

public record ImageGenerationRequest(string prompt, string size, int n, string response_format, string user);