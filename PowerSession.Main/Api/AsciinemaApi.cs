
namespace PowerSession.Main.Api
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Text;
    using PowerSession.Api;
    using System.Reflection;

    public class AsciinemaApi : IApiService
    {
        private readonly HttpClient _httpClient;
        private string ApiHost = "https://asciinema.org";
        private string AuthUrl => $"{ApiHost}/connect/{AppConfig.InstallId}";
        private string UploadUrl => $"{ApiHost}/api/asciicasts";


        private static readonly OperatingSystem Os = Environment.OSVersion;
        private static readonly string RuntimeFramework = $"dotnet/{Environment.Version.Major}.{Environment.Version.Minor}.{Environment.Version.Build}";
        private static readonly string OperatingSystem = $"Windows/{Os.Version.Major}-{Os.Version.Major}.{Os.Version.Minor}.{Os.Version.Build}-SP{Os.ServicePack.Split(' ').Skip(2).FirstOrDefault() ?? "0"}";

        public string Upload(string filePath)
        {
            var req = new MultipartFormDataContent();
            var cast = new ByteArrayContent(File.ReadAllBytes(filePath));
            cast.Headers.ContentType = MediaTypeHeaderValue.Parse(System.Net.Mime.MediaTypeNames.Text.Plain);
            req.Add(cast, "asciicast", "ascii.cast");

            var res = _httpClient.PostAsync(UploadUrl, req).Result;
            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine("Upload Failed:");
                Console.WriteLine(res.Content.ReadAsStringAsync().Result);
                return null;
            }
            return res.Headers.Location.ToString();
        }

        public AsciinemaApi()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(System.Net.Mime.MediaTypeNames.Application.Json));

            _httpClient.DefaultRequestHeaders.Add("User-Agent",$"asciinema/2.0.0 {RuntimeFramework} {OperatingSystem}");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"user:{AppConfig.InstallId}")));
        }

        public void Auth()
        {
            Console.WriteLine("Open the following URL in a web browser to link your" +
                $"install ID with your {ApiHost} user account:\n\n" +
                $"{AuthUrl}\n\n" +
                "This will associate all recordings uploaded from this machine " +
                "(past and future ones) to your account, " +
                $"and allow you to manage them (change title/theme, delete) at {ApiHost}.");
        }
    }
}