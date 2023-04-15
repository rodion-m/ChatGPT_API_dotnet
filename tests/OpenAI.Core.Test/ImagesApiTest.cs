using FluentAssertions;
using OpenAI.Models.Images;
using Xunit.Abstractions;

namespace OpenAI.Core.Test;

[Collection("OpenAiTestCollection")] //to prevent parallel execution
public class ImagesApiTest
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly OpenAiClient _client;

    public ImagesApiTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _client = new OpenAiClient(Helpers.GetKeyFromEnvironment("openai_api_key_paid"));
    }

    [Fact]
    public async void Generate_image_bytes_works()
    {
        byte[] image = await _client.GenerateImageBytes("bicycle", "test", OpenAiImageSize._256);
        image.Should().NotBeEmpty();
    }
    
    [Fact]
    public async void Generate_two_images_uri_works()
    {
        Uri[] uris = await _client.GenerateImagesUris("bicycle", size: OpenAiImageSize._256, count: 2);
        uris.Should().HaveCount(2);
    }
}