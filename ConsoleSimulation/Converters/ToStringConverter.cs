namespace ConsoleSimulation.Converters
{
    internal class ToStringConverter : ValueConverter<string>
    {
        public override string Convert(string s)
        {
            return s;
        }
    }
}