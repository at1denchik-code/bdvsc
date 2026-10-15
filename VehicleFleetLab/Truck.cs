using System;
using System.IO;
using System.Globalization;

namespace VehicleFleetLab
{
    public class Truck : Vehicle, IStorable
    {
        public double LoadCapacity { get; private set; }
        public string FilePath { get; private set; }

        public Truck(string model, int power, double loadCapacity) : base(model, power)
        {
            if (loadCapacity <= 0)
                throw new ArgumentException("Грузоподъёмность должна быть больше нуля.");
            LoadCapacity = loadCapacity;
            FilePath = string.Empty;
        }

        public override string GetInfo()
        {
            return string.Format("{0}, грузовой, грузоподъёмность: {1:0.##} т", base.GetInfo(), LoadCapacity);
        }

        public override double CalculateTax()
        {
            return Power * 25;
        }

        public void Save(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Не указан путь к файлу.");

            string content = "Тип: Грузовой автомобиль" + Environment.NewLine
                + "Модель: " + Model + Environment.NewLine
                + "Мощность: " + Power + " л.с." + Environment.NewLine
                + "Грузоподъёмность: " + LoadCapacity.ToString(CultureInfo.InvariantCulture) + " т" + Environment.NewLine
                + "Налог: " + CalculateTax().ToString("0.##", CultureInfo.InvariantCulture) + Environment.NewLine;

            File.WriteAllText(path, content);
            FilePath = path;
        }
    }
}
