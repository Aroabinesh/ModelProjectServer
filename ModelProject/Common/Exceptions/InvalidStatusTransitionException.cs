namespace ModelProject.Common.Exceptions;

public class InvalidStatusTransitionException : Exception
{
    public InvalidStatusTransitionException(string message) : base(message)
    {
    }
}
