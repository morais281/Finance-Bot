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
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        // 1. Manter a porta 8080 aberta para o Render não matar o bot
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args });
        builder.WebHost.UseUrls("http://0.0.0.0:8080");
        var app = builder.Build();
        app.MapMethods("/", new[] { "GET", "HEAD" }, () => "Bot de Finanças online e a bombar!");
        app.StartAsync();

        // 2. Ligar e preparar a Base de Dados (onde ele guarda o user)
        using (var db = new AppDbContext())
        {
            Console.WriteLine("A verificar a base de dados PostgreSQL...");
            db.Database.EnsureCreated();
            Console.WriteLine("Base de dados pronta e ligada!");
        }

        // 3. Arrancar o Bot do Telegram
        string token = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN");
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Erro: Variável TELEGRAM_TOKEN não encontrada!");
        }
        else
        {
            FinanceBotHandler bot = new FinanceBotHandler(token);
            bot.IniciarBot();
            Console.WriteLine("Bot online e à escuta no Telegram...");
        }

        Thread.Sleep(Timeout.Infinite);
    }
}