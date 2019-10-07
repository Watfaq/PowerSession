namespace PowerSession.Commands
{
    public class AuthCommand : BaseCommand, ICommand
    {
        public int Execute()
        {
            Api.Auth();
            return 0;
        }
    }
}