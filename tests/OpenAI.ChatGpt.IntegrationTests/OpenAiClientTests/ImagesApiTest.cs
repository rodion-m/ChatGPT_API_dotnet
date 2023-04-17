using OpenAI.ChatGpt.Models.Images;

namespace OpenAI.ChatGpt.IntegrationTests.OpenAiClientTests;

[Collection("OpenAiTestCollection")] //to prevent parallel execution
public class ImagesApiTest
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly OpenAiClient _client;

    public ImagesApiTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _client = new OpenAiClient(Helpers.GetKeyFromEnvironment("OPEN_AI_API_KEY"));
    }

    [Fact(Skip = "Images API will be removed")]
    public async void Generate_image_bytes_works()
    {
        byte[] image = await _client.GenerateImageBytes("bicycle", "test", OpenAiImageSize._256);
        image.Should().NotBeEmpty();
    }
    
    [Fact(Skip = "Images API will be removed")]
    public async void Generate_two_images_uri_works()
    {
        Uri[] uris = await _client.GenerateImagesUris("bicycle", size: OpenAiImageSize._256, count: 2);
        uris.Should().HaveCount(2);
    }
}