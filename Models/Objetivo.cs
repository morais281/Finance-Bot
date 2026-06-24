namespace FinanceBot.Models
{
    public class Objetivo
    {
        public int Id { get; set; } // Identificador único
        public long UserTelegramId { get; set; } // ID do utilizador no Telegram
        public string Nome { get; set; } // Nome do objetivo
        public decimal ValorAlvo { get; set; } // Quanto queres poupar
        public decimal ValorAtual { get; set; } // Quanto já poupaste
    }
}