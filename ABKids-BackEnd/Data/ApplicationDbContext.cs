using ABKids_BackEnd.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static ABKids_BackEnd.Models.User;
using Task = ABKids_BackEnd.Models.Task;

namespace ABKids_BackEnd.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Parent> Parents { get; set; }
        public DbSet<Child> Children { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<SavingsGoal> SavingsGoals { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TPH Inheritance for User
            modelBuilder.Entity<User>()
                .HasDiscriminator(u => u.Type)
                .HasValue<Parent>(UserType.Parent)
                .HasValue<Child>(UserType.Child);

            // Parent Relationships
            modelBuilder.Entity<Parent>()
                .HasOne(p => p.Account)
                .WithOne(a => a.ParentOwner)
                .HasForeignKey<Parent>(p => p.AccountId);

            modelBuilder.Entity<Parent>()
                .HasMany(p => p.Children)
                .WithOne(c => c.Parent)
                .HasForeignKey(c => c.ParentId);

            modelBuilder.Entity<Parent>()
                .HasMany(p => p.TasksCreated)
                .WithOne(t => t.Parent)
                .HasForeignKey(t => t.ParentId);

            // Child Relationships
            modelBuilder.Entity<Child>()
                .HasOne(c => c.Account)
                .WithOne(a => a.ChildOwner)
                .HasForeignKey<Child>(c => c.AccountId);

            modelBuilder.Entity<Child>()
                .HasMany(c => c.SavingsGoals)
                .WithOne(sg => sg.Child)
                .HasForeignKey(sg => sg.ChildId);

            modelBuilder.Entity<Child>()
                .HasMany(c => c.TasksAssigned)
                .WithOne(t => t.Child)
                .HasForeignKey(t => t.ChildId);

            modelBuilder.Entity<Child>()
                .HasMany(c => c.LoyaltyTransactions)
                .WithOne(lt => lt.Child)
                .HasForeignKey(lt => lt.ChildId);

            // Account Relationships
            modelBuilder.Entity<Account>()
                .HasOne(a => a.ParentOwner)
                .WithOne(p => p.Account)
                .HasForeignKey<Account>(a => a.OwnerId)
                .IsRequired(false);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.ChildOwner)
                .WithOne(c => c.Account)
                .HasForeignKey<Account>(a => a.OwnerId)
                .IsRequired(false);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.SavingsGoal)
                .WithOne(sg => sg.Account)
                .HasForeignKey<Account>(a => a.OwnerId)
                .IsRequired(false);

            modelBuilder.Entity<Account>()
                .HasMany(a => a.SentTransactions)
                .WithOne(t => t.SenderAccount)
                .HasForeignKey(t => t.SenderAccountId);

            modelBuilder.Entity<Account>()
                .HasMany(a => a.ReceivedTransactions)
                .WithOne(t => t.ReceiverAccount)
                .HasForeignKey(t => t.ReceiverAccountId);

            // SavingsGoal Relationships
            modelBuilder.Entity<SavingsGoal>()
                .HasOne(sg => sg.Account)
                .WithOne(a => a.SavingsGoal)
                .HasForeignKey<SavingsGoal>(sg => sg.AccountId);
            // Task Relationships
            modelBuilder.Entity<Task>()
                .HasOne(t => t.Parent)
                .WithMany(p => p.TasksCreated)
                .HasForeignKey(t => t.ParentId)
                .OnDelete(DeleteBehavior.Restrict); // Disable cascade for ParentId

            modelBuilder.Entity<Task>()
                .HasOne(t => t.Child)
                .WithMany(c => c.TasksAssigned)
                .HasForeignKey(t => t.ChildId)
                .OnDelete(DeleteBehavior.Restrict);// Disable cascade for ChildId
            // Transaction Relationships
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.SenderAccount)
                .WithMany(a => a.SentTransactions)
                .HasForeignKey(t => t.SenderAccountId)
                .OnDelete(DeleteBehavior.Restrict); // Disable cascade for SenderAccountId

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.ReceiverAccount)
                .WithMany(a => a.ReceivedTransactions)
                .HasForeignKey(t => t.ReceiverAccountId)
                .OnDelete(DeleteBehavior.Restrict); // Disable cascade for ReceiverAccountId
        
    }
    }
}