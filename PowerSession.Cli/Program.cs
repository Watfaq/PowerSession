namespace PowerSession.Cli
{
    using System;
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
                new Argument<FileInfo>("file"){Description = "The filename to save the record"},
                new Option(new []{"--command", "-c"}, "The command to record, default to be powershell.exe", typeof(string))
            };
            record.Description = "Record and save a session";
            record.Handler = CommandHandler.Create((FileInfo file, string command) =>
            {
                var recordCmd = new RecordCommand(new RecordArgs
                {
                    Filename = file.FullName,
                    Command = command
                });

                recordCmd.Execute();
            });
            
            var play = new Command("play")
            {
                new Argument<FileInfo>("file"){Description = "The record session"}
            };
            play.Description = "Play a recorded session";
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
                }), 
                Description = "Auth with asciinema.org"
            };
            
            var upload = new Command("upload")
            {
                new Argument<FileInfo>("file"){Description = "The file to be uploaded"}
            };
            upload.Description = "Upload a session to ascinema.org";
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