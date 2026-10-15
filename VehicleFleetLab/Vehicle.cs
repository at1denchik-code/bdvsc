using System;

namespace VehicleFleetLab
{
    // Абстрактный базовый класс транспортного средства.
    public abstract class Vehicle
    {
        public string Model { get; private set; }
        public int Power { get; private set; }

        protected Vehicle(string model, int power)
        {
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Введите модель транспортного средства.");
            if (power <= 0)
                throw new ArgumentException("Мощность должна быть больше нуля.");

            Model = model;
            Power = power;
        }

        public virtual string GetInfo()
        {
            return string.Format("{0}, мощность: {1} л.с.", Model, Power);
        }

        public abstract double CalculateTax();
    }
}
