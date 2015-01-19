using System;
using System.Threading.Tasks;

namespace ThinkTel.uControl.Cdrs
{
    public interface ICdrClient
    {
		Task<CdrFile[]> ListCdrFilesAsync();
		Task<Cdr[]> GetCdrFileAsync(string cdrFile);
    }
}
