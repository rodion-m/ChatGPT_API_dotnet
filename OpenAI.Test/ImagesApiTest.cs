using System.Text;
using FluentAssertions;
using OpenAI.Models.ChatCompletion;
using OpenAI.Models.Images;
using Xunit.Abstractions;

namespace OpenAI.Test;

[Collection("OpenAiTestCollection")]
public class ImagesApiTest
{
    private readonly OpenAiClient _client;

    public ImagesApiTest(ITestOutputHelper outputHelper)
    {
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
        Uri[] uris = await _client.GenerateImagesUris("bicycle", "test", OpenAiImageSize._256, count: 2);
        uris.Should().HaveCount(2);
    }
}