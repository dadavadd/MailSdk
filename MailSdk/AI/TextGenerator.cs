using Mscc.GenerativeAI;

namespace MailSdk.AI;

public class TextGenerator
{
    private readonly GenerativeModel _model;

    public TextGenerator()
    {
        _model = new()
        {
            Model = Model.Gemini20FlashExperimental,
            ApiKey = "https://aistudio.google.com/apikey"
        };
    }

    public async Task<string> GenerateAsnwerAsync(string input, IEnumerable<Models.Image> images = null!)
    {
        var request = new GenerateContentRequest(prompt: "Промт: \n" + input)
        {
            SystemInstruction =

            new(string.Empty)
            {
                Role = "user",
                Parts =
                [
                    new TextData { Text = "Инструкции сюда" }
                ]
            },

            GenerationConfig = new GenerationConfig
            {
                Temperature = 0f,
                TopP = 0.95f,
                MaxOutputTokens = 8192
            }
        };

        if (images is not null)
            foreach (var image in images)
                await request.AddMedia(image.Url);

        var response = await _model.GenerateContent(request);
        return response.Text!;
    }
}
