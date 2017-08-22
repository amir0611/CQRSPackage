namespace DataAccessLogic
{
    public interface IHandleCommand<in TCommand> where TCommand : ICommand
    {
        void Handle(TCommand command);
    }
}
