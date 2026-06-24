using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using FinanceBot.Services;
using FinanceBot.Data;

class Program
{
    static void Main(string[] args)
    {
        // 1. Configurar servidor web na porta 8080 para satisfazer o Render
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions {
            Args = args
        });
        
        builder.WebHost.UseUrls("http://0.0.0.0:8080");
        
        var app = builder.Build();
        app.MapGet("/", () => "Bot is alive!");
        app.StartAsync(); // Inicia o servidor em background

        // 2. Garantir a base de dados
        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();
            Console.WriteLine("Base de dados verificada!");
        }

        // 3. Iniciar o Bot
        string token = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN");
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Erro: TELEGRAM_TOKEN não encontrado!");
        }
        else
        {
            FinanceBotHandler bot = new FinanceBotHandler(token);
            bot.IniciarBot();
            Console.WriteLine("Bot de Finanças online e à escuta...");
        }

        // Manter a aplicação viva
        Thread.Sleep(Timeout.Infinite);
    }
}