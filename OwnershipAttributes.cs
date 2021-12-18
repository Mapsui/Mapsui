using System;

#pragma warning disable SA1649 /// File name should match first type name
#pragma warning disable SA1402 /// File may only contain a single type

namespace IDisposableAnalyzers
{
    /// <summary>
    /// The return value must be disposed by the caller.
    /// </summary>
    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    internal sealed class GivesOwnershipAttribute : Attribute
    {
    }

    /// <summary>
    /// The return value must not be disposed by the caller.
    /// </summary>
    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    internal sealed class KeepsOwnershipAttribute : Attribute
    {
    }

    /// <summary>
    /// The ownership of instance is transferred and the receiver is responsible for disposing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    internal sealed class TakesOwnershipAttribute : Attribute
    {
    }
}