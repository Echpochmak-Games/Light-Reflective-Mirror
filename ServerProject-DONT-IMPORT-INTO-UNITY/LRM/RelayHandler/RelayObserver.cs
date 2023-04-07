using System.Net.Http;
using System.Threading.Tasks;

namespace LightReflectiveMirror
{
    public class RelayObserver
    {
        private const string SettingsFileName = "settings.json";
        private const string BearerFileName = "bearer.json";
        private const string LoginFileName = "login.json";

        private API api;
        private Login login;
        private Bearer bearer;
        
        private HttpClient httpClient = new HttpClient();

        public RelayObserver()
        {
            FileWorker.Init();
        }

        public async Task Authentication()
        {
            api ??= await FileWorker.ReadFile<API>(SettingsFileName);
            login ??= await FileWorker.ReadFile<Login>(LoginFileName);

            using HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post, 
                api.AuthenticationAddress + string.Format(api.Authentication, login.Email, login.Password));
            using HttpResponseMessage response = await httpClient.SendAsync(request);

            foreach (var header in response.Headers)
            {
                foreach (var headerValue in header.Value)
                {
                    if (string.Equals(header.Key , "Authorization"))
                        FileWorker.WriteInFile(new Bearer(header.Key, headerValue), BearerFileName);
                }
            }
        }
        
        public async Task Clear(int accessToken)
        {
            api ??= await FileWorker.ReadFile<API>(SettingsFileName);
            bearer ??= await FileWorker.ReadFile<Bearer>(BearerFileName);

            using HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post,
                api.AuthenticationAddress + string.Format(api.Clear, accessToken));
            
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add(bearer.Name, "Bearer " + bearer.Token);
            
            using HttpResponseMessage response = await httpClient.SendAsync(request);
        }
    }
}