using Microsoft.EntityFrameworkCore;
using QMS.Domain.Models;

namespace QMS.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<EmployeeInfo> Employees { get; set; }
        public DbSet<CustomerInfo> Customers { get; set; }
        public DbSet<DisplayScreen> DisplayScreens { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<DisplayTicket> DisplayTickets { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(u =>
            {
                u.HasKey(u => u.Id);
                u.HasIndex(u => u.Username).IsUnique();
                u.HasIndex(u => u.PhoneNumber).IsUnique();
                u.HasIndex(u => u.Email).IsUnique();
                u.HasIndex(u => u.NationalNumber).IsUnique();
                u.Property(u => u.Role).HasConversion<string>();
            });

            modelBuilder.Entity<EmployeeInfo>(e =>
            {
                e.HasKey(e => e.UserId);

                // NOTE: this unique index means each Service can only be assigned to ONE employee.
                // If you later need multiple counters handling the same service, remove this index
                // and update CallNextTicketAsync to filter by employee's serviceId.
                // SQL Server allows multiple NULLs in a unique index, so DoorVerifiers (ServiceId = null) are fine.
                e.HasIndex(e => e.ServiceId).IsUnique();

                e.HasOne(e => e.User)
                 .WithOne(u => u.EmployeeInfo)
                 .HasForeignKey<EmployeeInfo>(e => e.UserId)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<CustomerInfo>(c =>
            {
                c.HasKey(c => c.UserId);
                c.HasOne(c => c.User)
                 .WithOne(u => u.CustomerInfo)
                 .HasForeignKey<CustomerInfo>(c => c.UserId)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Service>(s =>
            {
                s.HasKey(s => s.Id);
                s.HasIndex(s => s.Name).IsUnique();

                s.HasMany(s => s.Tickets)
                 .WithOne(t => t.Service)
                 .HasForeignKey(t => t.ServiceId)
                 .OnDelete(DeleteBehavior.NoAction);

                s.HasOne(s => s.Employee)
                 .WithOne(e => e.Service)
                 .HasForeignKey<EmployeeInfo>(e => e.ServiceId)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Branch>(b =>
            {
                b.HasKey(b => b.Id);
                b.HasIndex(b => b.Contact).IsUnique();
                b.HasIndex(b => b.Name).IsUnique();

                b.HasMany(b => b.Employees)
                 .WithOne(e => e.Branch)
                 .HasForeignKey(e => e.BranchId)
                 .OnDelete(DeleteBehavior.NoAction);

                // FIX: explicit 1-to-1 Branch <-> DisplayScreen relationship.
                // Without this EF may not discover the inverse navigation property (Branch.Display).
                b.HasOne(b => b.Display)
                 .WithOne(d => d.Branch)
                 .HasForeignKey<DisplayScreen>(d => d.BranchId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Ticket>(t =>
            {
                t.HasKey(t => t.Id);
                t.Property(t => t.Status).HasConversion<string>();
                t.HasIndex(t => new { t.TicketNumber, t.BranchId }).IsUnique();

                t.HasOne(t => t.Customer)
                 .WithMany(c => c.Tickets)
                 .HasForeignKey(t => t.CustomerId)
                 .OnDelete(DeleteBehavior.NoAction);

                t.HasOne(t => t.Service)
                 .WithMany(s => s.Tickets)
                 .HasForeignKey(t => t.ServiceId)
                 .OnDelete(DeleteBehavior.NoAction);

                t.HasOne(t => t.Branch)
                 .WithMany(b => b.Tickets)
                 .HasForeignKey(t => t.BranchId)
                 .OnDelete(DeleteBehavior.NoAction);

                t.HasOne(t => t.Employee)
                 .WithMany(e => e.HandeledTickets)
                 .HasForeignKey(t => t.EmployeeId)
                 .OnDelete(DeleteBehavior.NoAction);

                t.HasMany(t => t.DisplayTickets)
                 .WithOne(dt => dt.Ticket)
                 .HasForeignKey(dt => dt.TicketId)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<DisplayScreen>(d =>
            {
                d.HasKey(d => d.Id);
                d.HasMany(d => d.DisplayTickets)
                 .WithOne(dt => dt.Display)
                 .HasForeignKey(dt => dt.DisplayId)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<RefreshToken>(r =>
            {
                r.HasKey(r => r.Id);
                r.HasIndex(r => r.Token);
            });
        }
    }
}