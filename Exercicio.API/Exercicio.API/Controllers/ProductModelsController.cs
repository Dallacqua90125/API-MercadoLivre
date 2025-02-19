using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Exercicio.API.DataContext;
using Exercicio.API.Models;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Exercicio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductModelsController : ControllerBase
    {
        private readonly AplicationDbContext _context;

        public ProductModelsController(AplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ProductModels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductModel>>> Getproducts()
        {
            return await _context.products.ToListAsync();
        }

        // GET: api/ProductModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductModel>> GetProductModel(int id)
        {
            var productModel = await _context.products.FindAsync(id);

            if (productModel == null)
            {
                return NotFound();
            }

            return productModel;
        }

        // PUT: api/ProductModels/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductModel(int id, ProductModel productModel)
        {
            if (id != productModel.Id)
            {
                return BadRequest();
            }

            _context.Entry(productModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ProductModels
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProductModel>> PostProductModel(ProductModel productModel)
        {
            _context.products.Add(productModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProductModel", new { id = productModel.Id }, productModel);
        }

        // DELETE: api/ProductModels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductModel(int id)
        {
            var productModel = await _context.products.FindAsync(id);
            if (productModel == null)
            {
                return NotFound();
            }

            _context.products.Remove(productModel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductModelExists(int id)
        {
            return _context.products.Any(e => e.Id == id);
        }

        [HttpPost("run-scraper")]
        public async Task<IActionResult> RunScraper([FromBody] SraperRequestModel request)
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

                    // Agora chamamos ProcessScraperOutput sem precisar passar o contexto
                    var products = ProcessScraperOutput(output);

                    return Ok(new { message = "Scraping concluído com sucesso!", resultado = products });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Removemos o parâmetro dbContext porque já temos o _context
        private List<ProductModel> ProcessScraperOutput(string output)
        {
            // Extrair a parte relevante do JSON (a lista de produtos)
            var startIndex = output.IndexOf("[");
            var endIndex = output.LastIndexOf("]") + 1;
            var jsonArray = output.Substring(startIndex, endIndex - startIndex);

            // Desserializar o JSON para uma lista de objetos dinâmicos
            var productsData = JsonConvert.DeserializeObject<List<dynamic>>(jsonArray);

            // Lista para armazenar os produtos a serem adicionados
            var products = new List<ProductModel>();

            foreach (var productData in productsData)
            {
                var productName = (string)productData.nome;
                var existingProduct = _context.products.FirstOrDefault(p => p.Nome == productName);

                if (existingProduct == null) // Se não existir, adicionamos
                {
                    var product = new ProductModel
                    {
                        Nome = productName,
                        Price = productData.preco,
                        Assessment = productData.avaliacao,
                        Link = productData.link,
                        Product = productData.produto,
                        Image = productData.image
                    };

                    products.Add(product);
                    _context.products.Add(product); // Adiciona ao banco
                }
            }

            _context.SaveChanges(); // Salva as alterações no banco

            return products;
        }

        private async Task SaveProductsToDatabase(List<ProductModel> products)
        {
            foreach (var product in products)
            {
                _context.products.Add(product);
            }
            await _context.SaveChangesAsync();
        }
    }
}
