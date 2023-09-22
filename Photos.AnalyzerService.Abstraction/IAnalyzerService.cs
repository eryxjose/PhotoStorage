using System;
using System.Threading.Tasks;

namespace Photos.AnalyzerService.Abstraction
{
    public interface IAnalyzerService
    {
        Task<dynamic> AnalyzeAsync(byte[] image);
    }
}
