using System;
using System.Threading;
using FinanceBot.Services;
using FinanceBot.Data;

class Program
{
    static void Main(string[] args)
    {
        // 1. Garante a base de dados
        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();
            Console.WriteLine("Base de dados verificada e pronta a usar!");
        }

        // 2. Inicia o Bot
        string token = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN") ?? "TEU_TOKEN_AQUI";
        FinanceBotHandler bot = new FinanceBotHandler(token);
        bot.IniciarBot();

        Console.WriteLine("Bot de Finanças online e à escuta...");

        // 3. Mantém o bot vivo
        Thread.Sleep(Timeout.Infinite);
    }
}