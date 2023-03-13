using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public Card card;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text manaText;
    public TMP_Text priorityText;
    public TMP_Text typeText;
    public Image artworkImage;
    public SpriteRenderer illustration;
    public GameObject folderBackgrounds;

    public Status status;
    public bool hiddenCard;
    public bool putOnBoardThisTurn = true;
    public bool putOnBoardThisTurnTemp = true;

    public GameObject monsterOwnThis; // GO du monstre qui poss�de cette carte
    public bool ownedByOppo;

    private bool init = true;

    // Variable pour les animations
    //public AnimationCurve scaleCurve;
    //public float duration = 0.5f;

    private string cardDescriptionCached;

    GameManager gameManager;

    // Start is called before the first frame update
    void Start() {
        gameManager = GameObject.FindAnyObjectByType<GameManager>();

        // On affiche le bon background en fonction du type �l�mentaire de la carte
        foreach (Transform childBackground in folderBackgrounds.transform) {
            childBackground.gameObject.SetActive(false);
            if (childBackground.gameObject.name == card.elementalAffinity.ToString()) {
                childBackground.gameObject.SetActive(true);
            }
        }

        nameText.text = card.name;
        descriptionText.text = card.description;
        manaText.text = card.manaCost.ToString();
        priorityText.text = card.priority.ToString();
        typeText.text = card.type.ToString();
        if (artworkImage != null)
            artworkImage.sprite = card.artwork;
        if (illustration != null)
            illustration.sprite = card.artwork;

        cardDescriptionCached = card.description;

        //refreshDescriptionDamage();
    }

    // Update is called once per frame
    void Update() {
        if (init) {
            init = false;
            refreshDescriptionDamage();
        }

        // On affiche l'aura d'activation de la carte ECHO
        if (putOnBoardThisTurn != putOnBoardThisTurnTemp && card.type == Type.Echo) {
            putOnBoardThisTurnTemp = putOnBoardThisTurn;
            GetComponent<SbireDisplay>().attackAura.SetActive(!putOnBoardThisTurn);
        }
    }

    // Action lors d'une activation qui cible cette carte
    public bool onDrop(GameObject cardPlayed) {
        bool isPutOnBoard = false;

        if (GameManager.dragged) {
            GameObject target = gameObject;

            // Ciblage d'une aura
            if (card.type == Type.Aura
            && status == Status.AuraSlot
            && (
                cardPlayed.GetComponent<CardDisplay>().status == Status.Hand
                || cardPlayed.GetComponent<CardDisplay>().status == Status.SlotHidden
                || cardPlayed.GetComponent<CardDisplay>().status == Status.SlotVisible // Carte echo
            )) {
                // On v�rifie les conditions de ciblage pour pouvoir activer la carte
                bool targetCondition = false;
                TargetType[] cardPlayedTargetType = cardPlayed.GetComponent<CardDisplay>().card.targetType;
                bool ownedByOppo = monsterOwnThis.GetComponent<MonsterDisplay>().ownedByOppo;
                foreach (TargetType cardTargetType in cardPlayedTargetType) {
                    if (cardTargetType == TargetType.OpponantCardAura && ownedByOppo
                        || cardTargetType == TargetType.PlayerCardAura && !ownedByOppo
                        ) {
                        targetCondition = true;
                        break;
                    }
                }
                // On active la carte si les conditions de ciblages sont respect�es
                if (targetCondition) {
                    gameManager.activeCardOnTarget(cardPlayed, target);
                } else {
                    Debug.Log("ERR : bad target [" + target.name + "] / ownByOppo = " + ownedByOppo.ToString());
                }
            }
            // Ciblage d'un enchantement
            else if (card.type == Type.Enchantment
            && status == Status.EnchantmentSlot
            && (
                cardPlayed.GetComponent<CardDisplay>().status == Status.Hand
                || cardPlayed.GetComponent<CardDisplay>().status == Status.SlotHidden
                || cardPlayed.GetComponent<CardDisplay>().status == Status.SlotVisible // Carte echo
            )) {
                // On v�rifie les conditions de ciblage pour pouvoir activer la carte
                bool targetCondition = false;
                TargetType[] cardPlayedTargetType = cardPlayed.GetComponent<CardDisplay>().card.targetType;
                bool ownedByOppo = monsterOwnThis.GetComponent<MonsterDisplay>().ownedByOppo;
                foreach (TargetType cardTargetType in cardPlayedTargetType) {
                    if (cardTargetType == TargetType.OpponantCardEnchantment && ownedByOppo
                        || cardTargetType == TargetType.PlayerCardEnchantment && !ownedByOppo
                        ) {
                        targetCondition = true;
                        break;
                    }
                }
                // On active la carte si les conditions de ciblages sont respect�es
                if (targetCondition) {
                    gameManager.activeCardOnTarget(cardPlayed, target);
                } else {
                    Debug.Log("ERR : bad target [" + target.name + "] / ownByOppo = " + ownedByOppo.ToString());
                }
            }
            // Ciblage d'un sbire par un spell
            else if (card.type == Type.Sbire
            && status == Status.SlotVisible
            && gameObject.GetComponent<SbireDisplay>().sbireHealthAvailable > 0
            && cardPlayed.GetComponent<CardDisplay>().card.type != Type.Sbire
            && (
                cardPlayed.GetComponent<CardDisplay>().status == Status.Hand
                || cardPlayed.GetComponent<CardDisplay>().status == Status.SlotHidden
                || cardPlayed.GetComponent<CardDisplay>().status == Status.SlotVisible // Carte echo
            )) {
                bool targetCondition = false;
                bool ownedByOppo = GetComponent<CardDisplay>().monsterOwnThis.GetComponent<MonsterDisplay>().ownedByOppo;
                TargetType[] cardPlayedTargetType = cardPlayed.GetComponent<CardDisplay>().card.targetType;
                foreach (TargetType cardTargetType in cardPlayedTargetType) {
                    if (cardTargetType == TargetType.PlayerCardSbire && !ownedByOppo
                        || cardTargetType == TargetType.OpponantCardSbire && ownedByOppo) {
                        targetCondition = true;
                        break;
                    }
                }
                // On active la carte si les conditions de ciblages sont respect�es
                if (targetCondition) {
                    gameManager.activeCardOnTarget(cardPlayed, target);
                } else {
                    Debug.Log("ERR : bad target [" + target.name + "] / ownByOppo = " + ownedByOppo.ToString());
                }
                
            }
            // Ciblage d'un sbire par un autre sbire
            else if (card.type == Type.Sbire
            && status == Status.SlotVisible
            && gameObject.GetComponent<SbireDisplay>().sbireHealthAvailable > 0
            && cardPlayed.GetComponent<CardDisplay>().card.type == Type.Sbire
            && cardPlayed.GetComponent<CardDisplay>().status == Status.SlotVisible
            && cardPlayed.GetComponent<SbireDisplay>().sbireHealthAvailable > 0
            && (
                !ownedByOppo == cardPlayed.GetComponent<CardDisplay>().ownedByOppo
                || ownedByOppo == !cardPlayed.GetComponent<CardDisplay>().ownedByOppo
            )) {
                isPutOnBoard = true;
                bool sbireHaveTaunt = false;
                foreach (CardDisplay cardDisplay in gameManager.GO_CounterAttackAreaOppo.GetComponentsInChildren<CardDisplay>()) {
                    if (cardDisplay.card.type == Type.Sbire) {
                        foreach (SbirePassifEffect sbirePassifEffect in cardDisplay.card.sbirePassifEffects) {
                            if (sbirePassifEffect == SbirePassifEffect.Tank) {
                                sbireHaveTaunt = true;
                                break;
                            }
                        }

                        if (sbireHaveTaunt) break;
                    }
                }

                bool haveTaunt = false;
                foreach (SbirePassifEffect sbirePassifEffect in card.sbirePassifEffects) {
                    if (sbirePassifEffect == SbirePassifEffect.Tank) {
                        haveTaunt = true;
                        break;
                    }
                }

                if (!sbireHaveTaunt || haveTaunt) {
                    SbireDisplay sbireDisplay = cardPlayed.GetComponent<SbireDisplay>();
                    SbireDisplay targetSbireDisplay = target.GetComponent<SbireDisplay>();
                    StartCoroutine(sbireDisplay.fightVersus(targetSbireDisplay));
                } else {
                    Debug.Log("ERR : Bad target, one sbire or more have Taunt");
                }
            }
        }

        return isPutOnBoard;
    }

    // On active les effets de la carte
    public void activeCard(GameObject target) {
        card.activeEffect(target);
        gameObject.GetComponent<ZoomCard2D>().destroyPlaceholder();

        // Si c'est une carte de Type.Spell ou Type.Echo
        if (card.type == Type.Spell || card.type == Type.Echo) {
            Debug.Log("In Grave");
            gameManager.inGrave(gameObject);
        }
    }

    // On desactive les effets de la carte (aura, enchantement)
    public void disableCard() {
        card.disableEffect();
        gameManager.inGrave(gameObject);
    }

    // On place la carte face visible
    public void putOnBoard(GameObject target, bool isVisible) {
        if (isVisible) {
            showVisibleFace();
            hiddenCard = false;
        } else {
            showHiddenFace();
            hiddenCard = true;
        }

        if (target.GetComponent<SlotDisplay>() != null) {
            if (hiddenCard) {
                status = Status.SlotHidden;
            } else {
                status = Status.SlotVisible;
            }
            transform.SetParent(target.GetComponent<SlotDisplay>().slotCard.transform);
            transform.localPosition = Vector3.zero;
            target.GetComponent<SlotDisplay>().cardOnSlot = this.gameObject;
        } else if (target.GetComponent<AuraDisplay>() != null) {
            status = Status.AuraSlot;
            transform.SetParent(target.GetComponent<AuraDisplay>().slotCard.transform);
            transform.localPosition = Vector3.zero;
            target.GetComponent<AuraDisplay>().cardOnSlot = this.gameObject;
        } else if (target.GetComponent<EquipmentDisplay>() != null) {
            status = Status.EnchantmentSlot;
            transform.SetParent(target.transform.parent.parent.GetChild(1));
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
        }

        // Si c'est un sbire
        if (card.type == Type.Sbire) {
            GetComponent<SbireDisplay>().invokeSbire();
        }
        
        GetComponent<ZoomCard2D>().reinitCard();
    }

    // Renvoi les d�gats de base de l'attaque
    public List<int> getBaseDamage() {
        string pattern = @"\%D(\d+)";
        List<int> intList = new List<int>();

        MatchCollection m = Regex.Matches(cardDescriptionCached, pattern, RegexOptions.IgnoreCase);
        foreach (Match m2 in m) {
            intList.Add(int.Parse(m2.Groups[1].Value));
        }

        return intList;
    }

    // On met a jour la description de la carte avec les d�g�ts qui seront r�ellement inflig�s au monstre adverse
    public void refreshDescriptionDamage() {
        // Si c'est une carte sbire
        if (card.type == Type.Sbire) {
            int sbireBasePower = card.sbirePowerPoint;
            int outputSbireDamage;
            if (!ownedByOppo) {
                outputSbireDamage = gameManager.calculateDamage(gameManager.GO_MonsterInvokedOppo, card.elementalAffinity, sbireBasePower);
            } else {
                outputSbireDamage = gameManager.calculateDamage(gameManager.GO_MonsterInvoked, card.elementalAffinity, sbireBasePower);
            }
            GetComponent<SbireDisplay>().sbirePowerAvailable = outputSbireDamage;
        }
        // Si c'est un sort
        else {
            string pattern = @"\%D\d+";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            string output = null;
            foreach (int baseDamage in getBaseDamage()) {
                int trueDamage;
                if (!ownedByOppo) {
                    trueDamage = gameManager.calculateDamage(gameManager.GO_MonsterInvokedOppo, card.elementalAffinity, baseDamage);
                } else {
                    trueDamage = gameManager.calculateDamage(gameManager.GO_MonsterInvoked, card.elementalAffinity, baseDamage);
                }
                if (output != null) {
                    output = regex.Replace(output, trueDamage.ToString(), 1);
                } else {
                    output = regex.Replace(cardDescriptionCached, trueDamage.ToString(), 1);
                }
            }
            if (output != null) {
                descriptionText.text = output;
            }
        }
    }

    // On retourne la carte face cach�
    public void showHiddenFace() {
        gameObject.transform.Find("Front").gameObject.SetActive(false);
        gameObject.transform.Find("Back").gameObject.SetActive(true);
    }

    // On retourne la carte face visible
    public void showVisibleFace() {
        gameObject.transform.Find("Front").gameObject.SetActive(true);
        gameObject.transform.Find("Back").gameObject.SetActive(false);
    }

    // Animation de retournement de carte face visibile
    //public IEnumerator flipFront() {
    //    float time = 0f;
    //    bool endLoop = false;
    //    while (!endLoop) {
    //        float scale;
    //        if (time >= 1f) {
    //            endLoop = true;
    //            scale = scaleCurve.Evaluate(1f);
    //        } else {
    //            scale = scaleCurve.Evaluate(time);
    //        }
            
    //        if (time < 0.5f) {
    //            Transform cardBack = transform.Find("Back");
    //            cardBack.localScale = new Vector3(scale, cardBack.localScale.y, cardBack.localScale.z);
    //        } else {
    //            showVisibleFace();
    //            Transform cardFront = transform.Find("Front");
    //            cardFront.localScale = new Vector3(scale, cardFront.localScale.y, cardFront.localScale.z);
    //        }           

    //        time = time + Time.deltaTime / duration;
    //        yield return new WaitForFixedUpdate();
    //    }
    //}

    // Animation de retournement de carte face visibile
    //public IEnumerator flipBack() {
    //    float time = 0f;
    //    bool endLoop = false;
    //    while (!endLoop) {
    //        float scale;
    //        if (time >= 1f) {
    //            endLoop = true;
    //            scale = scaleCurve.Evaluate(1f);
    //        } else {
    //            scale = scaleCurve.Evaluate(time);
    //        }

    //        if (time < 0.5f) {
    //            Transform cardFront = transform.Find("Front");
    //            cardFront.localScale = new Vector3(scale, cardFront.localScale.y, cardFront.localScale.z);
    //        } else {
    //            showHiddenFace();
    //            Transform cardBack = transform.Find("Back");
    //            cardBack.localScale = new Vector3(scale, cardBack.localScale.y, cardBack.localScale.z);
    //        }

    //        time = time + Time.deltaTime / duration;
    //        yield return new WaitForFixedUpdate();
    //    }
    //}
}
