namespace VehicleFleetLab
{
    // Интерфейс для объектов, которые умеют сохранять данные в файл.
    public interface IStorable
    {
        string FilePath { get; }
        void Save(string path);
    }
}
