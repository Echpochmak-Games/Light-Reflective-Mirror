using System.Net.Http;
using System.Threading.Tasks;

namespace LightReflectiveMirror
{
    public class RelayObserver
    {
        private const string SettingsFileName = "settings.json";
        private const string BearerFileName = "bearer.json";
        private const string LoginFileName = "login.json";

        private HttpClient httpClient = new HttpClient();

        public RelayObserver()
        {
            FileWorker.Init();
        }

        public async Task Authentication()
        {
            var api = await FileWorker.ReadFile<API>(SettingsFileName);
            var login = await FileWorker.ReadFile<Login>(LoginFileName);

            using HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post, 
                api.AddressStr + string.Format(api.AuthStr, login.Email, login.Password));
            using HttpResponseMessage response = await httpClient.SendAsync(request);

            foreach (var header in response.Headers)
            {
                foreach (var headerValue in header.Value)
                {
                    if (header.Key == "Authorization")
                        FileWorker.WriteInFile(new Bearer(header.Key, headerValue), BearerFileName);
                }
            }
        }
        
        public async Task Clear(int accessToken)
        {
            var api = await FileWorker.ReadFile<API>(SettingsFileName);
            var bearer = await FileWorker.ReadFile<Bearer>(BearerFileName);

            using HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post,
                api.AddressStr + string.Format(api.ClearStr, accessToken));
            
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add(bearer.Name, "Bearer " + bearer.Token);
            
            using HttpResponseMessage response = await httpClient.SendAsync(request);
        }
    }
}