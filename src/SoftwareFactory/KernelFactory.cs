using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace SoftwareFactory;

public static class KernelFactory
{
    public static (Kernel Kernel, string Brain) Create()
    {
        var builder = Kernel.CreateBuilder();
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            builder.Services.AddSingleton<IChatCompletionService, MockChatCompletionService>();
            return (builder.Build(), "mock");
        }

        var model = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-mini";
        var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT");

        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            builder.AddOpenAIChatCompletion(modelId: model, endpoint: new Uri(endpoint), apiKey: apiKey);
        }
        else
        {
            builder.AddOpenAIChatCompletion(modelId: model, apiKey: apiKey);
        }

        return (builder.Build(), "openai");
    }
}
