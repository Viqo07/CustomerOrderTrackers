using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;

public class TrackerContext : DbContext
{
	// DbSets for Customers and Orders
	public DbSet<Customer> Customers { get; set; }
	public DbSet<Order> Orders { get; set; }

	// Configure database (CustomerOrders.db)
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseSqlite("Data Source=CustomerOrders.db");
	}

	// Configure model rules
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// Ensure Customer has a unique Email
		modelBuilder.Entity<Customer>()
			.HasIndex(c => c.Email)
			.IsUnique();

		// One-to-Many: Customer -> Orders
		modelBuilder.Entity<Customer>()
			.HasMany(c => c.Orders)
			.WithOne(o => o.Customer)
			.HasForeignKey(o => o.CustomerId)
			.OnDelete(DeleteBehavior.Cascade);

		// Ensure Order.TotalAmount has precision (18,2)
		modelBuilder.Entity<Order>()
			.Property(o => o.TotalAmount)
			.HasPrecision(18, 2);
	}
}