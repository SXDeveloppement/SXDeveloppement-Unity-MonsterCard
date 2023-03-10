using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AuraDisplay : MonoBehaviour, IDropHandler
{
    public GameObject slotCard;
    public GameObject cardOnSlot;

    public bool ownedByOppo;

    GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindAnyObjectByType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool onDrop(GameObject cardPlayed) {
        bool isPutOnBoard = false;
        if (gameManager.dragged) {
            GameObject targetSlot = this.gameObject;
            Debug.Log("Drop Aura");

            // Si l'emplacement est vide et que la carte dans la main ou dans la zone de contre attaque
            if (cardOnSlot == null && (cardPlayed.GetComponent<CardDisplay>().status == Status.Hand || cardPlayed.GetComponent<CardDisplay>().status == Status.SlotHidden)) {
                // On v�rifie les conditions de ciblage pour pouvoir placer la carte
                bool targetCondition = false;
                TargetType[] cardPlayedTargetType = cardPlayed.GetComponent<CardDisplay>().card.targetType;
                foreach (TargetType cardTargetType in cardPlayedTargetType) {
                    if (cardTargetType == TargetType.PlayerAura && !ownedByOppo) {
                        isPutOnBoard = gameManager.tryToPutOnBoard(cardPlayed, targetSlot, true);
                        targetCondition = true;
                        break;
                    }
                }

                // On place la carte si les conditions de ciblages sont respect�es
                if (!targetCondition) {
                    Debug.Log("ERR : bad target [" + targetSlot.name + "] / ownByOppo = " + ownedByOppo.ToString());
                }
            }
        }

        if (isPutOnBoard)
            cardOnSlot = cardPlayed;

        return isPutOnBoard;
    }

    void IDropHandler.OnDrop(PointerEventData eventData) {
        if (gameManager.dragged) {
            GameObject cardPlayed = eventData.pointerDrag;
            GameObject targetSlot = eventData.pointerCurrentRaycast.gameObject;

            //// On v�rifie que la cible soit bien un emplacement d'aura
            //bool isAura = false;
            //if (targetSlot == GO_Aura1 || targetSlot == GO_Aura2 || targetSlot == GO_Aura3 || targetSlot == GO_Aura4) {
            //    isAura = true;
            //}

            // Si l'emplacement est vide et que la carte dans la main ou dans la zone de contre attaque
            if (cardOnSlot == null && (cardPlayed.GetComponent<CardDisplay>().status == Status.Hand || cardPlayed.GetComponent<CardDisplay>().status == Status.SlotHidden)) {
                // On v�rifie les conditions de ciblage pour pouvoir placer la carte
                bool targetCondition = false;
                TargetType[] cardPlayedTargetType = cardPlayed.GetComponent<CardDisplay>().card.targetType;
                foreach (TargetType cardTargetType in cardPlayedTargetType) {
                    if (cardTargetType == TargetType.PlayerAura && !ownedByOppo) {
                        gameManager.tryToPutOnBoard(cardPlayed, targetSlot, true);
                        targetCondition = true;
                        break;
                    }
                }

                // On place la carte si les conditions de ciblages sont respect�es
                if (!targetCondition) {
                    Debug.Log("ERR : bad target [" + targetSlot.name + "] / ownByOppo = " + ownedByOppo.ToString());
                }
            }
        }
    }
}
