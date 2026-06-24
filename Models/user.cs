namespace FinanceBot.Models;

public class User
{
    // O ID do utilizador vai ser o próprio ChatId do Telegram (garante que é único)
    public long Id { get; set; }
    
    // O plano do utilizador: "Gratuito" ou "Premium"
    public string Tier { get; set; } = "Gratuito";
    
    // Data em que o utilizador começou a usar o bot
    public DateTime DataRegisto { get; set; } = DateTime.UtcNow;
}