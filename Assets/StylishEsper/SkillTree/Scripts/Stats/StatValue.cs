//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Newtonsoft.Json;
using System;

namespace Esper.SkillTree.Stats
{
    /// <summary>
    /// Represents a stat value.
    /// </summary>
    [Serializable]
    public struct StatValue
    {
        private const string strictTypeError = "Skill Tree: stat value type mismatch. You can disable strict stat value types from Skill Tree's settings.";

        /// <summary>
        /// The identity of the stat.
        /// </summary>
        [NonSerialized, JsonIgnore]
        public StatIdentity identity;

        /// <summary>
        /// The value.
        /// </summary>
        public Value value;

        /// <summary>
        /// The value as a string.
        /// </summary>
        [NonSerialized, JsonIgnore]
        public string stringValue;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="identity">The identity of this stat.</param>
        /// <param name="value">The stat value..</param>
        public StatValue(StatIdentity identity, Value value)
        {
            this.identity = identity;
            this.value = identity.Clamp(value);
            stringValue = string.Empty;
            stringValue = ToString();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="identity">The identity of this stat.</param>
        /// <param name="value">The stat value.</param>
        public StatValue(StatIdentity identity, int value)
        {
            this.identity = identity;

            value = identity.Clamp(value);

            switch (identity.numericType)
            {
                case NumericType.Float:
                    this.value = new Value() { floatValue = value };
                    break;
                case NumericType.Integer:
                    this.value = new Value() { integerValue = value };
                    break;
                default:
                    this.value = new Value();
                    break;
            }

            stringValue = string.Empty;
            stringValue = ToString();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="identity">The identity of this stat.</param>
        /// <param name="value">The current value.</param>
        public StatValue(StatIdentity identity, float value)
        {
            this.identity = identity;

            value = identity.Clamp(value);

            switch (identity.numericType)
            {
                case NumericType.Float:
                    this.value = new Value() { floatValue = value };
                    break;
                case NumericType.Integer:
                    this.value = new Value() { integerValue = (int)value };
                    break;
                default:
                    this.value = new Value();
                    break;
            }

            stringValue = string.Empty;
            stringValue = ToString();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="identity">The identity of this stat.</param>
        public StatValue(StatIdentity identity)
        {
            this.identity = identity;
            value = new Value();
            stringValue = string.Empty;
            stringValue = ToString();
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetValue(int value)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                value = identity.Clamp(value);

                if (identity.numericType != NumericType.Integer)
                {
                    throw new InvalidOperationException(strictTypeError);
                }

                this.value.integerValue = value;
                stringValue = ToString();
            }
            else
            {
                SetValueWithConversion(value);
            }
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetValue(float value)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                value = identity.Clamp(value);

                if (identity.numericType != NumericType.Float)
                {
                    throw new InvalidOperationException(strictTypeError);
                }

                this.value.floatValue = value;
                stringValue = ToString();
            }
            else
            {
                SetValueWithConversion(value);
            }
        }

        /// <summary>
        /// Sets the value to the relevant field based on the StatIdentity with value conversion if
        /// necessary.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetValueWithConversion(int value)
        {
            value = identity.Clamp(value);

            switch (identity.numericType)
            {
                case NumericType.Float:
                    this.value.floatValue = value;
                    break;
                case NumericType.Integer:
                    this.value.integerValue = value;
                    break;
                default:
                    this.value.floatValue = value;
                    break;
            }

            stringValue = ToString();
        }

        /// <summary>
        /// Sets the value to the relevant field based on the StatIdentity with value conversion if
        /// necessary.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetValueWithConversion(float value)
        {
            value = identity.Clamp(value);

            switch (identity.numericType)
            {
                case NumericType.Float:
                    this.value.floatValue = value;
                    break;
                case NumericType.Integer:
                    this.value.integerValue = (int)value;
                    break;
                default:
                    this.value.floatValue = value;
                    break;
            }

            stringValue = ToString();
        }

        public static StatValue operator /(StatValue a, float value)
        {
            switch (a.identity.numericType)
            {
                case NumericType.Float:
                    return new StatValue(a.identity, a.value.floatValue / value);
                case NumericType.Integer:
                    return new StatValue(a.identity, a.value.integerValue / value);
                default:
                    return new StatValue();
            }
        }

        public static StatValue operator /(StatValue a, int value)
        {
            switch (a.identity.numericType)
            {
                case NumericType.Float:
                    return new StatValue(a.identity, a.value.floatValue / value);
                case NumericType.Integer:
                    return new StatValue(a.identity, a.value.integerValue / value);
                default:
                    return new StatValue();
            }
        }

        public static StatValue operator /(StatValue a, StatValue b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                if (!a.IsComparable(b))
                {
                    throw new InvalidOperationException(strictTypeError);
                }

                switch (a.identity.numericType)
                {
                    case NumericType.Float:
                        return new StatValue(a.identity, a.value.floatValue / b.value.floatValue);
                    case NumericType.Integer:
                        return new StatValue(a.identity, a.value.integerValue / b.value.integerValue);
                    default:
                        return new StatValue();
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        switch (b.identity.numericType)
                        {
                            case NumericType.Integer:
                                return new StatValue(a.identity, a.value.integerValue / b.value.integerValue);
                            case NumericType.Float:
                                return new StatValue(a.identity, a.value.integerValue / b.value.floatValue);
                            default:
                                return new StatValue();
                        }
                    case NumericType.Float:
                        switch (b.identity.numericType)
                        {
                            case NumericType.Integer:
                                return new StatValue(a.identity, a.value.floatValue / b.value.integerValue);
                            case NumericType.Float:
                                return new StatValue(a.identity, a.value.floatValue / b.value.floatValue);
                            default:
                                return new StatValue();
                        }
                    default:
                        return new StatValue();
                }
            }
        }

        public static StatValue operator *(StatValue a, float value)
        {
            switch (a.identity.numericType)
            {
                case NumericType.Integer:
                    return new StatValue(a.identity, a.value.integerValue * value);
                case NumericType.Float:
                    return new StatValue(a.identity, a.value.floatValue * value);
                default:
                    return new StatValue();
            }
        }

        public static StatValue operator *(StatValue a, int value)
        {
            switch (a.identity.numericType)
            {
                case NumericType.Integer:
                    return new StatValue(a.identity, a.value.integerValue * value);
                case NumericType.Float:
                    return new StatValue(a.identity, a.value.floatValue * value);
                default:
                    return new StatValue();
            }
        }

        public static StatValue operator *(StatValue a, StatValue b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                if (!a.IsComparable(b))
                {
                    throw new InvalidOperationException(strictTypeError);
                }

                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return new StatValue(a.identity, a.value.integerValue * b.value.integerValue);
                    case NumericType.Float:
                        return new StatValue(a.identity, a.value.floatValue * b.value.floatValue);
                    default:
                        return new StatValue();
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        switch (b.identity.numericType)
                        {
                            case NumericType.Integer:
                                return new StatValue(a.identity, a.value.integerValue * b.value.integerValue);
                            case NumericType.Float:
                                return new StatValue(a.identity, a.value.integerValue * b.value.floatValue);
                            default:
                                return new StatValue();
                        }
                    case NumericType.Float:
                        switch (b.identity.numericType)
                        {
                            case NumericType.Integer:
                                return new StatValue(a.identity, a.value.floatValue * b.value.integerValue);
                            case NumericType.Float:
                                return new StatValue(a.identity, a.value.floatValue * b.value.floatValue);
                            default:
                                return new StatValue();
                        }
                    default:
                        return new StatValue();
                }
            }
        }

        public static StatValue operator +(StatValue a, float value)
        {
            switch (a.identity.numericType)
            {
                case NumericType.Integer:
                    return new StatValue(a.identity, a.value.integerValue + value);
                case NumericType.Float:
                    return new StatValue(a.identity, a.value.floatValue + value);
                default:
                    return new StatValue();
            }
        }

        public static StatValue operator +(StatValue a, int value)
        {
            switch (a.identity.numericType)
            {
                case NumericType.Integer:
                    return new StatValue(a.identity, a.value.integerValue + value);
                case NumericType.Float:
                    return new StatValue(a.identity, a.value.floatValue + value);
                default:
                    return new StatValue();
            }
        }

        public static StatValue operator +(StatValue a, StatValue b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                if (!a.IsComparable(b))
                {
                    throw new InvalidOperationException(strictTypeError);
                }

                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return new StatValue(a.identity, a.value.integerValue + b.value.integerValue);
                    case NumericType.Float:
                        return new StatValue(a.identity, a.value.floatValue + b.value.floatValue);
                    default:
                        return new StatValue();
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        switch (b.identity.numericType)
                        {
                            case NumericType.Integer:
                                return new StatValue(a.identity, a.value.integerValue + b.value.integerValue);
                            case NumericType.Float:
                                return new StatValue(a.identity, a.value.integerValue + b.value.floatValue);
                            default:
                                return new StatValue();
                        }
                    case NumericType.Float:
                        switch (b.identity.numericType)
                        {
                            case NumericType.Integer:
                                return new StatValue(a.identity, a.value.floatValue + b.value.integerValue);
                            case NumericType.Float:
                                return new StatValue(a.identity, a.value.floatValue + b.value.floatValue);
                            default:
                                return new StatValue();
                        }
                    default:
                        return new StatValue();
                }
            }
        }

        public static StatValue operator -(StatValue a, float value)
        {
            switch (a.identity.numericType)
            {
                case NumericType.Integer:
                    return new StatValue(a.identity, a.value.integerValue - value);
                case NumericType.Float:
                    return new StatValue(a.identity, a.value.floatValue - value);
                default:
                    return new StatValue();
            }
        }

        public static StatValue operator -(StatValue a, int value)
        {
            switch (a.identity.numericType)
            {
                case NumericType.Integer:
                    return new StatValue(a.identity, a.value.integerValue - value);
                case NumericType.Float:
                    return new StatValue(a.identity, a.value.floatValue - value);
                default:
                    return new StatValue();
            }
        }

        public static StatValue operator -(StatValue a, StatValue b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                if (!a.IsComparable(b))
                {
                    throw new InvalidOperationException(strictTypeError);
                }

                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return new StatValue(a.identity, a.value.integerValue - b.value.integerValue);
                    case NumericType.Float:
                        return new StatValue(a.identity, a.value.floatValue - b.value.floatValue);
                    default:
                        return new StatValue();
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        switch (b.identity.numericType)
                        {
                            case NumericType.Integer:
                                return new StatValue(a.identity, a.value.integerValue - b.value.integerValue);
                            case NumericType.Float:
                                return new StatValue(a.identity, a.value.integerValue - b.value.floatValue);
                            default:
                                return new StatValue();
                        }
                    case NumericType.Float:
                        switch (b.identity.numericType)
                        {
                            case NumericType.Integer:
                                return new StatValue(a.identity, a.value.floatValue - b.value.integerValue);
                            case NumericType.Float:
                                return new StatValue(a.identity, a.value.floatValue - b.value.floatValue);
                            default:
                                return new StatValue();
                        }
                    default:
                        return new StatValue();
                }
            }
        }

        public static bool operator ==(StatValue a, StatValue b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                if (!a.IsComparable(b))
                {
                    throw new InvalidOperationException(strictTypeError);
                }

                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue == b.value.integerValue;
                    case NumericType.Float:
                        return a.value.floatValue == b.value.floatValue;
                    default:
                        return false;
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        switch (b.identity.numericType)
                        {
                            case NumericType.Integer:
                                return a.value.integerValue == b.value.integerValue;
                            case NumericType.Float:
                                return a.value.integerValue == b.value.floatValue;
                            default:
                                return false;
                        }
                    case NumericType.Float:
                        switch (b.identity.numericType)
                        {
                            case NumericType.Integer:
                                return a.value.floatValue == b.value.integerValue;
                            case NumericType.Float:
                                return a.value.floatValue == b.value.floatValue;
                            default:
                                return false;
                        }
                    default:
                        return false;
                }
            }
        }

        public static bool operator ==(StatValue a, float b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        throw new InvalidOperationException(strictTypeError);
                    case NumericType.Float:
                        return a.value.floatValue == b;
                    default:
                        return false;
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue == b;
                    case NumericType.Float:
                        return a.value.floatValue == b;
                    default:
                        return false;
                }
            }
        }

        public static bool operator ==(StatValue a, int b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue == b;
                    case NumericType.Float:
                        throw new InvalidOperationException(strictTypeError);
                    default:
                        return false;
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue == b;
                    case NumericType.Float:
                        return a.value.floatValue == b;
                    default:
                        return false;
                }
            }
        }

        public static bool operator !=(StatValue a, StatValue b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                if (!a.IsComparable(b))
                {
                    throw new InvalidOperationException(strictTypeError);
                }

                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue != b.value.integerValue;
                    case NumericType.Float:
                        return a.value.floatValue != b.value.floatValue;
                    default:
                        return false;
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        switch (b.identity.numericType)
                        {
                            case NumericType.Integer:
                                return a.value.integerValue != b.value.integerValue;
                            case NumericType.Float:
                                return a.value.integerValue != b.value.floatValue;
                            default:
                                return false;
                        }
                    case NumericType.Float:
                        switch (b.identity.numericType)
                        {
                            case NumericType.Integer:
                                return a.value.floatValue != b.value.integerValue;
                            case NumericType.Float:
                                return a.value.floatValue != b.value.floatValue;
                            default:
                                return false;
                        }
                    default:
                        return false;
                }
            }
        }

        public static bool operator !=(StatValue a, float b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        throw new InvalidOperationException(strictTypeError);
                    case NumericType.Float:
                        return a.value.floatValue != b;
                    default:
                        return false;
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue != b;
                    case NumericType.Float:
                        return a.value.floatValue != b;
                    default:
                        return false;
                }
            }
        }

        public static bool operator !=(StatValue a, int b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue != b;
                    case NumericType.Float:
                        throw new InvalidOperationException(strictTypeError);
                    default:
                        return false;
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue != b;
                    case NumericType.Float:
                        return a.value.floatValue != b;
                    default:
                        return false;
                }
            }
        }

        public static bool operator >(StatValue a, StatValue b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                if (!a.IsComparable(b))
                {
                    throw new InvalidOperationException(strictTypeError);
                }

                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue > b.value.integerValue;
                    case NumericType.Float:
                        return a.value.floatValue > b.value.floatValue;
                    default:
                        return false;
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        switch (b.identity.numericType)
                        {
                            case NumericType.Integer:
                                return a.value.integerValue > b.value.integerValue;
                            case NumericType.Float:
                                return a.value.integerValue > b.value.floatValue;
                            default:
                                return false;
                        }
                    case NumericType.Float:
                        switch (b.identity.numericType)
                        {
                            case NumericType.Integer:
                                return a.value.floatValue > b.value.integerValue;
                            case NumericType.Float:
                                return a.value.floatValue > b.value.floatValue;
                            default:
                                return false;
                        }
                    default:
                        return false;
                }
            }
        }

        public static bool operator >(StatValue a, float b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        throw new InvalidOperationException(strictTypeError);
                    case NumericType.Float:
                        return a.value.floatValue > b;
                    default:
                        return false;
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue > b;
                    case NumericType.Float:
                        return a.value.floatValue > b;
                    default:
                        return false;
                }
            }
        }

        public static bool operator >(StatValue a, int b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue > b;
                    case NumericType.Float:
                        throw new InvalidOperationException(strictTypeError);
                    default:
                        return false;
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue > b;
                    case NumericType.Float:
                        return a.value.floatValue > b;
                    default:
                        return false;
                }
            }
        }

        public static bool operator <(StatValue a, StatValue b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                if (!a.IsComparable(b))
                {
                    throw new InvalidOperationException(strictTypeError);
                }

                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue < b.value.integerValue;
                    case NumericType.Float:
                        return a.value.floatValue < b.value.floatValue;
                    default:
                        return false;
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        switch (b.identity.numericType)
                        {
                            case NumericType.Integer:
                                return a.value.integerValue < b.value.integerValue;
                            case NumericType.Float:
                                return a.value.integerValue < b.value.floatValue;
                            default:
                                return false;
                        }
                    case NumericType.Float:
                        switch (b.identity.numericType)
                        {
                            case NumericType.Integer:
                                return a.value.floatValue < b.value.integerValue;
                            case NumericType.Float:
                                return a.value.floatValue < b.value.floatValue;
                            default:
                                return false;
                        }
                    default:
                        return false;
                }
            }
        }

        public static bool operator <(StatValue a, float b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        throw new InvalidOperationException(strictTypeError);
                    case NumericType.Float:
                        return a.value.floatValue < b;
                    default:
                        return false;
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue < b;
                    case NumericType.Float:
                        return a.value.floatValue < b;
                    default:
                        return false;
                }
            }
        }

        public static bool operator <(StatValue a, int b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue < b;
                    case NumericType.Float:
                        throw new InvalidOperationException(strictTypeError);
                    default:
                        return false;
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue < b;
                    case NumericType.Float:
                        return a.value.floatValue < b;
                    default:
                        return false;
                }
            }
        }

        public static bool operator >=(StatValue a, StatValue b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                if (!a.IsComparable(b))
                {
                    throw new InvalidOperationException(strictTypeError);
                }

                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue >= b.value.integerValue;
                    case NumericType.Float:
                        return a.value.floatValue >= b.value.floatValue;
                    default:
                        return false;
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        switch (b.identity.numericType)
                        {
                            case NumericType.Integer:
                                return a.value.integerValue >= b.value.integerValue;
                            case NumericType.Float:
                                return a.value.integerValue >= b.value.floatValue;
                            default:
                                return false;
                        }
                    case NumericType.Float:
                        switch (b.identity.numericType)
                        {
                            case NumericType.Integer:
                                return a.value.floatValue >= b.value.integerValue;
                            case NumericType.Float:
                                return a.value.floatValue >= b.value.floatValue;
                            default:
                                return false;
                        }
                    default:
                        return false;
                }
            }
        }

        public static bool operator >=(StatValue a, float b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        throw new InvalidOperationException(strictTypeError);
                    case NumericType.Float:
                        return a.value.floatValue >= b;
                    default:
                        return false;
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue >= b;
                    case NumericType.Float:
                        return a.value.floatValue >= b;
                    default:
                        return false;
                }
            }
        }

        public static bool operator >=(StatValue a, int b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue >= b;
                    case NumericType.Float:
                        throw new InvalidOperationException(strictTypeError);
                    default:
                        return false;
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue >= b;
                    case NumericType.Float:
                        return a.value.floatValue >= b;
                    default:
                        return false;
                }
            }
        }

        public static bool operator <=(StatValue a, StatValue b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                if (!a.IsComparable(b))
                {
                    throw new InvalidOperationException(strictTypeError);
                }

                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue <= b.value.integerValue;
                    case NumericType.Float:
                        return a.value.floatValue <= b.value.floatValue;
                    default:
                        return false;
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        switch (b.identity.numericType)
                        {
                            case NumericType.Integer:
                                return a.value.integerValue <= b.value.integerValue;
                            case NumericType.Float:
                                return a.value.integerValue <= b.value.floatValue;
                            default:
                                return false;
                        }
                    case NumericType.Float:
                        switch (b.identity.numericType)
                        {
                            case NumericType.Integer:
                                return a.value.floatValue <= b.value.integerValue;
                            case NumericType.Float:
                                return a.value.floatValue <= b.value.floatValue;
                            default:
                                return false;
                        }
                    default:
                        return false;
                }
            }
        }

        public static bool operator <=(StatValue a, float b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        throw new InvalidOperationException(strictTypeError);
                    case NumericType.Float:
                        return a.value.floatValue <= b;
                    default:
                        return false;
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue <= b;
                    case NumericType.Float:
                        return a.value.floatValue <= b;
                    default:
                        return false;
                }
            }
        }

        public static bool operator <=(StatValue a, int b)
        {
            if (SkillTree.Settings.strictStatValueTypes)
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue <= b;
                    case NumericType.Float:
                        throw new InvalidOperationException(strictTypeError);
                    default:
                        return false;
                }
            }
            else
            {
                switch (a.identity.numericType)
                {
                    case NumericType.Integer:
                        return a.value.integerValue <= b;
                    case NumericType.Float:
                        return a.value.floatValue <= b;
                    default:
                        return false;
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is StatValue)
            {
                return this == (StatValue)obj;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        /// <summary>
        /// Checks if another stat can be compared with this stat and if any mathematical operations can be performed.
        /// </summary>
        /// <param name="other">The other stat.</param>
        /// <returns>Returns true if the other stat is comparable. Otherwise, false.</returns>
        public bool IsComparable(StatValue other)
        {
            return identity == other.identity;
        }

        /// <summary>
        /// Converts the value to a string.
        /// </summary>
        /// <returns>The value as a string.</returns>
        public override string ToString()
        {
            switch (identity.numericType)
            {
                case NumericType.Integer:
                    return value.integerValue.ToString();
                case NumericType.Float:
                    return value.floatValue.ToString("0.##");
                default:
                    return "-";
            }
        }

        /// <summary>
        /// Converts the value to a string with symbols.
        /// </summary>
        /// <returns>The value as a string with symbols.</returns>
        public string ToStringWithSymbols()
        {
            string s = string.Empty;

            switch (identity.numericType)
            {
                case NumericType.Integer:
                    s += value.integerValue.ToString();
                    break;
                case NumericType.Float:
                    s += value.floatValue.ToString("0.##");
                    break;
                default:
                    s += "-";
                    break;
            }

            if (identity.valueType == ValueType.Percent)
            {
                s += "%";
            }

            return s;
        }

        /// <summary>
        /// Converts the value to a float.
        /// </summary>
        /// <returns>The value as a float.</returns>
        public float ToFloat()
        {
            switch (identity.numericType)
            {
                case NumericType.Integer:
                    return value.integerValue;
                case NumericType.Float:
                    return value.floatValue;
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Converts the value to an int.
        /// </summary>
        /// <returns>The value as an int.</returns>
        public int ToInt()
        {
            switch (identity.numericType)
            {
                case NumericType.Integer:
                    return value.integerValue;
                case NumericType.Float:
                    return (int)value.floatValue;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Supported stat numeric types.
        /// </summary>
        public enum NumericType
        {
            /// <summary>
            /// Float value type.
            /// </summary>
            Float,

            /// <summary>
            /// Integer value type.
            /// </summary>
            Integer
        }

        /// <summary>
        /// Supported stat value types.
        /// </summary>
        public enum ValueType
        {
            /// <summary>
            /// The stat's value should be treated as an exact value.
            /// </summary>
            Value,

            /// <summary>
            /// The stat's value should be treated as a percentage.
            /// </summary>
            Percent
        }

        /// <summary>
        /// The stat value.
        /// </summary>
        [Serializable]
        public struct Value
        {
            public int integerValue;
            public float floatValue;
        }
    }
}