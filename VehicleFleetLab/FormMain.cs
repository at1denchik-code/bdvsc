using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace VehicleFleetLab
{
    public class FormMain : Form
    {
        private readonly ComboBox cmbVehicleType = new ComboBox();
        private readonly TextBox txtModel = new TextBox();
        private readonly NumericUpDown numPower = new NumericUpDown();
        private readonly CheckBox chkIsElectric = new CheckBox();
        private readonly NumericUpDown numCapacity = new NumericUpDown();
        private readonly Button btnAdd = new Button();
        private readonly ListBox lstFleet = new ListBox();
        private readonly Button btnCalculateTax = new Button();
        private readonly Label lblTotalTax = new Label();
        private readonly Button btnSaveStorable = new Button();
        private readonly List<Vehicle> fleet = new List<Vehicle>();

        public FormMain()
        {
            Text = "Учёт автопарка";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(760, 510);
            MinimumSize = new Size(700, 480);
            Font = new Font("Segoe UI", 10);
            BuildInterface();
            cmbVehicleType.SelectedIndex = 0;
            UpdateSpecificControls();
        }

        private void BuildInterface()
        {
            Label lblType = new Label { Text = "Тип транспорта:", Left = 20, Top = 22, AutoSize = true };
            cmbVehicleType.SetBounds(160, 18, 210, 30);
            cmbVehicleType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbVehicleType.Items.AddRange(new object[] { "Легковой", "Грузовой" });
            cmbVehicleType.SelectedIndexChanged += delegate { UpdateSpecificControls(); };

            Label lblModel = new Label { Text = "Модель:", Left = 20, Top = 65, AutoSize = true };
            txtModel.SetBounds(160, 60, 210, 30);

            Label lblPower = new Label { Text = "Мощность (л.с.):", Left = 20, Top = 108, AutoSize = true };
            numPower.SetBounds(160, 103, 210, 30);
            numPower.Minimum = 1;
            numPower.Maximum = 2000;
            numPower.Value = 100;

            chkIsElectric.Text = "Электрический автомобиль";
            chkIsElectric.SetBounds(20, 150, 350, 30);

            Label lblCapacity = new Label { Text = "Грузоподъёмность (т):", Left = 20, Top = 190, AutoSize = true };
            numCapacity.SetBounds(190, 185, 180, 30);
            numCapacity.DecimalPlaces = 1;
            numCapacity.Increment = 0.5M;
            numCapacity.Minimum = 0.5M;
            numCapacity.Maximum = 200;
            numCapacity.Value = 5;

            btnAdd.Text = "Добавить транспорт";
            btnAdd.SetBounds(20, 235, 350, 38);
            btnAdd.Click += BtnAdd_Click;

            lstFleet.SetBounds(400, 20, 335, 315);
            lstFleet.HorizontalScrollbar = true;

            btnCalculateTax.Text = "Рассчитать общий налог";
            btnCalculateTax.SetBounds(20, 300, 350, 38);
            btnCalculateTax.Click += BtnCalculateTax_Click;

            lblTotalTax.Text = "Общий налог: 0";
            lblTotalTax.SetBounds(20, 350, 350, 30);
            lblTotalTax.Font = new Font(Font, FontStyle.Bold);

            btnSaveStorable.Text = "Сохранить грузовики в файлы";
            btnSaveStorable.SetBounds(20, 395, 350, 38);
            btnSaveStorable.Click += BtnSaveStorable_Click;

            Label note = new Label
            {
                Text = "Налог рассчитывается по ставкам из задания.",
                Left = 400, Top = 350, Width = 335, Height = 50
            };

            Controls.AddRange(new Control[] {
                lblType, cmbVehicleType, lblModel, txtModel, lblPower, numPower,
                chkIsElectric, lblCapacity, numCapacity, btnAdd, lstFleet,
                btnCalculateTax, lblTotalTax, btnSaveStorable, note
            });
        }

        private void UpdateSpecificControls()
        {
            bool isCar = cmbVehicleType.SelectedItem != null && cmbVehicleType.SelectedItem.ToString() == "Легковой";
            chkIsElectric.Enabled = isCar;
            numCapacity.Enabled = !isCar;
            if (!isCar) chkIsElectric.Checked = false;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                string model = txtModel.Text.Trim();
                int power = (int)numPower.Value;
                Vehicle vehicle;
                if (cmbVehicleType.SelectedItem.ToString() == "Легковой")
                    vehicle = new Car(model, power, chkIsElectric.Checked);
                else
                    vehicle = new Truck(model, power, (double)numCapacity.Value);

                fleet.Add(vehicle);
                lstFleet.Items.Add(vehicle.GetInfo());
                txtModel.Clear();
                txtModel.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnCalculateTax_Click(object sender, EventArgs e)
        {
            double total = 0;
            foreach (Vehicle vehicle in fleet)
                total += vehicle.CalculateTax();
            lblTotalTax.Text = string.Format("Общий налог: {0:0.##} руб.", total);
        }

        private void BtnSaveStorable_Click(object sender, EventArgs e)
        {
            List<Vehicle> storableVehicles = fleet.Where(v => v is IStorable).ToList();
            if (storableVehicles.Count == 0)
            {
                MessageBox.Show("В автопарке пока нет грузовиков для сохранения.");
                return;
            }

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Выберите папку для сохранения грузовиков";
                if (dialog.ShowDialog() != DialogResult.OK) return;

                int saved = 0;
                foreach (Vehicle vehicle in storableVehicles)
                {
                    if (vehicle is IStorable)
                    {
                        IStorable storable = (IStorable)vehicle;
                        string path = Path.Combine(dialog.SelectedPath, "Truck_" + (saved + 1) + ".txt");
                        storable.Save(path);
                        saved++;
                    }
                }

                MessageBox.Show("Сохранено файлов: " + saved, "Готово",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
