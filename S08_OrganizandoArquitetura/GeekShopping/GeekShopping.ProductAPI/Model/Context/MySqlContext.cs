using GeekShopping.ProductAPI.Model.Entity;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.ProductAPI.Model.Context
{
    public class MySqlContext : DbContext
    {
        public MySqlContext() { }

        public MySqlContext(DbContextOptions<MySqlContext> options) : base (options) { }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 2,
                Name = "Name",
                Price = new decimal(9.9),
                Description = "Descrição",
                ImageUrl = "http://imagemURL.com/Imagem2",
                CategoryName = "T-Shirt"
            });
        }
    }
}