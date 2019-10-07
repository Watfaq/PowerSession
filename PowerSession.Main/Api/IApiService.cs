namespace PowerSession.Api
{
    public interface IApiService
    {
        void Auth();
        string Upload(string filePath);
    }
}