using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace EddyLib.Stats
{

[Serializable]
public class Stat
{
    public readonly float baseValue;
    public StatTypeSO type;
    public virtual float Value
    {
        get
        {
            if (isDirty || baseValue != lastBaseValue)
            {
                lastBaseValue = baseValue;
                _value = CalculateFinalValue();
                isDirty = false;

                _value = Mathf.Clamp(_value, minValue, maxValue);
            }
            
            return _value;
        }
    }
    private float minValue;
    private float maxValue;
    // To mark the fact a modifier was added or removed so that CalaculateFinalValue()
    // isn't called every time you get the value whether there was a modifier change or not.
    protected bool isDirty = true;
    protected float _value;
    protected float lastBaseValue = float.MinValue;
    // The readonly keyword means we can't alter that variable except when we
    // are in the constructor of the class or in the declaration itself. 
    protected readonly List<StatModifier> statModifiers;
    // Public version of the variable above.
    public readonly ReadOnlyCollection<StatModifier> StatModifiers;

    // Constructors split like this becuase of a null refernce exception?
    public Stat()
    {
        statModifiers = new List<StatModifier>();
        StatModifiers = statModifiers.AsReadOnly();
    }
    // Constructor to create a stat with no ScriptableObject, just a float.
    public Stat(float _baseValue) : this()
    {
        baseValue = _baseValue;
        type = null;
        minValue = Mathf.NegativeInfinity;
        maxValue = Mathf.Infinity;

    }
    // Constructor to create a stat with a StatType ScriptableObject, using the values from the StatType.
    public Stat(StatTypeSO _type) : this()
    {
        baseValue = _type.DefaultValue;
        type = _type;
        minValue = _type.DefaultMinValue;
        maxValue = _type.DefaultMaxValue;
    }
    // Constructor to create a stat with a BaseStats ScriptableObject, using the value and StatType from that.
    public Stat(StatTypeSO _type, float _baseValue) : this()
    {
        baseValue = _baseValue;
        type = _type;
        minValue = _type.DefaultMinValue;
        maxValue = _type.DefaultMaxValue;
    }

    public virtual void AddModifier(StatModifier modifier)
    {
        isDirty = true;
        statModifiers.Add(modifier);
        statModifiers.Sort(CompareModifierOrder);
    }

    // Used for sorting modifier types by order.
    protected virtual int CompareModifierOrder(StatModifier modifierA, StatModifier modifierB)
    {
        if(modifierA.order < modifierB.order)
        {
            return -1;
        }
        else if(modifierA.order > modifierB.order)
        {
            return 1;
        }

        // If (modifierA.order == modifierB.order).
        return 0;
    }

    public virtual bool RemoveModifier(StatModifier modifier)
    {
        if(statModifiers.Remove(modifier))
        {
            isDirty = true;
            return true;
        }

        return false;
    }

    // Loops through all the statModifiers and removes the ones from
    // the specified source.
    public virtual bool RemoveAllModifiersFromSource(object source, out List<StatModifier> removedModifiers)
    {
        bool didRemove = false;
        removedModifiers = new();

        // Traverses the list in reverse order because its more efficient.
        for(int i = statModifiers.Count - 1; i >= 0; i--)
        {
            if(statModifiers[i].source == source)
            {
                isDirty = true;
                didRemove = true;
                removedModifiers.Add(statModifiers[i]);
                statModifiers.RemoveAt(i);
            }
        }

        return didRemove;
    }

    // Loops through the modifiers and calculates the final value in the correct order.
    protected virtual float CalculateFinalValue()
    {
        float finalValue = baseValue;
        float sumPercentAdd = 0;

        for (int i = 0; i < statModifiers.Count; i++)
        {
            StatModifier modifier = statModifiers[i];

            // Determines the modifier type and does calculations according to those.
            if(modifier.type == StatModifierTypes.Flat)
            {
                finalValue += statModifiers[i].value;
            }
            else if(modifier.type == StatModifierTypes.PercentAdd)
            {
                sumPercentAdd += modifier.value;

                // Iterates and add all modifiers of the same type until we reach a 
                // modifier of a different type or until we reach the end of the list.
                if( i + 1 >= statModifiers.Count || statModifiers[i + 1].type != StatModifierTypes.PercentAdd)
                {
                    if((baseValue == 0 && maxValue == 100))
                    {
                        finalValue = 100 * sumPercentAdd;
                    }
                    else
                    {
                        finalValue *= 1 + sumPercentAdd;
                    }
                    
                    sumPercentAdd = 0;
                }            
            }
            else if(modifier.type == StatModifierTypes.PercentMult)
            {
                if((baseValue == 0 && maxValue == 100))
                {
                    finalValue = 100 * modifier.value;
                }
                else
                {
                    finalValue *= 1 + modifier.value;
                }

                
            }
        }

        return (float) Math.Round(finalValue, 4);
    }
}

}
