using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using PowerSession.Commands;
using PowerSession.ConPTY;

namespace PowerSession.Cli
{
    static class Program
    {
        static void Main(string[] args)
        {
            var record = new Command("rec")
            {
                new Argument<FileInfo>("file"),
            };
            record.Handler = CommandHandler.Create((FileInfo file) =>
            {
                var recordCmd = new RecordCommand(new RecordArgs
                {
                    Filename = file.FullName
                });

                recordCmd.Execute();
            });
            
            var rooCommand = new RootCommand
            {
                record
            };

            rooCommand.Description = "PowerSession";
            
            rooCommand.InvokeAsync(args).Wait();
        }
    }
}