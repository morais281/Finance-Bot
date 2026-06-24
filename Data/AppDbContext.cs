namespace FinanceBot.Data;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using FinanceBot.Models;

public class AppDbContext : DbContext
{
    // Estas são as tuas tabelas na base de dados
    public DbSet<User> Users { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    

    // Este método configura a base de dados SQLite e cria o ficheiro "financas.db"
// Este método configura a base de dados PostgreSQL
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Adicionamos um '?' a dizer que envUrl pode vir vazia
        string? envUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

        if (!string.IsNullOrEmpty(envUrl))
        {
            // Lógica para quando o bot estiver no Render (ou se a variável existir)
            // Adicionamos um '?' no Uri
            bool isUri = Uri.TryCreate(envUrl, UriKind.Absolute, out Uri? dbUri);
            
            // Garantimos ao C# que o dbUri não é nulo antes de avançar
            if (isUri && dbUri != null)
            {
                var userInfo = dbUri.UserInfo.Split(':');
                // O truque para evitar o erro do "-1": se não houver porta no link, força a 5432
                var port = dbUri.Port > 0 ? dbUri.Port : 5432; 
                
                var connString = $"Host={dbUri.Host};Port={port};Database={dbUri.LocalPath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SslMode=Require;TrustServerCertificate=True;";
                optionsBuilder.UseNpgsql(connString);
            } 
            else 
            {
                // Se já for uma connection string normal
                optionsBuilder.UseNpgsql(envUrl); 
            }
        }
        else
        {
            // Lógica para rodar no teu PC local (Substitui "TUA_PASSWORD" pela pass do teu Postgres local)
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=FinanceBotDb;Username=postgres;Password=TUA_PASSWORD");
        }
    }
}