using System.Threading.Channels;
using TextFileSorter.Models;

namespace TextFileSorter.Interfaces.ProcessStages;

internal interface IChunkConsumer
{
    Task ConsumeAsync(Channel<Chunk> channel, CancellationToken cancellationToken);
}
