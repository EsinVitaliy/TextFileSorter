using System.Threading.Channels;
using TextFileSorter.Models;

namespace TextFileSorter.Interfaces.ProcessStages;

internal interface IChunkProducer
{
    Task ProduceAsync(ChannelWriter<Chunk> channelWriter, CancellationToken cancellationToken);
}
