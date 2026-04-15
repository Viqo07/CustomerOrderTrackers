using System;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("=== Customer Order Tracker (EF Core) ===");

// Ensure DB is up-to-date with migrations
using (var ctx = new TrackerContext())
{
	try
	{
		ctx.Database.Migrate();
	}
	catch (Exception ex)
	{
		Console.WriteLine("Database migration failed.");
		Console.WriteLine(ex.Message);
		Console.WriteLine("Make sure you ran:");
		Console.WriteLine(" dotnet ef migrations add InitialCreate");
		Console.WriteLine(" dotnet ef database update");
		return;
	}
}

// UI loop
while (true)
{
	PrintMenu();
	Console.Write("Choose an option: ");
	var choice = (Console.ReadLine() ?? "").Trim();

	try
	{
		switch (choice)
		{
			case "1":
				await AddCustomerInteractive();
				break;
			case "2":
				await AddOrderInteractive();
				break;
			case "3":
				await ViewOrdersInteractive();
				break;
			case "4":
				await UpdateCustomerEmailInteractive();
				break;
			case "5":
				await DeleteCustomerInteractive();
				break;
			case "6":
				await DeleteOrderInteractive();
				break;
			case "7":
				await ListCustomersInteractive();
				break;
			case "0":
				Console.WriteLine("Goodbye!");
				return;
			default:
				Console.WriteLine("Invalid option. Try again.");
				break;
		}
	}
	catch (Exception ex)
	{
		Console.WriteLine($"Error: {ex.Message}");
	}

	Console.WriteLine();
}

// ---------------- MENU ----------------
static void PrintMenu()
{
	Console.WriteLine("""
Menu
1) Add Customer
2) Add Order (by Customer ID)
3) View Orders (with Customer names)
4) Update Customer Email
5) Delete Customer
6) Delete Order
7) List Customers (with order counts)
0) Exit
""");
}

// ---------------- FEATURES ----------------

static async Task AddCustomerInteractive()
{
	Console.Write("Customer name: ");
	var name = (Console.ReadLine() ?? "").Trim();

	Console.Write("Customer email: ");
	var email = (Console.ReadLine() ?? "").Trim();

	// Validation
	if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
	{
		Console.WriteLine("Name and Email are required.");
		return;
	}

	using var ctx = new TrackerContext();

	var customer = new Customer
	{
		Name = name,
		Email = email
	};

	ctx.Customers.Add(customer);
	await ctx.SaveChangesAsync();

	Console.WriteLine($"Customer added with ID: {customer.CustomerId}");
}

static async Task AddOrderInteractive()
{
	int customerId = ReadInt("Customer ID: ");
	double totalAmount = ReadDouble("Order Total Amount: ");

	if (totalAmount < 0)
	{
		Console.WriteLine("TotalAmount must be >= 0.");
		return;
	}

	using var ctx = new TrackerContext();

	// Prevent orphan order
	var customer = await ctx.Customers.FindAsync(customerId);
	if (customer == null)
	{
		Console.WriteLine("Customer not found.");
		return;
	}

	var order = new Order
	{
		CustomerId = customerId,
		TotalAmount = totalAmount
	};

	ctx.Orders.Add(order);
	await ctx.SaveChangesAsync();

	Console.WriteLine($"Order added with ID: {order.OrderId}");
}

static async Task ViewOrdersInteractive()
{
	using var ctx = new TrackerContext();

	var orders = await ctx.Orders
		.Include(o => o.Customer)
		.OrderBy(o => o.OrderDate)
		.ToListAsync();

	Console.WriteLine("\nOrders:");

	if (orders.Count == 0)
	{
		Console.WriteLine(" (none)");
		return;
	}

	foreach (var o in orders)
	{
		var customerName = o.Customer?.Name ?? "Unknown";

		Console.WriteLine(
			$"OrderID: {o.OrderId} | Date: {o.OrderDate} | Total: ${o.TotalAmount:0.00} | Customer: {customerName} (ID: {o.CustomerId})"
		);
	}
}

static async Task UpdateCustomerEmailInteractive()
{
	int customerId = ReadInt("Customer ID: ");

	Console.Write("New email: ");
	var newEmail = (Console.ReadLine() ?? "").Trim();

	if (string.IsNullOrWhiteSpace(newEmail))
	{
		Console.WriteLine("Email is required.");
		return;
	}

	using var ctx = new TrackerContext();

	var customer = await ctx.Customers.FindAsync(customerId);

	if (customer == null)
	{
		Console.WriteLine("Customer not found.");
		return;
	}

	customer.Email = newEmail;
	await ctx.SaveChangesAsync();

	Console.WriteLine("Customer email updated.");
}

static async Task DeleteCustomerInteractive()
{
	int customerId = ReadInt("Customer ID to delete: ");

	using var ctx = new TrackerContext();

	var customer = await ctx.Customers
		.Include(c => c.Orders)
		.FirstOrDefaultAsync(c => c.CustomerId == customerId);

	if (customer is null)
	{
		Console.WriteLine("Customer not found.");
		return;
	}

	Console.WriteLine($"Deleting customer: {customer.CustomerId} - {customer.Name} ({customer.Email})");

	if (customer.Orders.Count > 0)
		Console.WriteLine($"NOTE: This customer has {customer.Orders.Count} order(s).");

	Console.Write("Type YES to confirm: ");
	var confirm = Console.ReadLine();

	if (confirm?.ToLower() != "yes")
	{
		Console.WriteLine("Delete cancelled.");
		return;
	}

	ctx.Customers.Remove(customer);
	await ctx.SaveChangesAsync();

	Console.WriteLine("Customer deleted.");
}

static async Task DeleteOrderInteractive()
{
	int orderId = ReadInt("Order ID to delete: ");

	using var ctx = new TrackerContext();

	var order = await ctx.Orders.FindAsync(orderId);

	if (order == null)
	{
		Console.WriteLine("Order not found.");
		return;
	}

	ctx.Orders.Remove(order);
	await ctx.SaveChangesAsync();

	Console.WriteLine("Order deleted.");
}

// ---------------- GIVEN ----------------

static async Task ListCustomersInteractive()
{
	using var ctx = new TrackerContext();

	var customers = await ctx.Customers
		.Select(c => new
		{
			c.CustomerId,
			c.Name,
			c.Email,
			OrderCount = c.Orders.Count,
			TotalSpent = c.Orders.Sum(o => (double?)o.TotalAmount) ?? 0.0
		})
		.OrderBy(c => c.Name)
		.ToListAsync();

	Console.WriteLine("\nCustomers:");

	if (customers.Count == 0)
	{
		Console.WriteLine(" (none)");
		return;
	}

	foreach (var c in customers)
	{
		Console.WriteLine($" - {c.CustomerId}: {c.Name,-20} {c.Email,-25} | Orders: {c.OrderCount,2} Spent: ${c.TotalSpent:0.00}");
	}
}

// ---------------- HELPERS ----------------

static int ReadInt(string prompt)
{
	while (true)
	{
		Console.Write(prompt);
		var s = (Console.ReadLine() ?? "").Trim();

		if (int.TryParse(s, out int value))
			return value;

		Console.WriteLine("Please enter a valid whole number.");
	}
}

static double ReadDouble(string prompt)
{
	while (true)
	{
		Console.Write(prompt);
		var s = (Console.ReadLine() ?? "").Trim();

		if (double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out double value) ||
			double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
			return value;

		Console.WriteLine("Please enter a valid number (example: 249.99).");
	}
}