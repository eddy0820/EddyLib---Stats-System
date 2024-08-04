using System;
using System.Collections.Generic;
using UnityEngine;

namespace EddyLib.Stats
{

[CreateAssetMenu(fileName = "New Base Stats", menuName = "EddyLib/Stats/Base Stats")]
public class BaseStatsSO : ScriptableObject
{
    [SerializeField] List<BaseStat> stats = new List<BaseStat>();
    public List<BaseStat> Stats => stats;

    [Serializable]
    public class BaseStat
    {
        [SerializeField] StatTypeSO statType = null;
        public StatTypeSO StatType => statType;

        [SerializeField] float value;
        public float Value => value;
    }
}

}

