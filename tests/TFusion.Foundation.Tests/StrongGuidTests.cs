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

    [Fact]
    public void M1_U04_AllIdentifierTryParseAndNewPathsAreExercised()
    {
        var text = Guid.NewGuid().ToString("D");

        Assert.True(DocumentId.TryParse(text, out var document));
        Assert.True(ComponentId.TryParse(text, out var component));
        Assert.True(BodyId.TryParse(text, out var body));
        Assert.True(FeatureId.TryParse(text, out var feature));
        Assert.True(SketchId.TryParse(text, out var sketch));
        Assert.True(SketchEntityId.TryParse(text, out var sketchEntity));
        Assert.True(ConstraintId.TryParse(text, out var constraint));
        Assert.True(ParameterId.TryParse(text, out var parameter));
        Assert.True(CommandId.TryParse(text, out var command));

        Assert.Equal(text, document.ToString());
        Assert.Equal(text, component.ToString());
        Assert.Equal(text, body.ToString());
        Assert.Equal(text, feature.ToString());
        Assert.Equal(text, sketch.ToString());
        Assert.Equal(text, sketchEntity.ToString());
        Assert.Equal(text, constraint.ToString());
        Assert.Equal(text, parameter.ToString());
        Assert.Equal(text, command.ToString());

        Assert.NotEqual(ComponentId.New(), ComponentId.New());
        Assert.NotEqual(BodyId.New(), BodyId.New());
        Assert.NotEqual(FeatureId.New(), FeatureId.New());
        Assert.NotEqual(SketchId.New(), SketchId.New());
        Assert.NotEqual(SketchEntityId.New(), SketchEntityId.New());
        Assert.NotEqual(ConstraintId.New(), ConstraintId.New());
        Assert.NotEqual(ParameterId.New(), ParameterId.New());
        Assert.NotEqual(CommandId.New(), CommandId.New());

        Assert.False(ComponentId.TryParse("invalid", out _));
        Assert.False(BodyId.TryParse("invalid", out _));
        Assert.False(FeatureId.TryParse("invalid", out _));
        Assert.False(SketchId.TryParse("invalid", out _));
        Assert.False(SketchEntityId.TryParse("invalid", out _));
        Assert.False(ConstraintId.TryParse("invalid", out _));
        Assert.False(ParameterId.TryParse("invalid", out _));
        Assert.False(CommandId.TryParse("invalid", out _));
    }
}
