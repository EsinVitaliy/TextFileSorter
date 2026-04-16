using AwesomeAssertions;
using TextFileSorter.Comparers;
using TextFileSorter.Models;

namespace TextFileSorter.Tests;

public class ComparerTests
{
    private static readonly TextLineComparer Comparer = new();

    [Theory]
    [InlineData(new[] { "45.za", "12.ay" }, new[] { "12.ay", "45.za" })]
    [InlineData(new[] { "12.ab", "12.aa" }, new[] { "12.aa", "12.ab" })]
    [InlineData(new[] { "45.aa", "12.aa" }, new[] { "12.aa", "45.aa" })]
    [InlineData(new[] { "13.aa", "12.aa" }, new[] { "12.aa", "13.aa" })]
    [InlineData(new[] { "12.ay", "45.za" }, new[] { "12.ay", "45.za" })]
    public void T01_should_sort(string[] initialText, string[] sortedText)
    {
        List<TextLine> list = initialText
            .Select(e => new TextLine(e))
            .ToList();

        list
            .Sort(Comparer);

        list
            .First().Value
            .Should().BeEquivalentTo(sortedText.First());

        list
            .Last().Value
            .Should().BeEquivalentTo(sortedText.Last());
    }
}
