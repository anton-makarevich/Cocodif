using Sanet.Cocodif.Models;

namespace Sanet.Cocodif.Reporters;

public interface IReporter
{
    string Generate(DiffCoverageReport report);
}