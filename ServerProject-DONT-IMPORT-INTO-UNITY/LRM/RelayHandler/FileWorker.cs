using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace LightReflectiveMirror
{
    public static class FileWorker
    {
        private static string _loginDirectoryName;
        private static JsonSerializerOptions _options = new JsonSerializerOptions { WriteIndented = true };

        private static string LocalPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory) + _loginDirectoryName;

        public static void Init(string loginDirectoryName)
        {
            _loginDirectoryName = loginDirectoryName;
            DirectoryInfo dirInfo = new DirectoryInfo(LocalPath);
            
            if (!dirInfo.Exists)
                dirInfo.Create();
        }

        public static void InitFile<T>(T obj, string fileName)
        {
           if (!File.Exists(LocalPath + fileName))
               WriteInFile(obj, fileName);
        }

        public static async void WriteInFile<T>(T obj, string fileName)
        {
            await using (FileStream fs = new FileStream(LocalPath + fileName, FileMode.OpenOrCreate))
            {
                await JsonSerializer.SerializeAsync<T>(fs, obj, _options);
            }
        }

        public static async Task<T> ReadFile<T>(string fileName)
        {
            await using (FileStream fs = new FileStream(LocalPath + fileName, FileMode.OpenOrCreate))
            {
                return await JsonSerializer.DeserializeAsync<T>(fs);
            }
        }
    }
}