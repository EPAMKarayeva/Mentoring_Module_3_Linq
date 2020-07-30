// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;

// Version Mad01

namespace SampleQueries
{
	[Title("LINQ Module")]
	[Prefix("Linq")]
	public class LinqSamples : SampleHarness
	{

		private DataSource dataSource = new DataSource();
		IEnumerable<Customer> listCustomers = new DataSource().Customers.Where(c => c.Orders.Any());

		[Category("Restriction Operators")]
		[Title("Where - Task 1")]
		[Description("This sample uses the where clause to find all elements of an array with a value less than 5.")]
		public void Linq1()
		{
			int[] numbers = { 5, 4, 1, 3, 9, 8, 6, 7, 2, 0 };

			var lowNums =
				from num in numbers
				where num < 5
				select num;

			Console.WriteLine("Numbers < 5:");
			foreach (var x in lowNums)
			{
				Console.WriteLine(x);
			}
		}

		[Category("Restriction Operators")]
		[Title("Where - Task 2")]
		[Description("This sample return return all presented in market products")]

		public void Linq2()
		{
			var products =
				from p in dataSource.Products
				where p.UnitsInStock > 0
				select p;

			foreach (var p in products)
			{
				ObjectDumper.Write(p);
			}
		}

		[Category("Task")]
		[Title("Task 1")]
		[Description("Max sum of total cost of orders")]
		public void Linq001()
		{
			decimal x = 1500;
			var customersList = dataSource.Customers
					.Where(c => c.Orders.Sum(o => o.Total) > x)
					.Select(c => new
					{
						CustomerId = c.CustomerID,
						TotalSum = c.Orders.Sum(o => o.Total)
					});

			ObjectDumper.Write($"Greater than {x}");

			foreach (var customer in customersList)
			{
				ObjectDumper.Write($"CustomerId = {customer.CustomerId} TotalSum = {customer.TotalSum}\n");
			}

		}

		[Category("Task")]
		[Title("Task 2")]
		[Description("All suppliers which are leaving in the same country and city as customer.")]
		public void Linq002()
		{
			var customersWithSuppliers = dataSource.Customers
								.Select(c => new
								{
									Customer = c,
									Suppliers = dataSource.Suppliers.Where(s => s.City == c.City && s.Country == c.Country)
								});

			foreach (var customer in customersWithSuppliers)
			{
				ObjectDumper.Write($"CustomerId: {customer.Customer.CustomerID} " +
						$"List of suppliers: {string.Join(", ", customer.Suppliers.Select(s => s.SupplierName))}");
			}

			
		}

		[Category("Task")]
		[Title("Task 3")]
		[Description("Displays all customers who has order with total greater than X.")]
		public void Linq003()
		{
			decimal amount = 1500;
			var customers = dataSource.Customers
				.Where(x => x.Orders.Sum(t => t.Total) > amount);

			ObjectDumper.Write($"Sum Greater than {amount}");

			foreach (var customer in customers)
			{
				ObjectDumper.Write(customer);
			}

		}

		[Category("Task")]
		[Title("Task 4")]
		[Description("Displays first order's date")]
		public void Linq004()
		{
			var customers = dataSource.Customers
			.Select(c => new
				{
					CustomerId = c.CustomerID,
					StartDate = c.Orders.Select(o => o.OrderDate).OrderBy(o => o.Date).FirstOrDefault()
				});

			foreach (var c in customers)
			{
				ObjectDumper.Write($"CustomerId = {c.CustomerId}; " +
						$"Month = {c.StartDate.Month}; Year = {c.StartDate.Year}");
			}
		}

		[Category("Task")]
		[Title("Task 5")]
		[Description("Ordering list of customers")]
		public void Linq005()
		{
			var customers = dataSource.Customers.Where(c => c.Orders.Any())
				.Select(c => new
				{
					CustomerId = c.CustomerID,
					StartDate = c.Orders.Select(o => o.OrderDate).OrderBy(o => o.Date).FirstOrDefault(),
					TotalSum = c.Orders.Sum(o => o.Total)
				}).OrderBy(c => c.StartDate.Year)
				.ThenBy(c => c.CustomerId)
				.ThenBy(c => c.StartDate.Month)				
				.ThenByDescending(c => c.TotalSum);
				

			foreach (var c in customers)
			{
				ObjectDumper.Write($"CustomerId = {c.CustomerId} TotalSum: {c.TotalSum} " +
						$"Month = {c.StartDate.Month} Year = {c.StartDate.Year}");
			}
		}

		[Category("Task")]
		[Title("Task 6")]
		[Description("Displays all customers with not number postal code or without region or whithout operator's code")]
		public void Linq006()
		{
			var customers = dataSource.Customers.Where(
								c => c.PostalCode != null && c.PostalCode.Any(sym => sym < '0' || sym > '9')
										|| string.IsNullOrWhiteSpace(c.Region)
										|| c.Phone.FirstOrDefault() != '(');

			foreach (var customer in customers)
			{
				ObjectDumper.Write(customer);
			}

		}

		[Category("Task")]
		[Title("Task 7")]
		[Description("Groups for products")]
		public void Linq007()
		{
			var products = dataSource.Products
				.GroupBy(p => p.Category)
				.Select(s => new
				{
					Category = s.Key,
					ProductInStock = s.GroupBy(p=>p.UnitsInStock != 0)
					.Select(c=> new 
					{
					  HasInStock = c.Key,
						Cost = c.OrderBy(cost=> cost.UnitPrice)
					})
				});

			foreach (var productsByCategory in products)
			{
				ObjectDumper.Write($"Category: {productsByCategory.Category}\n");
				foreach (var productsByStock in productsByCategory.ProductInStock)
				{
					ObjectDumper.Write($"\tHas in stock: {productsByStock.HasInStock}");
					foreach (var product in productsByStock.Cost)
					{
						ObjectDumper.Write($"\t\tProduct: {product.ProductName} Price: {product.UnitPrice}");
					}
				}
			}
		}

		[Category("Task")]
		[Title("Task 8")]
		[Description("Groups products by price: cheap, average and expensive.")]
		public void Linq008()
		{
			decimal cheap = 10;
			decimal expensive = 50;

			var groups = dataSource.Products
				.GroupBy(p => p.UnitPrice < cheap ? "Cheap"
										: p.UnitPrice < expensive? "Average price" : "Expensive");

			foreach (var group in groups)
			{
				ObjectDumper.Write($"{group.Key}:");
				foreach (var product in group)
				{
					ObjectDumper.Write($"\tProduct: {product.ProductName} Price: {product.UnitPrice}\n");
				}
			}
		}

		[Category("Task")]
		[Title("Task 9")]
		[Description("Average sum of orders from one city and average intensity of orders in each city.")]
		public void Linq009()
		{
			var results = dataSource.Customers
				.GroupBy(c => c.City)
				.Select(c => new
				{ 
					City = c.Key,
					AverageSum = c.Average(p => p.Orders.Sum(s => s.Total)),
					Intensity = c.Average(p=> p.Orders.Length)
				});

			foreach (var group in results)
			{
				ObjectDumper.Write($"City: {group.City}");
				ObjectDumper.Write($"\tAverage Income: {group.AverageSum}");
				ObjectDumper.Write($"\tIntensity: {group.Intensity}");
				
			}
		}

		[Category("Task")]
		[Title("Task 10")]
		[Description("Counts average order sum for and average client's intensity for every city")]
		public void Linq010()
		{
			var resultList = dataSource.Customers
				.Select(c => new
				{
					c.CustomerID,
					MonthAcitivity = c.Orders.GroupBy(o => o.OrderDate.Month)
																				.Select(g => new { Month = g.Key, OrdersCount = g.Count() }),
					YearActivity = c.Orders.GroupBy(o => o.OrderDate.Year)
																				.Select(g => new { Year = g.Key, OrdersCount = g.Count() }),
					MonthYearActivity = c.Orders.GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
																				.Select(g => new { g.Key.Year, g.Key.Month, OrdersCount = g.Count() }),
				});

			foreach (var record in resultList)
			{
				ObjectDumper.Write($"CustomerId: {record.CustomerID}");
				ObjectDumper.Write("\tMonths statistic:\n");
				foreach (var ms in record.MonthAcitivity)
				{
					ObjectDumper.Write($"\t\tMonth: {ms.Month}; Orders count: {ms.OrdersCount}");
				}
				ObjectDumper.Write("\tYears statistic:\n");
				foreach (var ys in record.YearActivity)
				{
					ObjectDumper.Write($"\t\tYear: {ys.Year}; Orders count: {ys.OrdersCount}");
				}
				ObjectDumper.Write("\tYear and month statistic:\n");                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         
				foreach (var ym in record.MonthYearActivity)
				{
					ObjectDumper.Write($"\t\tYear: {ym.Year}; Month: {ym.Month}; Orders count: {ym.OrdersCount}");
				}
			}
		}
	}
}
