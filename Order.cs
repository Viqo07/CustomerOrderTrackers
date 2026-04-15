using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Order
{
	// Primary Key
	public int OrderId { get; set; }

	// Order Date (default = current UTC time)
	public DateTime OrderDate { get; set; } = DateTime.UtcNow;

	// Total Amount (range 0 to max double)
	[Range(0, double.MaxValue)]
	public double TotalAmount { get; set; }

	// Foreign Key
	public int CustomerId { get; set; }

	// Navigation Property
	[ForeignKey("CustomerId")]
	public Customer Customer { get; set; }
}