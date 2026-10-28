using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EnterpriseLinQ
{
    public class FormMain : Form
    {
        private readonly ReportService service = new ReportService();
        private readonly TabControl tabs = new TabControl();
        private DataGridView gridSales = new DataGridView();
        private DataGridView gridStats = new DataGridView();
        private DataGridView gridSets = new DataGridView();
        private DataGridView gridPaging = new DataGridView();
        private readonly ComboBox categoryBox = new ComboBox();
        private readonly ComboBox channelBox = new ComboBox();
        private readonly TextBox auditBox = new TextBox();
        private readonly NumericUpDown pageSize = new NumericUpDown();
        private readonly Label pageLabel = new Label();
        private int pageNumber = 1;

        public FormMain()
        {
            Text = "Enterprise — LINQ и SQLite";
            Width = 1150; Height = 720; StartPosition = FormStartPosition.CenterScreen;
            tabs.Dock = DockStyle.Fill;
            Controls.Add(tabs);
            BuildSalesTab();
            BuildStatsTab();
            BuildSetsTab();
            BuildPagingTab();
            RefreshAll();
        }

        private TabPage NewTab(string title)
        {
            var tab = new TabPage(title);
            tabs.TabPages.Add(tab);
            return tab;
        }

        private DataGridView MakeGrid()
        {
            var g = new DataGridView();
            g.Dock = DockStyle.Fill;
            g.ReadOnly = true;
            g.AutoGenerateColumns = true;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.AllowUserToAddRows = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            return g;
        }

        private FlowLayoutPanel Bar()
        {
            var p = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8), AutoSize = false };
            return p;
        }

        private void BuildSalesTab()
        {
            var tab = NewTab("Реестр продаж и DTO");
            var bar = Bar();
            categoryBox.DropDownStyle = ComboBoxStyle.DropDownList; categoryBox.Width = 180;
            channelBox.DropDownStyle = ComboBoxStyle.DropDownList; channelBox.Width = 130;
            categoryBox.Items.Add("(Все категории)");
            foreach (var c in service.Categories) categoryBox.Items.Add(c.Name);
            categoryBox.SelectedIndex = 0;
            channelBox.Items.AddRange(new object[] { "(Все каналы)", "Online", "Retail" });
            channelBox.SelectedIndex = 0;
            var apply = new Button { Text = "Применить фильтры", AutoSize = true };
            var reset = new Button { Text = "Сбросить", AutoSize = true };
            apply.Click += (s, e) => gridSales.DataSource = service.FilterReport(categoryBox.Text, channelBox.Text);
            reset.Click += (s, e) => { categoryBox.SelectedIndex = 0; channelBox.SelectedIndex = 0; gridSales.DataSource = service.FilterReport(null, null); };
            bar.Controls.Add(new Label { Text = "Категория:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
            bar.Controls.Add(categoryBox); bar.Controls.Add(new Label { Text = "Канал:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
            bar.Controls.Add(channelBox); bar.Controls.Add(apply); bar.Controls.Add(reset);
            gridSales = MakeGrid();
            tab.Controls.Add(gridSales); tab.Controls.Add(bar);
        }

        private void BuildStatsTab()
        {
            var tab = NewTab("Группировка и агрегация");
            var panel = new Panel { Dock = DockStyle.Bottom, Height = 70, Padding = new Padding(8) };
            auditBox.Dock = DockStyle.Fill; auditBox.Multiline = true; auditBox.ReadOnly = true;
            panel.Controls.Add(auditBox);
            gridStats = MakeGrid();
            tab.Controls.Add(gridStats); tab.Controls.Add(panel);
        }

        private void BuildSetsTab()
        {
            var tab = NewTab("Анализ каналов (множества)");
            var bar = Bar();
            var online = new Button { Text = "Online Only (Except)", AutoSize = true };
            var common = new Button { Text = "Общие товары (Intersect)", AutoSize = true };
            var all = new Button { Text = "Все уникальные (Union)", AutoSize = true };
            online.Click += (s, e) => gridSets.DataSource = service.OnlineOnly();
            common.Click += (s, e) => gridSets.DataSource = service.CommonProducts();
            all.Click += (s, e) => gridSets.DataSource = service.AllUniqueProducts();
            bar.Controls.Add(online); bar.Controls.Add(common); bar.Controls.Add(all);
            gridSets = MakeGrid();
            tab.Controls.Add(gridSets); tab.Controls.Add(bar);
        }

        private void BuildPagingTab()
        {
            var tab = NewTab("Постраничный просмотр (Paging)");
            var bar = Bar();
            var prev = new Button { Text = "Назад", AutoSize = true };
            var next = new Button { Text = "Вперед", AutoSize = true };
            pageSize.Minimum = 1; pageSize.Maximum = 50; pageSize.Value = 5; pageSize.Width = 60;
            prev.Click += (s, e) => { if (pageNumber > 1) pageNumber--; RefreshPage(); };
            next.Click += (s, e) => { if (pageNumber < PageCount()) pageNumber++; RefreshPage(); };
            pageSize.ValueChanged += (s, e) => { pageNumber = 1; RefreshPage(); };
            bar.Controls.Add(prev); bar.Controls.Add(next);
            bar.Controls.Add(new Label { Text = "Размер страницы:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
            bar.Controls.Add(pageSize); bar.Controls.Add(pageLabel);
            gridPaging = MakeGrid();
            tab.Controls.Add(gridPaging); tab.Controls.Add(bar);
        }

        private int PageCount()
        {
            return Math.Max(1, (int)Math.Ceiling(service.GetReport().Count() / (double)pageSize.Value));
        }

        private void RefreshPage()
        {
            pageNumber = Math.Min(pageNumber, PageCount());
            gridPaging.DataSource = service.GetPage(pageNumber, (int)pageSize.Value);
            pageLabel.Text = "Страница " + pageNumber + " из " + PageCount();
        }

        private void RefreshAll()
        {
            gridSales.DataSource = service.FilterReport(null, null);
            gridStats.DataSource = service.GetStats();
            auditBox.Text = service.GetAudit() + Environment.NewLine + service.DemonstrateDeferred();
            gridSets.DataSource = service.AllUniqueProducts();
            RefreshPage();
        }
    }
}
