using System;
using System.Threading;
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
}