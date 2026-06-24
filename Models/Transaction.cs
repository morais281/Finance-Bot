namespace FinanceBot.Models;

using System;

public class Transaction
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public decimal Valor { get; set; }
    public string Categoria { get; set; } = "Geral";
    
    // NOVA COLUNA: Define se o dinheiro entra ou sai
    public string Tipo { get; set; } = "Despesa"; 
    
    public DateTime Data { get; set; } = DateTime.UtcNow;
}