namespace Plates.Engine;

/// <summary>What a scheme's <c>Decide</c> returns: the unrounded Target, the code and the new scheme state.</summary>
public sealed record SchemeDecision<TState>(Weight Target, DecisionCode Code, TState State);
