#if NETSTANDARD2_0
using System;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Provides support for init-only setters when targeting frameworks that do not define this type.
    /// </summary>
    internal static class IsExternalInit
    {
    }

    /// <summary>
    /// Marks members as required for object initialization on frameworks missing this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    internal sealed class RequiredMemberAttribute : Attribute
    {
    }

    /// <summary>
    /// Declares compiler feature requirements on frameworks missing this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    internal sealed class CompilerFeatureRequiredAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompilerFeatureRequiredAttribute"/> class.
        /// </summary>
        /// <param name="featureName">Name of the required compiler feature.</param>
        public CompilerFeatureRequiredAttribute(string featureName)
        {
            FeatureName = featureName;
        }

        /// <summary>
        /// Name of the compiler feature.
        /// </summary>
        public string FeatureName
        {
            get;
        }

        /// <summary>
        /// Indicates whether this requirement is optional.
        /// </summary>
        public bool IsOptional
        {
            get;
            init;
        }
    }
}

namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// Indicates a constructor or method sets all required members on frameworks missing this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, Inherited = false)]
    internal sealed class SetsRequiredMembersAttribute : Attribute
    {
    }
}
#endif
