namespace FinanceBot.Data;

using Microsoft.EntityFrameworkCore;
using FinanceBot.Models;

public class AppDbContext : DbContext
{
    // Estas são as tuas tabelas na base de dados
    public DbSet<User> Users { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    // Este método configura a base de dados SQLite e cria o ficheiro "financas.db"
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=financas.db");
    }
}