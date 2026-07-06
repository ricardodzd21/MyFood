using System.Text;
using System.Text.Json;

namespace MyFood_api.Services;

// Analisa uma foto de comida/bebida e devolve os campos sugeridos para o catalogo.
// Usa o free tier do Google Gemini Flash. Se nao houver ApiKey configurada, IsEnabled = false.
public class GeminiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly string? _apiKey;
    private readonly string _model;

    public GeminiService(IHttpClientFactory httpFactory, IConfiguration config)
    {
        _httpFactory = httpFactory;
        _apiKey = config["Gemini:ApiKey"];
        _model = config["Gemini:Model"] ?? "gemini-2.0-flash";
    }

    public bool IsEnabled => !string.IsNullOrWhiteSpace(_apiKey);

    // imageBytes: bytes da foto. mimeType: ex "image/jpeg". categories: nomes das categorias existentes.
    // Retorna JSON (JsonElement) no formato esperado pelo frontend, ou lanca em caso de erro.
    public async Task<JsonElement> AnalyzeAsync(byte[] imageBytes, string mimeType, IEnumerable<string> categories)
    {
        if (!IsEnabled)
            throw new InvalidOperationException("IA não configurada (Gemini:ApiKey ausente).");

        var catList = string.Join(", ", categories);
        var prompt =
            "Você é um assistente que cataloga comidas e bebidas a partir de uma foto.\n" +
            "Analise a imagem (pode ser um rótulo de bebida, uma garrafa ou um prato de comida) e extraia as informações.\n" +
            $"As categorias existentes são: [{catList}]. Escolha a mais adequada em 'category'; se nenhuma servir, sugira um nome novo.\n" +
            "Para 'subcategory' sugira algo específico (ex: Tinto Suave, IPA, Massa, Sobremesa).\n" +
            "Em 'attributes' inclua pares nome/valor relevantes lidos ou inferidos da imagem: " +
            "para bebidas use Teor Alcoólico, Origem, Safra, Uva quando visíveis no rótulo; " +
            "para comidas use Ingredientes e Estabelecimento (só se aparecer na foto).\n" +
            "Escreva os valores em português. Não invente preço nem nota. Se não tiver certeza de um campo, omita-o dos attributes.\n" +
            "'description' deve ser uma frase curta descrevendo o item.";

        // Schema de saida estruturada
        var responseSchema = new
        {
            type = "object",
            properties = new
            {
                name = new { type = "string" },
                category = new { type = "string" },
                subcategory = new { type = "string" },
                description = new { type = "string" },
                attributes = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            name = new { type = "string" },
                            value = new { type = "string" }
                        },
                        required = new[] { "name", "value" }
                    }
                }
            },
            required = new[] { "name", "category" }
        };

        var body = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = prompt },
                        new { inline_data = new { mime_type = mimeType, data = Convert.ToBase64String(imageBytes) } }
                    }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                responseSchema
            }
        };

        var client = _httpFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(60);
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
        var json = JsonSerializer.Serialize(body);

        var resp = await client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
        var respText = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
            throw new Exception($"Gemini retornou {(int)resp.StatusCode}: {respText}");

        // Estrutura: candidates[0].content.parts[0].text -> string JSON
        using var doc = JsonDocument.Parse(respText);
        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "{}";

        // O text ja e um JSON (por causa do responseMimeType). Reparse e clona para retornar.
        using var parsed = JsonDocument.Parse(text);
        return parsed.RootElement.Clone();
    }
}
