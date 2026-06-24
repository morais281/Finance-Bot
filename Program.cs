using System;
using System.Threading;
using FinanceBot.Services;
using FinanceBot.Data;


// 1. Garante a base de dados
using (var db = new AppDbContext())

{
    // Tentativa de ligação com pausa para o Postgres acordar
    for (int i = 0; i < 5; i++)
    {
        try
        {
            db.Database.EnsureCreated();
            Console.WriteLine("Base de dados verificada!");
            break;
        }
        catch
        {
            Console.WriteLine("A aguardar ligação à base de dados...");
            Thread.Sleep(5000); // Espera 5 segundos
        }
    }
}

// 2. Inicia o Bot
string token = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN") ?? "TEU_TOKEN_AQUI";
FinanceBotHandler bot = new FinanceBotHandler(token);
bot.IniciarBot();

Console.WriteLine("Bot de Finanças online e à escuta...");

// 3. Mantém o bot vivo
Thread.Sleep(Timeout.Infinite);
