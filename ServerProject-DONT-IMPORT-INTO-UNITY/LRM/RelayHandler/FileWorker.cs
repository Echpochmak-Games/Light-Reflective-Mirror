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

        private static string GetPath(string fileName) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _loginDirectoryName, fileName);

        public static void Init(string loginDirectoryName)
        {
            _loginDirectoryName = loginDirectoryName;
            DirectoryInfo dirInfo = new DirectoryInfo(GetPath(""));
            
            if (!dirInfo.Exists)
                dirInfo.Create();
        }

        public static void InitFile<T>(T obj, string fileName)
        {
           if (!File.Exists(GetPath(fileName)))
               WriteInFile(obj, fileName);
        }

        public static async void WriteInFile<T>(T obj, string fileName)
        {
            await using (FileStream fs = new FileStream(GetPath(fileName), FileMode.OpenOrCreate))
            {
                await JsonSerializer.SerializeAsync<T>(fs, obj, _options);
            }
        }

        public static async Task<T> ReadFile<T>(string fileName)
        {
            await using (FileStream fs = new FileStream(GetPath(fileName), FileMode.OpenOrCreate))
            {
                return await JsonSerializer.DeserializeAsync<T>(fs);
            }
        }
    }
}