namespace PowerSession.Cli
{
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO;
    using Commands;
    using Main.Commands;

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

            var auth = new Command("auth")
            {
                Handler = CommandHandler.Create(() =>
                {
                    var authCommand = new AuthCommand();
                    authCommand.Execute();
                })
            };
            
            var upload = new Command("upload")
            {
                new Argument<FileInfo>("file")
            };
            upload.Handler = CommandHandler.Create((FileInfo file) =>
            {
                var uploadCommand = new UploadCommand(file.FullName);
                uploadCommand.Execute();
            });

            var rooCommand = new RootCommand
            {
                record,
                play,
                auth,
                upload
            };

            rooCommand.Description = "Record, Play and Share your PowerShell Session.";

            rooCommand.InvokeAsync(args).Wait();
        }
    }
}