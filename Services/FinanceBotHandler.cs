namespace FinanceBot.Services; 

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums; 
using FinanceBot.Data;   
using FinanceBot.Models; 

public class FinanceBotHandler
{
    private readonly TelegramBotClient _botClient;

    public FinanceBotHandler(string botToken)
    {
        _botClient = new TelegramBotClient(botToken);
    }

    public void IniciarBot()
    {
        _botClient.StartReceiving(ProcessarMensagemAsync, TratarErroAsync);
        Console.WriteLine("Bot de Finanças online e à escuta...");
    }

    private async Task ProcessarMensagemAsync(ITelegramBotClient client, Update update, CancellationToken token)
    {
        if (update.Message is not { Text: { } textoOriginal })
            return;

        var chatId = update.Message.Chat.Id;
        var texto = textoOriginal.ToLower();

        using var db = new AppDbContext();

        if (texto.StartsWith("/start"))
        {
            var utilizador = db.Users.FirstOrDefault(u => u.Id == chatId);
            if (utilizador == null)
            {
                // Agora toda a gente entra e usa à vontade sem limites
                utilizador = new FinanceBot.Models.User { Id = chatId, Tier = "Gratuito" };
                db.Users.Add(utilizador);
                db.SaveChanges();
            }
            await client.SendTextMessageAsync(chatId, "Bem-vindo! 💸\nUsa /ganho [valor] [categoria] para receitas.\nUsa /gasto [valor] [categoria] para despesas.\nUsa /resumo para veres o teu saldo.", cancellationToken: token);
        }
        else if (texto.StartsWith("/gasto") || texto.StartsWith("/ganho"))
        {
            var utilizador = db.Users.FirstOrDefault(u => u.Id == chatId);
            if (utilizador == null) return;

            string tipoTransacao = texto.StartsWith("/gasto") ? "Despesa" : "Receita";

            // Barreira Premium e contadores apagados! Código mais rápido e 100% grátis.

            string[] partes = textoOriginal.Split(' ', 3);
            if (partes.Length < 3)
            {
                await client.SendTextMessageAsync(chatId, $"⚠️ Formato incorreto. Exemplo: {partes[0]} 12.50 {(tipoTransacao == "Despesa" ? "Alimentação" : "Salário")}", cancellationToken: token);
                return;
            }

            string valorTexto = partes[1].Replace(',', '.');
            if (!decimal.TryParse(valorTexto, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal valorRecebido))
            {
                await client.SendTextMessageAsync(chatId, "⚠️ Valor inválido.", cancellationToken: token);
                return;
            }

            var novaTransacao = new FinanceBot.Models.Transaction
            {
                UserId = chatId,
                Valor = valorRecebido,
                Categoria = partes[2],
                Tipo = tipoTransacao,
                Data = DateTime.UtcNow // Mantive o UtcNow que configurámos antes!
            };

            db.Transactions.Add(novaTransacao);
            db.SaveChanges();

            string emoji = tipoTransacao == "Despesa" ? "💸" : "💰";
            await client.SendTextMessageAsync(chatId, $"{emoji} Registado: {valorRecebido}€ em '{partes[2]}'.", cancellationToken: token);
        }
        else if (texto.StartsWith("/resumo"))
        {
            var inicioDoMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var transacoesDoMes = db.Transactions.Where(t => t.UserId == chatId && t.Data >= inicioDoMes).ToList();

            var totalGanho = transacoesDoMes.Where(t => t.Tipo == "Receita").Sum(t => t.Valor);
            var totalGasto = transacoesDoMes.Where(t => t.Tipo == "Despesa").Sum(t => t.Valor);
            var saldo = totalGanho - totalGasto;

            var resumoDespesas = transacoesDoMes.Where(t => t.Tipo == "Despesa")
                .GroupBy(t => t.Categoria)
                .Select(g => $"• {g.Key}: {g.Sum(t => t.Valor)}€")
                .ToList();

            string despesasTexto = resumoDespesas.Any() ? string.Join("\n", resumoDespesas) : "• Sem despesas";

            string mensagemResumo = $"📊 *RESUMO DO MÊS*\n\n" +
                                    $"🟢 *Total Ganho:* {totalGanho}€\n" +
                                    $"🔴 *Total Gasto:* {totalGasto}€\n" +
                                    $"{(saldo >= 0 ? "✅" : "⚠️")} *SALDO DISPONÍVEL:* {saldo}€\n\n" +
                                    $"📂 *Despesas por Categoria:*\n{despesasTexto}";

            await client.SendTextMessageAsync(chatId, mensagemResumo, parseMode: ParseMode.Markdown, cancellationToken: token);
        }
    }

    private Task TratarErroAsync(ITelegramBotClient client, Exception exception, CancellationToken token)
    {
        Console.WriteLine($"Erro no Bot: {exception.Message}");
        return Task.CompletedTask;
    }
}