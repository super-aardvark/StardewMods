using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider for a built-in condition whose value may change with the context.</summary>
    internal class ConditionTypeValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>The allowed root values (or <c>null</c> if any value is allowed).</summary>
        private readonly InvariantHashSet AllowedRootValues;

        /// <summary>Get the current values.</summary>
        private readonly Func<InvariantHashSet> FetchValues;

        /// <summary>Get whether the value provider is applicable in the current context, or <c>null</c> if it's always applicable.</summary>
        private readonly Func<bool> IsValidInContextImpl;

        /// <summary>The values as of the last context update.</summary>
        private readonly InvariantHashSet Values = new InvariantHashSet();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="type">The condition type.</param>
        /// <param name="values">Get the current values.</param>
        /// <param name="isValidInContext">Get whether the value provider is applicable in the current context, or <c>null</c> if it's always applicable.</param>
        /// <param name="canHaveMultipleValues">Whether the root may contain multiple values.</param>
        /// <param name="allowedValues">The allowed values (or <c>null</c> if any value is allowed).</param>
        public ConditionTypeValueProvider(ConditionType type, Func<IEnumerable<string>> values, Func<bool> isValidInContext = null, bool canHaveMultipleValues = false, IEnumerable<string> allowedValues = null)
            : base(type, canHaveMultipleValues)
        {
            this.IsValidInContextImpl = isValidInContext;
            this.AllowedRootValues = allowedValues != null ? new InvariantHashSet(allowedValues) : null;
            this.FetchValues = () => new InvariantHashSet(values());
            this.EnableInputArguments(required: false, canHaveMultipleValues: false);
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="type">The condition type.</param>
        /// <param name="value">Get the current value.</param>
        /// <param name="isValidInContext">Get whether the value provider is applicable in the current context, or <c>null</c> if it's always applicable.</param>
        /// <param name="canHaveMultipleValues">Whether the root may contain multiple values.</param>
        /// <param name="allowedValues">The allowed values (or <c>null</c> if any value is allowed).</param>
        public ConditionTypeValueProvider(ConditionType type, Func<string> value, Func<bool> isValidInContext = null, bool canHaveMultipleValues = false, IEnumerable<string> allowedValues = null)
            : this(type, () => new[] { value() }, isValidInContext, canHaveMultipleValues, allowedValues) { }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public override bool UpdateContext(IContext context)
        {
            return this.IsChanged(this.Values, () =>
            {
                this.Values.Clear();
                if (this.MarkReady(this.IsValidInContextImpl == null || this.IsValidInContextImpl()))
                {
                    foreach (string value in this.FetchValues())
                        this.Values.Add(value);
                }
            });
        }

        /// <summary>Get whether the token always chooses from a set of known values for the given input. Mutually exclusive with <see cref="IValueProvider.HasBoundedRangeValues"/>.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="allowedValues">The possible values for the input.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override bool HasBoundedValues(ITokenString input, out InvariantHashSet allowedValues)
        {
            allowedValues = input.IsMeaningful()
                ? InvariantHashSet.Boolean()
                : this.AllowedRootValues;
            return allowedValues != null;
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override IEnumerable<string> GetValues(ITokenString input)
        {
            this.AssertInputArgument(input);

            if (input.IsMeaningful())
                return new[] { this.Values.Contains(input.Value).ToString() };
            return this.Values;
        }
    }
}
