using System;

namespace EnterpriseLinQ
{
    public class Category
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public double DiscountRate { get; private set; }

        public Category(int id, string name, double discountRate)
        {
            Id = id; Name = name; DiscountRate = discountRate;
        }
    }

    public class Product : IEquatable<Product>
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public int CategoryId { get; private set; }
        public double Price { get; private set; }
        public int Stock { get; private set; }

        public Product(int id, string name, int categoryId, double price, int stock)
        {
            Id = id; Name = name; CategoryId = categoryId; Price = price; Stock = stock;
        }

        // Для Except/Intersect/Union два товара с одинаковым Id считаются одним товаром.
        public override bool Equals(object obj) { return Equals(obj as Product); }
        public bool Equals(Product other) { return other != null && Id == other.Id; }
        public override int GetHashCode() { return Id.GetHashCode(); }
    }

    public class Sale
    {
        public int Id { get; private set; }
        public int ProductId { get; private set; }
        public string CustomerName { get; private set; }
        public int Quantity { get; private set; }
        public string SaleDate { get; private set; }
        public string Channel { get; private set; }

        public Sale(int id, int productId, string customerName, int quantity, string saleDate, string channel)
        {
            Id = id; ProductId = productId; CustomerName = customerName;
            Quantity = quantity; SaleDate = saleDate; Channel = channel;
        }
    }

    public class SaleReportDto
    {
        public string CustomerName { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public double DiscountedPrice { get; set; }
        public double Total { get; set; }
        public string Channel { get; set; }
        public string SaleDate { get; set; }
    }

    public class CategoryStatsDto
    {
        public string CategoryName { get; set; }
        public int Deals { get; set; }
        public double Revenue { get; set; }
        public double AverageCheck { get; set; }
        public double MinPurchase { get; set; }
        public double MaxPurchase { get; set; }
    }
}
