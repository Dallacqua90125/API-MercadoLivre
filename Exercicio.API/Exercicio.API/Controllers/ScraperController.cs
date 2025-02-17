using System.Diagnostics;
using System.Text.Json;
using Exercicio.API.Models;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class ScraperController : ControllerBase
{
    [HttpPost("run-scraper")]
    public IActionResult RunScraper([FromBody] SraperRequestModel request)
    {
        if (string.IsNullOrEmpty(request.Produto))
            return BadRequest(new { error = "O nome do produto é obrigatório." });

        try
        {
            ProcessStartInfo puppeteerInfo = new ProcessStartInfo
            {
                FileName = "node",
                Arguments = $"\"C:\\Users\\Timeware 027\\Timeware\\Puppeteer\\Exercicio\\webScrapping-MercadoLivre\\Mercado Livre\\src\\index.js\" \"{request.Produto}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process puppeteerProcess = Process.Start(puppeteerInfo))
            {
                string output = puppeteerProcess.StandardOutput.ReadToEnd();
                string error = puppeteerProcess.StandardError.ReadToEnd();
                puppeteerProcess.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                    return StatusCode(500, new { error = "Erro no Puppeteer", detalhes = error });

                return Ok(new { message = "Scraping concluído com sucesso!", resultado = output });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

}
