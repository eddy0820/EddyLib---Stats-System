using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;

namespace EddyLib.Stats
{

public abstract class Stats : MonoBehaviour
{
    [Expandable, SerializeField] protected BaseStatsSO baseStatsSO;

    protected Dictionary<string, Stat> getStatFromName = new();
    public Dictionary<string, Stat> GetStatFromName => getStatFromName;
    protected Dictionary<StatTypeSO, Stat> getStatFromType = new();
    public Dictionary<StatTypeSO, Stat> GetStatFromType => getStatFromType;
    protected Dictionary<string, StatTypeSO> getStatTypeFromName = new();
    public Dictionary<string, StatTypeSO> GetStatTypeFromName => getStatTypeFromName;

    public Action<Stat, StatModifier, EStatModifierChangedOperation> OnStatModifierChanged;

    protected void Awake()
    {
        InitializeCharacterStats();
    }

    protected virtual void InitializeCharacterStats()
    {
        foreach(BaseStatsSO.BaseStat baseStat in baseStatsSO.Stats)
        {
            Stat stat = new(baseStat.StatType, baseStat.Value);
            getStatFromName.Add(baseStat.StatType.Name, stat);
            getStatFromType.Add(baseStat.StatType, stat);
            getStatTypeFromName.Add(baseStat.StatType.Name, baseStat.StatType);
        }
    }

    public void ApplyStatModifier(StatModifier statModifier, string statTypeName) => ApplyStatModifier(getStatFromName[statTypeName], statModifier);
    public void ApplyStatModifier(StatModifier statModifier, StatTypeSO statType) => ApplyStatModifier(getStatFromType[statType], statModifier);
    private void ApplyStatModifier(Stat stat, StatModifier statModifier)
    {
        stat.AddModifier(statModifier);
        OnStatModifierChanged?.Invoke(stat, statModifier, EStatModifierChangedOperation.Added);
    }

    public void RemoveStatModifier(StatModifier statModifier, string statTypeName) => RemoveStatModifier(getStatFromName[statTypeName], statModifier);
    public void RemoveStatModifier(StatModifier statModifier, StatTypeSO statType) => RemoveStatModifier(getStatFromType[statType], statModifier);
    private void RemoveStatModifier(Stat stat, StatModifier statModifier)
    {
        if(stat.RemoveModifier(statModifier))
            OnStatModifierChanged?.Invoke(stat, statModifier, EStatModifierChangedOperation.Removed);
    }

    public void RemoveAllStatModifiersFromSource(object source) => getStatFromName.ToList().ForEach(stat => RemoveAllStatModifiersFromSource(stat.Value, source));
    public void RemoveAllStatModifiersFromSource(string statTypeName, object source) => RemoveAllStatModifiersFromSource(getStatFromName[statTypeName], source);
    public void RemoveAllStatModifiersFromSource(StatTypeSO statType, object source) => RemoveAllStatModifiersFromSource(getStatFromType[statType], source);
    private void RemoveAllStatModifiersFromSource(Stat stat, object source)
    {
        if(stat.RemoveAllModifiersFromSource(source, out List<StatModifier> removedModifiers))
        {
            foreach(StatModifier modifier in removedModifiers)
            {
                OnStatModifierChanged?.Invoke(stat, modifier, EStatModifierChangedOperation.RemovedAllFromSource);
            }
        }
    }

    public bool CharacterHasStats(params string[] statTypeNames)
    {
        foreach(string statTypeName in statTypeNames)
        {
            if(!getStatFromName.ContainsKey(statTypeName)) 
            {
                return false;
            }
        }

        return true;
    }

    public bool CharacterHasStats(params StatTypeSO[] statTypes)
    {
        foreach(StatTypeSO statType in statTypes)
        {
            if(!getStatFromType.ContainsKey(statType)) 
            {
                return false;
            }
        }

        return true;
    }
}

}
