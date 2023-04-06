using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace LightReflectiveMirror
{
    public static class FileWorker
    {
        private static string loginDirectoryName = "API\\";

        private static string LocalPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory) + loginDirectoryName;

        public static void Init()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(LocalPath);
            if (!dirInfo.Exists)
                dirInfo.Create();
        }

        public static async void WriteInFile<T>(T obj, string fileName)
        {
            await using (FileStream fs = new FileStream(LocalPath + fileName, FileMode.OpenOrCreate))
            {
                await JsonSerializer.SerializeAsync<T>(fs, obj);
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