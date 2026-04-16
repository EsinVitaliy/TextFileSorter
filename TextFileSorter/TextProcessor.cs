using Humanizer;
using System.Runtime;
using System.Threading.Channels;
using TextFileSorter.Interfaces.ProcessStages;
using TextFileSorter.Models;

namespace TextFileSorter;

internal sealed class TextProcessor(
    IChunkProducer chunkProducer,
    IChunkConsumer chunkConsumer,
    IChunkMergeSorter chunkMergeSorter,
    int memoryToReserveInMegabytes
    )
{
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        try
        {
            using (MemoryFailPoint memoryFailPoint = new(memoryToReserveInMegabytes))
            {
                await DoWorkAsync(cancellationToken);
            }
        }
        catch (InsufficientMemoryException)
        {
            Console.WriteLine($"Error: Can not reserve {memoryToReserveInMegabytes.Bytes().Humanize()} of memory");
            throw;
        }
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        var channel = Channel.CreateBounded<Chunk>(
            new BoundedChannelOptions(0)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = true
            });

        var producerTask = chunkProducer.ProduceAsync(channel.Writer, cancellationToken);
        var consumerTask = chunkConsumer.ConsumeAsync(channel, cancellationToken);
        await Task.WhenAll(producerTask, consumerTask);

        await chunkMergeSorter.MergeAsync(cancellationToken);
    }
}
