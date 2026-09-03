namespace TFusion.Foundation.Identifiers;

public readonly record struct DocumentId
{
    public DocumentId(Guid value) => Value = StrongGuid.Validate(value, nameof(value));
    public Guid Value { get; }
    public static DocumentId New() => new(StrongGuid.New());
    public static DocumentId Parse(string value) => new(StrongGuid.Parse(value));
    public static bool TryParse(string? value, out DocumentId id) => Try(value, out id);
    public override string ToString() => StrongGuid.Format(Value);
    private static bool Try(string? value, out DocumentId id)
    {
        var success = StrongGuid.TryParse(value, out var parsed);
        id = success ? new DocumentId(parsed) : default;
        return success;
    }
}

public readonly record struct ComponentId
{
    public ComponentId(Guid value) => Value = StrongGuid.Validate(value, nameof(value));
    public Guid Value { get; }
    public static ComponentId New() => new(StrongGuid.New());
    public static ComponentId Parse(string value) => new(StrongGuid.Parse(value));
    public static bool TryParse(string? value, out ComponentId id) => Try(value, out id);
    public override string ToString() => StrongGuid.Format(Value);
    private static bool Try(string? value, out ComponentId id)
    {
        var success = StrongGuid.TryParse(value, out var parsed);
        id = success ? new ComponentId(parsed) : default;
        return success;
    }
}

public readonly record struct BodyId
{
    public BodyId(Guid value) => Value = StrongGuid.Validate(value, nameof(value));
    public Guid Value { get; }
    public static BodyId New() => new(StrongGuid.New());
    public static BodyId Parse(string value) => new(StrongGuid.Parse(value));
    public static bool TryParse(string? value, out BodyId id) => Try(value, out id);
    public override string ToString() => StrongGuid.Format(Value);
    private static bool Try(string? value, out BodyId id)
    {
        var success = StrongGuid.TryParse(value, out var parsed);
        id = success ? new BodyId(parsed) : default;
        return success;
    }
}

public readonly record struct FeatureId
{
    public FeatureId(Guid value) => Value = StrongGuid.Validate(value, nameof(value));
    public Guid Value { get; }
    public static FeatureId New() => new(StrongGuid.New());
    public static FeatureId Parse(string value) => new(StrongGuid.Parse(value));
    public static bool TryParse(string? value, out FeatureId id) => Try(value, out id);
    public override string ToString() => StrongGuid.Format(Value);
    private static bool Try(string? value, out FeatureId id)
    {
        var success = StrongGuid.TryParse(value, out var parsed);
        id = success ? new FeatureId(parsed) : default;
        return success;
    }
}

public readonly record struct SketchId
{
    public SketchId(Guid value) => Value = StrongGuid.Validate(value, nameof(value));
    public Guid Value { get; }
    public static SketchId New() => new(StrongGuid.New());
    public static SketchId Parse(string value) => new(StrongGuid.Parse(value));
    public static bool TryParse(string? value, out SketchId id) => Try(value, out id);
    public override string ToString() => StrongGuid.Format(Value);
    private static bool Try(string? value, out SketchId id)
    {
        var success = StrongGuid.TryParse(value, out var parsed);
        id = success ? new SketchId(parsed) : default;
        return success;
    }
}

public readonly record struct SketchEntityId
{
    public SketchEntityId(Guid value) => Value = StrongGuid.Validate(value, nameof(value));
    public Guid Value { get; }
    public static SketchEntityId New() => new(StrongGuid.New());
    public static SketchEntityId Parse(string value) => new(StrongGuid.Parse(value));
    public static bool TryParse(string? value, out SketchEntityId id) => Try(value, out id);
    public override string ToString() => StrongGuid.Format(Value);
    private static bool Try(string? value, out SketchEntityId id)
    {
        var success = StrongGuid.TryParse(value, out var parsed);
        id = success ? new SketchEntityId(parsed) : default;
        return success;
    }
}

public readonly record struct ConstraintId
{
    public ConstraintId(Guid value) => Value = StrongGuid.Validate(value, nameof(value));
    public Guid Value { get; }
    public static ConstraintId New() => new(StrongGuid.New());
    public static ConstraintId Parse(string value) => new(StrongGuid.Parse(value));
    public static bool TryParse(string? value, out ConstraintId id) => Try(value, out id);
    public override string ToString() => StrongGuid.Format(Value);
    private static bool Try(string? value, out ConstraintId id)
    {
        var success = StrongGuid.TryParse(value, out var parsed);
        id = success ? new ConstraintId(parsed) : default;
        return success;
    }
}

public readonly record struct ParameterId
{
    public ParameterId(Guid value) => Value = StrongGuid.Validate(value, nameof(value));
    public Guid Value { get; }
    public static ParameterId New() => new(StrongGuid.New());
    public static ParameterId Parse(string value) => new(StrongGuid.Parse(value));
    public static bool TryParse(string? value, out ParameterId id) => Try(value, out id);
    public override string ToString() => StrongGuid.Format(Value);
    private static bool Try(string? value, out ParameterId id)
    {
        var success = StrongGuid.TryParse(value, out var parsed);
        id = success ? new ParameterId(parsed) : default;
        return success;
    }
}

public readonly record struct CommandId
{
    public CommandId(Guid value) => Value = StrongGuid.Validate(value, nameof(value));
    public Guid Value { get; }
    public static CommandId New() => new(StrongGuid.New());
    public static CommandId Parse(string value) => new(StrongGuid.Parse(value));
    public static bool TryParse(string? value, out CommandId id) => Try(value, out id);
    public override string ToString() => StrongGuid.Format(Value);
    private static bool Try(string? value, out CommandId id)
    {
        var success = StrongGuid.TryParse(value, out var parsed);
        id = success ? new CommandId(parsed) : default;
        return success;
    }
}
