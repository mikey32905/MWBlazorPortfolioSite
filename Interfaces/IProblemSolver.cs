namespace MWBlazorPortfolioSite.Interfaces
{
    public interface IProblemSolver
    {
        bool ProblemEncountered { get; }
        void SolveWithInnovation();
    }
}
