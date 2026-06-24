namespace FinanceBot.Data;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using FinanceBot.Models;

public class AppDbContext : DbContext
{
    // Estas são as tuas tabelas na base de dados
    public DbSet<User> Users { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Objetivo> Objetivos { get; set; }

    // Este método configura a base de dados SQLite e cria o ficheiro "financas.db"
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    // Lê a variável e protege contra nulos
    string url = Environment.GetEnvironmentVariable("DATABASE_URL") 
                 ?? "postgres://user:pass@localhost/db"; 

    var uri = new Uri(url);
    
    // Extração segura de componentes
    var host = uri.Host;
    var port = uri.Port;
    var database = uri.AbsolutePath.TrimStart('/');
    var userInfo = uri.UserInfo.Split(':');
    var username = userInfo.Length > 0 ? userInfo[0] : "";
    var password = userInfo.Length > 1 ? userInfo[1] : "";

    var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";

    optionsBuilder.UseNpgsql(connectionString);
}
}