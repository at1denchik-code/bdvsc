namespace VehicleFleetLab
{
    public interface IStorable
    {
        string FilePath { get; }
        void Save(string path);
    }
}
