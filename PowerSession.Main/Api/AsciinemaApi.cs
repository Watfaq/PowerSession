namespace PowerSession.Main.Api
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime.InteropServices;
    using System.Text;
    using PowerSession.Api;

    public class AsciinemaApi : IApiService
    {
        private readonly HttpClient _httpClient;
        private string ApiHost = "https://asciinema.org";
        private string AuthUrl => $"{ApiHost}/connect/{AppConfig.InstallId}";
        private string UploadUrl => $"{ApiHost}/api/asciicasts";

        private static readonly string RuntimeFramework = $"NetCoreApp/{Environment.Version.Major}.{Environment.Version.Minor}";
        private static readonly string OperatingSystem = $"Windows {Environment.OSVersion.Version.Major}-{Environment.OSVersion.VersionString}";
        
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