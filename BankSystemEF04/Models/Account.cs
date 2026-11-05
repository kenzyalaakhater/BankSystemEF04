namespace BankSystemEF04.Models;
public class Account
{
    public string AccountNumber { get; set; } = null!;
    public AccountType AccountType { get; set; }
    public decimal CurrentBalance { get; set; }
    public DateTime OpeningDate { get; set; }

    public string BranchCode { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public ICollection<CustomerAccount> CustomerAccounts { get; set; } = new List<CustomerAccount>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}