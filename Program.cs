using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Telegram.Bot;

class Program
{
    static void Main(string[] args)
    {
        // Servidor web na porta 8080 (Obrigatório para o Render)
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args });
        builder.WebHost.UseUrls("http://0.0.0.0:8080");
        var app = builder.Build();
        app.MapGet("/", () => "Bot is running!");
        app.StartAsync();

        // Iniciar o Bot do Telegram
        string token = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN") ?? "";
        if (!string.IsNullOrEmpty(token))
        {
            Console.WriteLine("A iniciar bot...");
            // Substitui pela tua lógica de início de bot
            // Exemplo: var bot = new TelegramBotClient(token);
            Console.WriteLine("Bot online!");
        }

        Thread.Sleep(Timeout.Infinite);
    }
}