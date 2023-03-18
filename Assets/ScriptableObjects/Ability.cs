using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "ScriptableObjects/Ability")]
public class Ability : ScriptableObject
{
    public new string name;
    [TextArea(3, 4)]
    public string description;
    public Sprite illustration;
    public AbilityType abilityType;
    public int manaCost;
    public int activationPerTurn;
    public int cooldown;
    public ElementalAffinity elementalAffinity;
    public TargetType[] targetType;

    public CardEffect[] effects;

    public void activeEffect(GameObject target) {
        foreach (var effect in effects) {
            effect.ExecuteEffect(target, elementalAffinity);
        }
    }

    public void disableEffect() {
        foreach (var effect in effects) {
            effect.DisableEffect();
        }
    }
}