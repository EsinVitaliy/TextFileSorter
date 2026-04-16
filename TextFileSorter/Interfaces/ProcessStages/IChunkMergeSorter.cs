namespace TextFileSorter.Interfaces.ProcessStages;

internal interface IChunkMergeSorter
{
    Task MergeAsync(CancellationToken cancellationToken);
}
