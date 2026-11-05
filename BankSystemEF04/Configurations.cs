using BankSystemEF04.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BankSystemEF04.Configurations;
public class ManagerConfiguration : IEntityTypeConfiguration<Manager>
{
    public void Configure(EntityTypeBuilder<Manager> builder)
    {
        builder.ToTable("Managers");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.FullName).IsRequired().HasMaxLength(150);
        builder.Property(m => m.Email).IsRequired().HasMaxLength(200);
        builder.Property(m => m.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(m => m.HireDate).IsRequired();

        builder.HasData(GetSeed());   // SEED
    }
    private static Manager[] GetSeed() =>
    [
        new Manager { Id = 1, FullName = "Khaled Mansour", Email = "khaled.mansour@nationalbank.com",
                      PhoneNumber = "0100000001", HireDate = new DateTime(2015, 4, 1) },
        new Manager { Id = 2, FullName = "Rania Youssef", Email = "rania.youssef@nationalbank.com",
                      PhoneNumber = "0100000002", HireDate = new DateTime(2017, 9, 12) },
        new Manager { Id = 3, FullName = "Tamer Said", Email = "tamer.said@nationalbank.com",
                      PhoneNumber = "0100000003", HireDate = new DateTime(2019, 1, 20) }
    ];
}
public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");
        builder.HasKey(b => b.Code);
        builder.Property(b => b.Code).HasMaxLength(10);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(100);
        builder.Property(b => b.Address).IsRequired().HasMaxLength(300);
        builder.Property(b => b.PhoneNumber).IsRequired().HasMaxLength(20);
        // One-to-One: Branch <-> Manager (Manage)
        builder.HasOne(b => b.Manager)
               .WithOne(m => m.Branch)
               .HasForeignKey<Branch>(b => b.ManagerId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(GetSeed());   // SEED
    }
    private static Branch[] GetSeed() =>
    [
        new Branch { Code = "CAI-01", Name = "Cairo Main Branch", Address = "1 Tahrir Square, Cairo",
                     PhoneNumber = "0225000001", ManagerId = 1 },
        new Branch { Code = "ALX-01", Name = "Alexandria Branch", Address = "25 Corniche Road, Alexandria",
                     PhoneNumber = "0335000002", ManagerId = 2 },
        new Branch { Code = "GIZ-01", Name = "Giza Branch", Address = "10 Pyramids Street, Giza",
                     PhoneNumber = "0235000003", ManagerId = 3 }
    ];
}
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.FullName).IsRequired().HasMaxLength(150);
        builder.Property(c => c.NationalId).IsRequired().HasMaxLength(20);
        builder.HasIndex(c => c.NationalId).IsUnique();
        builder.Property(c => c.DateOfBirth).IsRequired();
        builder.Property(c => c.Email).IsRequired().HasMaxLength(200);
        builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Address).IsRequired().HasMaxLength(300);
        builder.Property(c => c.CustomerType).IsRequired().HasConversion<string>().HasMaxLength(20);
    }
}
public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(a => a.AccountNumber);
        builder.Property(a => a.AccountNumber).HasMaxLength(20);
        builder.Property(a => a.AccountType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.CurrentBalance).IsRequired().HasPrecision(18, 2);
        builder.Property(a => a.OpeningDate).IsRequired();
        builder.Property(a => a.BranchCode).IsRequired().HasMaxLength(10);
        // Branch 1 --- * Account (Belong)
        builder.HasOne(a => a.Branch)
               .WithMany(b => b.Accounts)
               .HasForeignKey(a => a.BranchCode)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);
    }
}
public class CustomerAccountConfiguration : IEntityTypeConfiguration<CustomerAccount>
{
    public void Configure(EntityTypeBuilder<CustomerAccount> builder)
    {
        builder.ToTable("CustomerAccounts");
        // Composite key: a customer is linked to an account only once
        builder.HasKey(ca => new { ca.CustomerId, ca.AccountNumber });
        builder.Property(ca => ca.AccountNumber).HasMaxLength(20);
        builder.Property(ca => ca.OwnershipType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(ca => ca.OwnershipStartDate).IsRequired();
        builder.Property(ca => ca.AccountStatus).IsRequired().HasConversion<string>().HasMaxLength(20);
        // Customer 1 --- * CustomerAccount
        builder.HasOne(ca => ca.Customer)
               .WithMany(c => c.CustomerAccounts)
               .HasForeignKey(ca => ca.CustomerId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
        // Account 1 --- * CustomerAccount
        builder.HasOne(ca => ca.Account)
               .WithMany(a => a.CustomerAccounts)
               .HasForeignKey(ca => ca.AccountNumber)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
    }
}
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(t => t.TransactionNumber);
        builder.Property(t => t.TransactionDate).IsRequired();
        builder.Property(t => t.Amount).IsRequired().HasPrecision(18, 2);
        builder.Property(t => t.TransactionType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Note).HasMaxLength(500);
        builder.Property(t => t.AccountNumber).IsRequired().HasMaxLength(20);
        // Account 1 --- * Transaction (Related): history is never deleted with the account
        builder.HasOne(t => t.Account)
               .WithMany(a => a.Transactions)
               .HasForeignKey(t => t.AccountNumber)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);
    }
}