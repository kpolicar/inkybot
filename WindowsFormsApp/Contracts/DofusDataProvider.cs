using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WindowsFormsApp
{
    public interface DofusDataProvider
    {
        Task<Dictionary<string, string>> Stats();
        Task<Dictionary<string, string>> Max();
        Task<Dictionary<string, string>> Min();
    }
}