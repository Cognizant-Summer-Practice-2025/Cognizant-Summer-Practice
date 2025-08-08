using FluentAssertions;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = false)]

public static class TestConfiguration
{
    static TestConfiguration()
    {
        // Configure FluentAssertions to handle cyclic references
        AssertionOptions.AssertEquivalencyUsing(options =>
        {
            options.IgnoringCyclicReferences();
            return options;
        });
        
        // Configure assertion formatting to avoid deep recursion in error messages
        AssertionOptions.FormattingOptions.MaxDepth = 3;
    }
} 