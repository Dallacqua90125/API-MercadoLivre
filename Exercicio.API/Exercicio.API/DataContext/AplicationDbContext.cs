using Exercicio.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Exercicio.API.DataContext
{
    public class AplicationDbContext : DbContext
    {
        public AplicationDbContext(DbContextOptions<AplicationDbContext> options) : base(options)
        {
        }  

        public DbSet<ProductModel> products { get; set; }
    }
}
