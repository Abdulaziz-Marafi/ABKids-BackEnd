using ABKids_BackEnd.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static ABKids_BackEnd.Models.Account;
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
        public DbSet<Reward> Rewards { get; set; }

        public static void Seed(ApplicationDbContext context)
        {
            // Seed RewardSystem account
            if (!context.Accounts.Any(a => a.OwnerType == AccountOwnerType.RewardSystem))
            {
                context.Accounts.Add(new Account
                {
                    OwnerId = 0, // Unique, not tied to a user
                    OwnerType = AccountOwnerType.RewardSystem,
                    Balance = 0m // No funds needed
                });
            }
            context.SaveChanges();
        }

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
                .HasForeignKey<Parent>(p => p.ParentAccountId);

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
                .HasForeignKey<Child>(c => c.ChildAccountId);

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
            // Remove incorrect one-to-one relationships using OwnerId
            // Instead, define relationships based on OwnerId and OwnerType without foreign key constraints
            modelBuilder.Entity<Account>()
                .Property(a => a.OwnerId)
                .IsRequired();

            modelBuilder.Entity<Account>()
                .Property(a => a.OwnerType)
                .IsRequired();

            // Add composite unique index on (OwnerId, OwnerType)
            modelBuilder.Entity<Account>()
                .HasIndex(a => new { a.OwnerId, a.OwnerType })
                .IsUnique();

            // Transactions relationships (Account side)
            modelBuilder.Entity<Account>()
                .HasMany(a => a.SentTransactions)
                .WithOne(t => t.SenderAccount)
                .HasForeignKey(t => t.SenderAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Account>()
                .HasMany(a => a.ReceivedTransactions)
                .WithOne(t => t.ReceiverAccount)
                .HasForeignKey(t => t.ReceiverAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // SavingsGoal Relationships
            modelBuilder.Entity<SavingsGoal>()
                .HasOne(sg => sg.Account)
                .WithOne() // No inverse navigation property needed
                .HasForeignKey<SavingsGoal>(sg => sg.SavingsGoalAccountId)
                .IsRequired(false); // Keep nullable

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