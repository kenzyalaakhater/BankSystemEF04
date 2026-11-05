namespace BankSystemEF04.Models;
public class CustomerAccount
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public Account Account { get; set; } = null!;
    public OwnershipType OwnershipType { get; set; }
    public DateTime OwnershipStartDate { get; set; }
    public AccountStatus AccountStatus { get; set; }
}