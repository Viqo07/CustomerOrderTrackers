using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Customer
{
	// Primary Key
	public int CustomerId { get; set; }

	// Name (required, max 120)
	[Required]
	[MaxLength(120)]
	public string Name { get; set; } = string.Empty;

	// Email (required, max 120)
	[Required]
	[MaxLength(120)]
	public string Email { get; set; } = string.Empty;

	// Navigation property (One-to-Many)
	public List<Order> Orders { get; set; } = new List<Order>();
}