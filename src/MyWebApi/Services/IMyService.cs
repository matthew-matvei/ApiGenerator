using System.Threading.Tasks;

namespace MyWebApi.Services
{
    public interface IMyService
    {
         Task CreateSummedValues(int x, int y);
    }
}