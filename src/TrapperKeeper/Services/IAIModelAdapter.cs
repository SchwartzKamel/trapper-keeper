using System.Collections.Generic;
using System.Threading.Tasks;
using TrapperKeeper;

namespace TrapperKeeper.Services
{
    public interface IAIModelAdapter
    {
        Task<string> GenerateResponseAsync(IEnumerable<ChatMessage> messages);
    }
}