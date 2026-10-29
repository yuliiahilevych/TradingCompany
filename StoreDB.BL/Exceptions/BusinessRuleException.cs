namespace StoreBL.Exceptions
{
    public class BusinessRuleException : Exception
    {
        public IReadOnlyList<string> Errors { get; }

        public BusinessRuleException(string error) : this(new[] { error })
        {
        }

        public BusinessRuleException(IReadOnlyList<string> errors)
            : base(string.Join(Environment.NewLine, errors))
        {
            Errors = errors;
        }
    }
}