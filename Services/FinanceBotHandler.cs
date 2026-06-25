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

        // 1. COMANDO START COM MENSAGEM ATUALIZADA
        if (texto.StartsWith("/start"))
        {
            var utilizador = db.Users.FirstOrDefault(u => u.Id == chatId);
            if (utilizador == null)
            {
                utilizador = new FinanceBot.Models.User { Id = chatId, Tier = "Gratuito" };
                db.Users.Add(utilizador);
                db.SaveChanges();
            }

            string mensagemBoasVindas = "Olá! 👋 Bem-vindo ao teu novo *Gestor Financeiro*.\n\n" +
                                        "Estou aqui para te ajudar a controlar o teu dinheiro de forma simples e rápida. Sem folhas de cálculo complicadas!\n\n" +
                                        "Aqui tens os comandos mágicos:\n" +
                                        "💰 */ganho [valor] [categoria]* - Regista o que entra\n" +
                                        "💸 */gasto [valor] [categoria]* - Regista o que sai\n" +
                                        "📊 */resumo* - Vê o teu saldo do mês\n" +
                                        "⏪ */resumo_anterior* - Vê o saldo do mês passado\n" +
                                        "📅 */resumo_ano* - Vê o saldo do ano inteiro\n\n" +
                                        "Os teus dados são privados e só tu os podes ver. Bora começar? 🚀";

            await client.SendTextMessageAsync(chatId, mensagemBoasVindas, parseMode: ParseMode.Markdown, cancellationToken: token);
        }

        // 2. COMANDOS DE GASTO E GANHO
        else if (texto.StartsWith("/gasto") || texto.StartsWith("/ganho"))
        {
            var utilizador = db.Users.FirstOrDefault(u => u.Id == chatId);
            if (utilizador == null) return;

            string tipoTransacao = texto.StartsWith("/gasto") ? "Despesa" : "Receita";

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
                Data = DateTime.UtcNow
            };

            db.Transactions.Add(novaTransacao);
            db.SaveChanges();

            string emoji = tipoTransacao == "Despesa" ? "💸" : "💰";
            await client.SendTextMessageAsync(chatId, $"{emoji} Registado: {valorRecebido}€ em '{partes[2]}'.", cancellationToken: token);
        }

        // 3. COMANDO RESUMO ANTERIOR
        else if (texto.StartsWith("/resumo_anterior"))
        {
            var inicioDoMesAtual = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var inicioMesAnterior = inicioDoMesAtual.AddMonths(-1);

            var transacoesMesAnterior = db.Transactions
                .Where(t => t.UserId == chatId && t.Data >= inicioMesAnterior && t.Data < inicioDoMesAtual)
                .ToList();

            var totalGanho = transacoesMesAnterior.Where(t => t.Tipo == "Receita").Sum(t => t.Valor);
            var totalGasto = transacoesMesAnterior.Where(t => t.Tipo == "Despesa").Sum(t => t.Valor);
            var saldo = totalGanho - totalGasto;

            var resumoDespesas = transacoesMesAnterior.Where(t => t.Tipo == "Despesa")
                .GroupBy(t => t.Categoria)
                .Select(g => $"• {g.Key}: {g.Sum(t => t.Valor)}€")
                .ToList();

            string despesasTexto = resumoDespesas.Any() ? string.Join("\n", resumoDespesas) : "• Sem despesas registadas no mês passado";

            string mensagemResumo = $"⏪ *RESUMO DO MÊS PASSADO*\n\n" +
                                    $"🟢 *Total Ganho:* {totalGanho}€\n" +
                                    $"🔴 *Total Gasto:* {totalGasto}€\n" +
                                    $"{(saldo >= 0 ? "✅" : "⚠️")} *SALDO DO MÊS:* {saldo}€\n\n" +
                                    $"📂 *Despesas por Categoria:*\n{despesasTexto}";

            await client.SendTextMessageAsync(chatId, mensagemResumo, parseMode: ParseMode.Markdown, cancellationToken: token);
        }

        // 4. COMANDO RESUMO ANO
        else if (texto.StartsWith("/resumo_ano"))
        {
            var inicioDoAno = new DateTime(DateTime.UtcNow.Year, 1, 1);

            var transacoesDoAno = db.Transactions
                .Where(t => t.UserId == chatId && t.Data >= inicioDoAno)
                .ToList();

            var totalGanho = transacoesDoAno.Where(t => t.Tipo == "Receita").Sum(t => t.Valor);
            var totalGasto = transacoesDoAno.Where(t => t.Tipo == "Despesa").Sum(t => t.Valor);
            var saldo = totalGanho - totalGasto;

            var resumoDespesas = transacoesDoAno.Where(t => t.Tipo == "Despesa")
                .GroupBy(t => t.Categoria)
                .Select(g => $"• {g.Key}: {g.Sum(t => t.Valor)}€")
                .ToList();

            string despesasTexto = resumoDespesas.Any() ? string.Join("\n", resumoDespesas) : "• Sem despesas registadas este ano";

            string mensagemResumo = $"📅 *RESUMO DO ANO ({DateTime.UtcNow.Year})*\n\n" +
                                    $"🟢 *Total Ganho:* {totalGanho}€\n" +
                                    $"🔴 *Total Gasto:* {totalGasto}€\n" +
                                    $"{(saldo >= 0 ? "✅" : "⚠️")} *SALDO ANUAL:* {saldo}€\n\n" +
                                    $"📂 *Despesas por Categoria (Anual):*\n{despesasTexto}";

            await client.SendTextMessageAsync(chatId, mensagemResumo, parseMode: ParseMode.Markdown, cancellationToken: token);
        }

        // 5. COMANDO RESUMO MÊS ATUAL (Tem de ficar no fim dos resumos!)
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