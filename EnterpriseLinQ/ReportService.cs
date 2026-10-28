using System;
using System.Collections.Generic;
using System.Linq;

namespace EnterpriseLinQ
{
    public class ReportService
    {
        private readonly List<Category> categories;
        private readonly List<Product> products;
        private readonly List<Sale> sales;

        public ReportService()
        {
            categories = DatabaseInitializer.LoadCategories();
            products = DatabaseInitializer.LoadProducts();
            sales = DatabaseInitializer.LoadSales();
        }

        public List<Category> Categories { get { return categories; } }
        public List<Product> Products { get { return products; } }
        public List<Sale> Sales { get { return sales; } }

        // Join объединяет продажи с товарами и категориями; Select создаёт DTO для таблицы отчёта.
        public IEnumerable<SaleReportDto> GetReport()
        {
            return from s in sales
                   join p in products on s.ProductId equals p.Id
                   join c in categories on p.CategoryId equals c.Id
                   select new SaleReportDto
                   {
                       CustomerName = s.CustomerName,
                       ProductName = p.Name,
                       CategoryName = c.Name,
                       DiscountedPrice = p.Price * (1 - c.DiscountRate),
                       Total = p.Price * (1 - c.DiscountRate) * s.Quantity,
                       Channel = s.Channel,
                       SaleDate = s.SaleDate
                   };
        }

        // GroupJoin возвращает категорию вместе с коллекцией относящихся к ней товаров.
        public object GetCategoryProducts()
        {
            return categories.GroupJoin(products, c => c.Id, p => p.CategoryId,
                (c, ps) => new { Категория = c.Name, Товары = string.Join(", ", ps.Select(p => p.Name)) }).ToList();
        }

        public List<SaleReportDto> FilterReport(string category, string channel)
        {
            IEnumerable<SaleReportDto> q = GetReport();
            if (!string.IsNullOrEmpty(category) && category != "(Все категории)")
                q = q.Where(x => x.CategoryName == category);
            if (!string.IsNullOrEmpty(channel) && channel != "(Все каналы)")
                q = q.Where(x => x.Channel == channel);
            return q.OrderBy(x => x.CategoryName).ThenByDescending(x => x.Total).ToList();
        }

        public List<SaleReportDto> GetPage(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            return GetReport().OrderBy(x => x.SaleDate).ThenBy(x => x.CustomerName)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        }

        public List<CategoryStatsDto> GetStats()
        {
            return GetReport().GroupBy(x => x.CategoryName)
                .Select(g => new CategoryStatsDto
                {
                    CategoryName = g.Key,
                    Deals = g.Count(),
                    Revenue = g.Sum(x => x.Total),
                    AverageCheck = g.Average(x => x.Total),
                    MinPurchase = g.Min(x => x.Total),
                    MaxPurchase = g.Max(x => x.Total)
                }).OrderBy(x => x.CategoryName).ToList();
        }

        public string GetAudit()
        {
            var list = GetReport().ToList();
            if (list.Count == 0) return "Продаж пока нет.";
            return list.Select(x => x.Total).Aggregate((a, b) => a + b)
                .ToString("N2") + " ₸ — общая выручка; сделок: " + list.Count +
                "; средний чек: " + list.Average(x => x.Total).ToString("N2") + " ₸.";
        }

        public List<Product> OnlineOnly()
        {
            var online = sales.Where(s => s.Channel == "Online")
                .Join(products, s => s.ProductId, p => p.Id, (s, p) => p).Distinct();
            var retail = sales.Where(s => s.Channel == "Retail")
                .Join(products, s => s.ProductId, p => p.Id, (s, p) => p).Distinct();
            return online.Except(retail).OrderBy(p => p.Name).ToList();
        }

        public List<Product> CommonProducts()
        {
            var online = sales.Where(s => s.Channel == "Online").Join(products, s => s.ProductId, p => p.Id, (s, p) => p).Distinct();
            var retail = sales.Where(s => s.Channel == "Retail").Join(products, s => s.ProductId, p => p.Id, (s, p) => p).Distinct();
            return online.Intersect(retail).OrderBy(p => p.Name).ToList();
        }

        public List<Product> AllUniqueProducts()
        {
            var online = sales.Where(s => s.Channel == "Online").Join(products, s => s.ProductId, p => p.Id, (s, p) => p);
            var retail = sales.Where(s => s.Channel == "Retail").Join(products, s => s.ProductId, p => p.Id, (s, p) => p);
            return online.Union(retail).OrderBy(p => p.Name).ToList();
        }

        // Демонстрация: deferred повторно выполняется над тем же списком; ToList фиксирует снимок.
        public string DemonstrateDeferred()
        {
            var deferred = GetReport();
            var immediate = GetReport().ToList();
            int beforeDeferred = deferred.Count();
            int beforeImmediate = immediate.Count;
            var original = sales.Count;
            var last = sales[sales.Count - 1];
            sales.Add(new Sale(999, last.ProductId, "Тестовый клиент", 1, "2026-10-28", "Online"));
            int afterDeferred = deferred.Count();
            int afterImmediate = immediate.Count;
            sales.RemoveAt(sales.Count - 1);
            return "До добавления: IEnumerable=" + beforeDeferred + ", ToList=" + beforeImmediate +
                ". После добавления в список: IEnumerable=" + afterDeferred + ", ToList=" + afterImmediate +
                ". Важно: добавление только в список C#, не в SQLite.";
        }
    }
}
