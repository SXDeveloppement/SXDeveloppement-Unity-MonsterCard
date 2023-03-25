public enum CardStatus {
    Unreal, Hand, Graveyard, SlotVisible, SlotHidden, AuraSlot, EnchantmentSlot, ActionSlot
}

public enum ElementalAffinity {
    Fire, Water, Electric, Earth, Combat, Mental, Neutral
}

public enum CardType {
    Spell, Enchantment, Aura, CounterAttack, Echo, Sbire
}

public enum IsFrom {
    Monster, Equipment1, Equipment2, Equipment3, Equipment4
}

public enum TargetType {
    PlayerMonster, OpponantMonster, PlayerEquipment, PlayerCardEnchantment, OpponantCardEnchantment, PlayerAura, PlayerCardAura, OpponantCardAura, SlotVisible, SlotHidden, PlayerCardSbire, OpponantCardSbire, OpponantEquipment, Null
}

public enum BuffDebuffType {
    Power, Guard, Speed, Mana, DamageRaw, DamagePercent
}

/// <summary>
/// Global = bonus toujours actif / Trigger = qui se d�clenche lors d'une action sp�cifique / Active = que le joueur doit activer
/// </summary>
public enum AbilityType {
    Global, Trigger, Active
}

public enum AbilityStatus {
    Board, Action, TeamLayout
}

/// <summary>
/// quickness = c�l�rit� / pierce = pi�tinement / Domination = Initiative / Tank = provocation de HS
/// </summary>
public enum SbirePassifEffect {
    Quickness, Pierce, Domination, Tank
}