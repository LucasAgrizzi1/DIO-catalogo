using Newtonsoft.Json;

public class Filme
{
    // O ID (id) é crucial para o Cosmos DB, ele deve ser uma string única
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("titulo")]
    public string Titulo { get; set; }

    [JsonProperty("anoLancamento")]
    public int AnoLancamento { get; set; }

    [JsonProperty("genero")]
    public string Genero { get; set; }
}
