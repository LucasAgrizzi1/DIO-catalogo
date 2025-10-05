using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public static class AdicionarFilmeFunction
{
    // [CosmosDB] é a Associação de Saída (Output Binding)
    [FunctionName("AdicionarFilme")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "filmes")] HttpRequest req,
        
        // Configura a associação de saída para o Cosmos DB
        // databaseName: Nome do seu banco de dados (ex: 'CatalogoDB')
        // collectionName: Nome do seu contêiner (ex: 'Filmes')
        // ConnectionStringSetting: Nome da chave de conexão definida nas configurações do App Service
        [CosmosDB(
            databaseName: "CatalogoDB",
            containerName: "Filmes",
            ConnectionSetting = "CosmosDBConnection")]
            IAsyncCollector<Filme> documentoOut,
        
        ILogger log)
    {
        log.LogInformation("Função 'AdicionarFilme' acionada por requisição HTTP POST.");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        Filme novoFilme = null;

        try
        {
            // Desserializa o JSON da requisição para o nosso objeto Filme
            novoFilme = JsonConvert.DeserializeObject<Filme>(requestBody);

            if (novoFilme == null || string.IsNullOrWhiteSpace(novoFilme.Titulo))
            {
                return new BadRequestObjectResult("Dados inválidos. O título do filme é obrigatório.");
            }
            
            // Atribui um ID único (GUID) se não for fornecido
            if (string.IsNullOrEmpty(novoFilme.Id))
            {
                novoFilme.Id = Guid.NewGuid().ToString();
            }

            // ----------------------------------------------------
            // AQUI OCORRE A MÁGICA DO BINDING
            // ----------------------------------------------------
            // Adiciona o objeto à coleção de saída. 
            // O Azure Functions Runtime se encarrega de enviá-lo ao Cosmos DB.
            await documentoOut.AddAsync(novoFilme);
            // ----------------------------------------------------

            log.LogInformation($"Filme '{novoFilme.Titulo}' adicionado com sucesso ao Cosmos DB.");
            
            return new OkObjectResult(novoFilme);
        }
        catch (Exception ex)
        {
            log.LogError($"Erro ao processar requisição: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
