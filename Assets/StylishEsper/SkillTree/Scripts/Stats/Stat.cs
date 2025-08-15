//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Newtonsoft.Json;
using System;

namespace Esper.SkillTree.Stats
{
    /// <summary>
    /// Represents a stat that can be managed.
    /// </summary>
    [Serializable]
    public class Stat
    {
        /// <summary>
        /// The starting value of the stat.
        /// </summary>
        public StatValue initialValue;

        /// <summary>
        /// The current value of the stat.
        /// </summary>
        public StatValue currentValue;

        /// <summary>
        /// The value added to the stat from external sources.
        /// </summary>
        public StatValue externalValue;

        /// <summary>
        /// The amount this stat will scale each level.
        /// </summary>
        public StatValue scaling;

        /// <summary>
        /// The last calculated excess value.
        /// </summary>
        private StatValue excessValue;

        /// <summary>
        /// The current level of this stat.
        /// </summary>
        public int currentLevel = 1;

        /// <summary>
        /// The max level of this stat.
        /// </summary>
        public int maxLevel = 1;

        /// <summary>
        /// The combine type used for mathematical operations.
        /// </summary>
        public CombineType combineType;

        /// <summary>
        /// The combine operator used for mathematical operations.
        /// </summary>
        public CombineOperator combineOperator;

        /// <summary>
        /// The ID of the stat identity.
        /// </summary>
        public int statID;

        /// <summary>
        /// The stat identity. Use Identity or SetIdentity instead.
        /// </summary>
        [JsonIgnore]
        public StatIdentity identity;

        /// <summary>
        /// The base value of the stat at the current level.
        /// </summary>
        [JsonIgnore]
        public StatValue BaseValue { get => initialValue + (scaling * (currentLevel - 1)); }

        /// <summary>
        /// The max value of the stat.
        /// </summary>
        [JsonIgnore]
        public StatValue MaxValue { get => BaseValue + externalValue; }

        /// <summary>
        /// The value of the stat if leveled up.
        /// </summary>
        [JsonIgnore]
        public StatValue NextBaseValue { get => initialValue + scaling * currentLevel; }

        /// <summary>
        /// The max value of the stat if it reaches the max level.
        /// </summary>
        [JsonIgnore]
        public StatValue MaxBaseValue { get => initialValue + scaling * maxLevel; }

        /// <summary>
        /// If this stat is maxed.
        /// </summary>
        [JsonIgnore]
        public bool IsMaxed { get => currentLevel == maxLevel; }

        /// <summary>
        /// The stat identity.
        /// </summary>
        [JsonIgnore]
        public StatIdentity Identity 
        {
            get
            {
                if (!identity)
                {
                    identity = SkillTree.GetStatIdentity(statID);
                }

                return identity;
            }
            set
            {
                SetIdentity(value);
            }
        }

        /// <summary>
        /// The stats provider reference.
        /// </summary>
        [NonSerialized, JsonIgnore]
        public UnitStatsProvider statsProviderReference;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Stat()
        {
            
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="identity">The stat identity.</param>
        public Stat(StatIdentity identity)
        {
            statID = identity.id;
            this.identity = identity;
            initialValue = new StatValue(identity);
            currentValue = new StatValue(identity);
            scaling = new StatValue(identity);
            externalValue = new StatValue(identity);
            excessValue = new StatValue(identity);
            currentLevel = 0;
            maxLevel = 0;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="identity">The stat identity.</param>
        /// <param name="initialValue">The starting value.</param>
        /// <param name="currentValue">The current value.</param>
        /// <param name="scaling">The amount this stat will scale each level.</param>
        /// <param name="maxLevel">The max level of the stat.</param>
        public Stat(StatIdentity identity, StatValue initialValue, StatValue currentValue, StatValue scaling = default, int maxLevel = 0)
        {
            statID = identity.id;
            this.identity = identity;
            initialValue.identity = identity;
            currentValue.identity = identity;
            scaling.identity = identity;
            this.initialValue = initialValue;
            this.currentValue = currentValue;
            this.scaling = scaling;
            externalValue = new StatValue(identity);
            excessValue = new StatValue(identity);
            currentLevel = 0;
            this.maxLevel = maxLevel;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="identity">The stat identity.</param>
        /// <param name="initialValue">The starting value.</param>
        /// <param name="currentValue">The current value.</param>
        /// <param name="scaling">The amount this stat will scale each level.</param>
        /// <param name="maxLevel">The max level of the stat.</param>
        public Stat(StatIdentity identity, int initialValue, int currentValue, int scaling = 0, int maxLevel = 0)
        {
            statID = identity.id;
            this.identity = identity;
            this.initialValue = new StatValue(identity, initialValue);
            this.currentValue = new StatValue(identity, currentValue);
            this.scaling = new StatValue(identity, scaling);
            externalValue = new StatValue(identity);
            excessValue = new StatValue(identity);
            currentLevel = 0;
            this.maxLevel = maxLevel;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="identity">The stat identity.</param>
        /// <param name="initialValue">The starting value.</param>
        /// <param name="currentValue">The current value.</param>
        /// <param name="scaling">The amount this stat will scale each level.</param>
        /// <param name="maxLevel">The max level of the stat.</param>
        public Stat(StatIdentity identity, float initialValue, float currentValue, float scaling = 0, int maxLevel = 0)
        {
            statID = identity.id;
            this.identity = identity;
            this.initialValue = new StatValue(identity, initialValue);
            this.currentValue = new StatValue(identity, currentValue);
            this.scaling = new StatValue(identity, scaling);
            externalValue = new StatValue(identity);
            excessValue = new StatValue(identity);
            currentLevel = 0;
            this.maxLevel = maxLevel;
        }

        /// <summary>
        /// Sets the stat identity for each value.
        /// </summary>
        /// <param name="identity">The stat identity.</param>
        public void SetIdentity(StatIdentity identity)
        {
            statID = identity.id;
            this.identity = identity;
            initialValue.identity = identity;
            currentValue.identity = identity;
            scaling.identity = identity;
            externalValue.identity = identity;
            excessValue.identity = identity;
        }

        /// <summary>
        /// Checks if this stat's name or abbreviation match the provided string.
        /// </summary>
        /// <param name="name">The stat's name or abbreviation.</param>
        /// <returns>True if the provided name matches the stat's name or abbreviation.</returns>
        public bool NameMatches(string name)
        {
            return Identity.name == name || Identity.abbreviation == name;
        }

        public static Stat operator +(Stat a, Stat b)
        {
            switch (b.combineType)
            {
                case CombineType.Value:
                    if (b.combineOperator == CombineOperator.Add)
                    {
                        a.externalValue += b.currentValue;
                        a.currentValue += b.currentValue;
                    }
                    else
                    {
                        a.externalValue -= b.currentValue;
                        a.currentValue -= b.currentValue;           
                    }
                    break;

                case CombineType.PercentMax:
                    var value = a.BaseValue * (b.currentValue * 0.01f);

                    if (b.combineOperator == CombineOperator.Add)
                    {
                        a.externalValue += value;
                        a.currentValue += value;       
                    }
                    else
                    {
                        a.externalValue -= value;
                        a.currentValue -= value;
                    }
                    break;

                case CombineType.PercentCurrent:
                    value = a.currentValue * (b.currentValue * 0.01f);

                    if (b.combineOperator == CombineOperator.Add)
                    {
                        a.externalValue += value;
                        a.currentValue += value;
                    }
                    else
                    {
                        a.externalValue -= value;
                        a.currentValue -= value;
                    }
                    break;
            }

            a.StatChanged();
            return a;
        }

        public static Stat operator +(Stat a, StatValue b)
        {
            a.currentValue += b;
            a.StatChanged();
            return a;
        }

        public static Stat operator +(Stat a, int b)
        {
            a.currentValue += b;
            a.StatChanged();
            return a;
        }

        public static Stat operator +(Stat a, float b)
        {
            a.currentValue += b;
            a.StatChanged();
            return a;
        }

        public static Stat operator -(Stat a, Stat b)
        {
            switch (b.combineType)
            {
                case CombineType.Value:
                    if (b.combineOperator == CombineOperator.Add)
                    {
                        a.externalValue -= b.currentValue;
                        a.currentValue -= b.currentValue;
                    }
                    else
                    {
                        a.externalValue += b.currentValue;
                        a.currentValue += b.currentValue;
                    }
                    break;

                case CombineType.PercentMax:
                    var value = a.BaseValue * (b.currentValue * 0.01f);

                    if (b.combineOperator == CombineOperator.Add)
                    {
                        a.externalValue -= value;
                        a.currentValue -= value;
                    }
                    else
                    {
                        a.externalValue += value;
                        a.currentValue += value;
                    }
                    break;

                case CombineType.PercentCurrent:
                    value = a.currentValue * (b.currentValue * 0.01f);

                    if (b.combineOperator == CombineOperator.Add)
                    {
                        a.externalValue -= value;
                        a.currentValue -= value;
                    }
                    else
                    {
                        a.externalValue += value;
                        a.currentValue += value;
                    }
                    break;
            }

            a.StatChanged();
            return a;
        }

        public static Stat operator -(Stat a, StatValue b)
        {
            a.currentValue -= b;
            a.StatChanged();
            return a;
        }

        public static Stat operator -(Stat a, int b)
        {
            a.currentValue -= b;
            a.StatChanged();
            return a;
        }

        public static Stat operator -(Stat a, float b)
        {
            a.currentValue -= b;
            a.StatChanged();
            return a;
        }

        public static Stat operator *(Stat a, StatValue b)
        {
            a.currentValue *= b;
            a.StatChanged();
            return a;
        }

        public static Stat operator *(Stat a, int b)
        {
            a.currentValue *= b;
            a.StatChanged();
            return a;
        }

        public static Stat operator *(Stat a, float b)
        {
            a.currentValue *= b;
            a.StatChanged();
            return a;
        }

        public static Stat operator /(Stat a, StatValue b)
        {
            a.currentValue /= b;
            a.StatChanged();
            return a;
        }

        public static Stat operator /(Stat a, int b)
        {
            a.currentValue /= b;
            a.StatChanged();
            return a;
        }

        public static Stat operator /(Stat a, float b)
        {
            a.currentValue /= b; 
            a.StatChanged();
            return a;
        }

        public static bool operator ==(Stat a, Stat b)
        {
            if (a is null && b is null)
            {
                return true;
            }
            else if (a is null && b is not null)
            {
                return false;
            }
            else if (a is not null && b is null)
            {
                return false;
            }

            return a.currentValue == b.currentValue;
        }

        public static bool operator !=(Stat a, Stat b)
        {
            if (a is null && b is null)
            {
                return false;
            }
            else if (a is null && b is not null)
            {
                return true;
            }
            else if (a is not null && b is null)
            {
                return true;
            }

            return a.currentValue != b.currentValue;
        }

        public override bool Equals(object obj)
        {
            if (obj is Stat)
            {
                return this == (Stat)obj;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return currentValue.GetHashCode();
        }

        /// <summary>
        /// Gets the last calculated excess value. This shoud be used right after a mathematical operation
        /// that may lead to an excess value. The excess value will be reset when this is called.
        /// </summary>
        /// <returns>The cached excess value.</returns>
        public StatValue GetExcessValue()
        {
            var excess = excessValue;
            excessValue = default;
            return excess;
        }

        /// <summary>
        /// Clamps the current value within the min and max ranges.
        /// </summary>
        private void Clamp()
        {
            if (currentValue > MaxValue)
            {
                excessValue = currentValue - MaxValue;
                currentValue = MaxValue;
            }
            else if (Identity.hasMax && currentValue > Identity.MaxValue)
            {
                excessValue = currentValue - Identity.MaxValue;
                currentValue = Identity.MaxValue;
            }
            else if (Identity.hasMin && currentValue < Identity.MinValue)
            {
                excessValue = currentValue - Identity.MinValue;
                currentValue = Identity.MinValue;
            }
            else
            {
                excessValue = new StatValue(identity, 0);
            }     
        }

        /// <summary>
        /// Invokes stat changed callbacks.
        /// </summary>
        private void StatChanged()
        {
            Clamp();

            if (statsProviderReference)
            {
                statsProviderReference.onStatChanged.Invoke(this);

                if (currentValue >= MaxValue)
                {
                    statsProviderReference.onStatReachedMax.Invoke(this);
                }
                else if (Identity.hasMin && currentValue <= Identity.MinValue)
                {
                    statsProviderReference.onStatReachedMin.Invoke(this);
                }
                else if (currentValue == MaxValue || (Identity.hasMax && currentValue >= Identity.MaxValue))
                {
                    statsProviderReference.onStatReachedMax.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Sets the stat level.
        /// </summary>
        /// <param name="level">The level to set to.</param>
        public void SetLevel(int level)
        {
            if (level < 0 || level > maxLevel)
            {
                return;
            }

            currentLevel = level;

            if (statsProviderReference)
            {
                statsProviderReference.onStatLevelChanged.Invoke(this);
            }
        }

        /// <summary>
        /// Increases the stat level by an amount if possible.
        /// </summary>
        /// <param name="amount">The amount to increase by. Default: 1.</param>
        /// <returns>The excess amount of stat points.</returns>
        public int TryUpgrade(int amount = 1)
        {
            if (IsMaxed)
            {
                return amount;
            }

            int excess = 0;

            currentLevel += amount;

            if (currentLevel > maxLevel)
            {
                excess = amount - (amount - (currentLevel - maxLevel));
                currentLevel = maxLevel;
            }

            if (statsProviderReference)
            {
                statsProviderReference.onStatLevelChanged.Invoke(this);
            }

            return excess;
        }

        /// <summary>
        /// Decreases the stat level by an amount if possible.
        /// </summary>
        /// <param name="amount">The amount to decrease by. Default: 1.</param>
        /// <returns>The excess amount of stat points.</returns>
        public int TryDowngrade(int amount = 1)
        {
            if (currentLevel == 0)
            {
                return amount;
            }

            int excess = 0;

            if (amount > currentLevel)
            {
                excess = amount - currentLevel;
                currentLevel = 0;
            }
            else
            {
                currentLevel -= amount;
            }

            if (statsProviderReference)
            {
                statsProviderReference.onStatLevelChanged.Invoke(this);
            }

            return excess;
        }

        /// <summary>
        /// Sets the current level to 0.
        /// </summary>
        public void Deplete()
        {
            TryDowngrade(currentLevel);
        }

        /// <summary>
        /// Resets the current and external values.
        /// </summary>
        public void ResetValues()
        {
            currentValue = BaseValue;
            externalValue = new StatValue(identity);
        }

        /// <summary>
        /// Resets the stat.
        /// </summary>
        public void Reset()
        {
            currentLevel = 1;
            SetIdentity(identity);
            ResetValues();
            Deplete();
        }

        /// <summary>
        /// Converts the stat to a string.
        /// </summary>
        /// <returns>The stat as a string.</returns>
        public override string ToString()
        {
            string s = string.Empty;

            if (combineType == CombineType.Value)
            {
                if (combineOperator == CombineOperator.Add)
                {
                    s = currentValue.ToString();
                }
                else
                {
                    s = $"-{currentValue}";
                }
            }
            else
            {
                switch (combineOperator)
                {
                    case CombineOperator.Add:
                        s += "+";
                        break;
                    case CombineOperator.Subtract:
                        s += "-";
                        break;
                    default:
                        break;
                }

                s += $"{currentValue}%";
            }

            return s;
        }

        /// <summary>
        /// Converts the stat to a float.
        /// </summary>
        /// <returns>The stat as a float.</returns>
        public float ToFloat()
        {
            return currentValue.ToFloat();
        }

        /// <summary>
        /// Converts the stat to an int.
        /// </summary>
        /// <returns>The stat as an int.</returns>
        public int ToInt()
        {
            return currentValue.ToInt();
        }

        /// <summary>
        /// Supported stat combine types.
        /// </summary>
        public enum CombineType
        {
            /// <summary>
            /// When combined with another stat, the stat's value will be treated as is.
            /// </summary>
            Value,

            /// <summary>
            /// When combined with another stat, the stat's value will be treated as a percentage of the other
            /// stat's max value.
            /// </summary>
            PercentMax,

            /// <summary>
            /// When combined with another stat, the stat's value will be treated as a percentage of the other
            /// stat's current value.
            /// </summary>
            PercentCurrent
        }

        /// <summary>
        /// Supported stat combine operators.
        /// </summary>
        public enum CombineOperator
        {
            /// <summary>
            /// Add the value to the other stat.
            /// </summary>
            Add,

            /// <summary>
            /// Subtract the value from the other stat.
            /// </summary>
            Subtract
        }
    }
}
