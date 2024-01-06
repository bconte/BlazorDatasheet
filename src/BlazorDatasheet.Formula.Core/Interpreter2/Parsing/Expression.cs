namespace BlazorDatasheet.Formula.Core.Interpreter2.Parsing;

public abstract class Node
{
    public abstract NodeKind Kind { get; }
}

public abstract class Expression : Node
{
    public abstract string ToExpressionText();
}