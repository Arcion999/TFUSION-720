using TFusion.Foundation.Identifiers;

namespace TFusion.Foundation.Tests;

public sealed class StrongGuidTests
{
    public static TheoryData<Type> IdentifierTypes => new()
    {
        typeof(DocumentId),
        typeof(ComponentId),
        typeof(BodyId),
        typeof(FeatureId),
        typeof(SketchId),
        typeof(SketchEntityId),
        typeof(ConstraintId),
        typeof(ParameterId),
        typeof(CommandId),
    };

    [Theory]
    [MemberData(nameof(IdentifierTypes))]
    public void M1_U04_EveryIdentifierRejectsEmptyAndRoundTripsInvariantText(Type identifierType)
    {
        var constructor = identifierType.GetConstructor([typeof(Guid)]);
        Assert.NotNull(constructor);
        var emptyError = Assert.ThrowsAny<Exception>(() => constructor.Invoke([Guid.Empty]));
        Assert.NotNull(emptyError);

        var value = Guid.NewGuid();
        var identifier = constructor.Invoke([value]);
        var formatted = identifier.ToString();
        Assert.Equal(value.ToString("D"), formatted);

        var parse = identifierType.GetMethod("Parse", [typeof(string)]);
        Assert.NotNull(parse);
        var parsed = parse.Invoke(null, [formatted]);
        Assert.Equal(identifier, parsed);
        Assert.Equal(identifier.GetHashCode(), parsed!.GetHashCode());
    }

    [Fact]
    public void M1_U04_DistinctIdentifierWrappersAreNotAssignmentCompatible()
    {
        Assert.NotEqual(typeof(BodyId), typeof(FeatureId));
        Assert.False(typeof(BodyId).IsAssignableFrom(typeof(FeatureId)));
        Assert.False(typeof(FeatureId).IsAssignableFrom(typeof(BodyId)));
    }

    [Fact]
    public void M1_U04_ParsingRequiresExactNonEmptyDFormat()
    {
        Assert.False(DocumentId.TryParse(null, out _));
        Assert.False(DocumentId.TryParse(Guid.Empty.ToString("D"), out _));
        Assert.False(DocumentId.TryParse(Guid.NewGuid().ToString("N"), out _));
        Assert.Throws<ArgumentException>(() => DocumentId.Parse(Guid.Empty.ToString("D")));
        Assert.NotEqual(DocumentId.New(), DocumentId.New());
    }
}
