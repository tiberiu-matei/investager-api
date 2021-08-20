namespace Investager.Core.Interfaces
{
    public interface IFuzzyMatch
    {
        int Compute(string input, string comparedTo);
    }
}
