namespace PowerSession.Cli
{
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO;
    using Commands;

    internal static class Program
    {
        private static void Main(string[] args)
        {
            var record = new Command("rec")
            {
                new Argument<FileInfo>("file")
            };
            record.Handler = CommandHandler.Create((FileInfo file) =>
            {
                var recordCmd = new RecordCommand(new RecordArgs
                {
                    Filename = file.FullName
                });

                recordCmd.Execute();
            });
            
            var play = new Command("play")
            {
                new Argument<FileInfo>("file")
            };
            play.Handler = CommandHandler.Create((FileInfo file) =>
            {
                var playCommand = new PlayCommand(new PlayArgs{Filename = file.FullName, EnableAnsiEscape = true});
                playCommand.Execute();
            });

            var rooCommand = new RootCommand
            {
                record,
                play
            };

            rooCommand.Description = "PowerSession";

            rooCommand.InvokeAsync(args).Wait();
        }
    }
}