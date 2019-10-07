namespace PowerSession.Commands
{
    public class UploadCommand: BaseCommand, ICommand
    {
        private readonly string _filePath;
        
        public UploadCommand(string filePath) : base(true)
        {
            _filePath = filePath;
        }
        public int Execute()
        {
            var result = Api.Upload(_filePath);
            if (!string.IsNullOrEmpty(result))
            {
                LogInfo($"Result Url: {result}");
                return 0;
            }

            return 1;
        }
    }
}