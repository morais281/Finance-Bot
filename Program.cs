using System;
using System.Threading;
using Npgsql;
using FinanceBot.Services;
using FinanceBot.Data; // <-- Importante para ler a Base de Dados

class Program
{
    static void Main(string[] args)
    {
        // Garante que o ficheiro da base de dados e as tabelas existem antes de ligar o bot
        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();
            Console.WriteLine(" Base de dados verificada e pronta a usar!");
        }

        // Substitui pelo teu token verdadeiro do BotFather
        // Vai procurar a chave oculta no Render. Se não encontrar (no teu PC), usa a versão manual.
string token = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN") ?? "8793064073:AAEzCfqIFvR61jXL99pXk_LWEiVIf5hcCKc";
        
        FinanceBotHandler bot = new FinanceBotHandler(token);
        bot.IniciarBot();

        Thread.Sleep(Timeout.Infinite);
    }
    string connString = Environment.GetEnvironmentVariable("DATABASE_URL");

using (var conn = new NpgsqlConnection(connString))
{
    conn.Open();
    // Aqui vais executar o CREATE TABLE que te dei antes
    using (var cmd = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS Objetivos (...)", conn))
    {
        cmd.ExecuteNonQuery();
    }
}
}