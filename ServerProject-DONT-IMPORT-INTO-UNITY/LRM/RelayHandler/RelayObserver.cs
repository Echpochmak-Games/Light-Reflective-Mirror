using System.Net.Http;
using System.Threading.Tasks;

namespace LightReflectiveMirror
{
    public class RelayObserver
    {
        private const string LOGIN_DIRECTORY_NAME = "API\\";

        private const string SETTINGS_FILE_NAME = "settings.json";
        private const string BEARER_FILE_NAME = "bearer.json";
        private const string LOGIN_FILE_NAME = "login.json";

        private API _api;
        private Login _login;
        private Bearer _bearer;
        
        private HttpClient httpClient = new HttpClient();

        public RelayObserver()
        {
            FileWorker.Init(LOGIN_DIRECTORY_NAME);
            FileWorker.InitFile<API>(new API(), SETTINGS_FILE_NAME);
            FileWorker.InitFile<Login>(new Login(null, null), LOGIN_FILE_NAME);
            FileWorker.InitFile<Bearer>(new Bearer(null, null), BEARER_FILE_NAME);
        }

        public async Task Authentication()
        {
            _api ??= await FileWorker.ReadFile<API>(SETTINGS_FILE_NAME);
            _login ??= await FileWorker.ReadFile<Login>(LOGIN_FILE_NAME);

            using HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post, 
                _api.Domain + string.Format(_api.AuthProviderPath, _login.Email, _login.Password));
            using HttpResponseMessage response = await httpClient.SendAsync(request);

            foreach (var header in response.Headers)
            {
                foreach (var headerValue in header.Value)
                {
                    if (string.Equals(header.Key , "Authorization"))
                        FileWorker.WriteInFile(new Bearer(header.Key, headerValue), BEARER_FILE_NAME);
                }
            }
        }
        
        public async Task Clear(int accessToken)
        {
            _api ??= await FileWorker.ReadFile<API>(SETTINGS_FILE_NAME);
            _bearer ??= await FileWorker.ReadFile<Bearer>(BEARER_FILE_NAME);

            using HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post,
                _api.Domain + string.Format(_api.ClearAccessTokenProviderPath, accessToken));
            
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add(_bearer.Name, "Bearer " + _bearer.Token);
            
            using HttpResponseMessage response = await httpClient.SendAsync(request);
        }
    }
}