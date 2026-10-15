namespace VehicleFleetLab
{
    public class Car : Vehicle
    {
        public bool IsElectric { get; private set; }

        public Car(string model, int power, bool isElectric) : base(model, power)
        {
            IsElectric = isElectric;
        }

        public override string GetInfo()
        {
            string type = IsElectric ? "электрический" : "с ДВС";
            return string.Format("{0}, легковой ({1})", base.GetInfo(), type);
        }

        public override double CalculateTax()
        {
            return Power * (IsElectric ? 5 : 12);
        }
    }
}
