using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "DataContainer/Character")]
public class CharacterData : ScriptableObject
{
    public int dataID;
    public string characterName;
    public string characterDescription;
    public int maxHealthPoints;
    public int maxAbilityPoints;
}
