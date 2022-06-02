namespace PowerSession.Cli
{
    using System.CommandLine;
    using System.IO;
    using Commands;
    using Main.Commands;

    internal static class Program
    {
        private static void Main(string[] args)
        {
            var recordArg = new Argument<FileInfo>("file") {Description = "The filename to save the record"};
            var recordOpt = new Option<string>(new[] {"--command", "-c"}, "The command to record, defaults to $SHELL");

            var record = new Command("rec")
            {
                recordArg,
                recordOpt,
            };
            record.Description = "Record and save a session";
            record.SetHandler((FileInfo file, string command) =>
            {
                var recordCmd = new RecordCommand(new RecordArgs
                {
                    Filename = file.FullName,
                    Command = command
                });

                recordCmd.Execute();
            }, recordArg, recordOpt);

            var playArg = new Argument<FileInfo>("file") {Description = "The record session"};
            var play = new Command("play")
            {
                playArg,
            };
            play.Description = "Play a recorded session";
            play.SetHandler((FileInfo file) =>
            {
                var playCommand = new PlayCommand(new PlayArgs{Filename = file.FullName, EnableAnsiEscape = true});
                playCommand.Execute();
            }, playArg);

            var auth = new Command("auth")
            {
                Description = "Auth with asciinema.org"
            };
            auth.SetHandler(() =>
            {
                var authCommand = new AuthCommand();
                authCommand.Execute();
            });

            var uploadArg = new Argument<FileInfo>("file") {Description = "The file to be uploaded"};
            var upload = new Command("upload")
            {
                uploadArg,
            };
            upload.Description = "Upload a session to ascinema.org";
            upload.SetHandler((FileInfo file) =>
            {
                var uploadCommand = new UploadCommand(file.FullName);
                uploadCommand.Execute();
            }, uploadArg);

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