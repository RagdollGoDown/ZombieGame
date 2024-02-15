public interface IDataService
{
    bool SaveData<T>(string RelativePath, T data, bool encrypted = true);

    T LoadData<T>(string RelativePath, bool encrypted = true);
}
