namespace BankSystemEF04.Models;
public class Transaction
{
    public int TransactionNumber { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public TransactionType TransactionType { get; set; }
    public string? Note { get; set; }
    public string AccountNumber { get; set; } = null!;
    public Account Account { get; set; } = null!;
}