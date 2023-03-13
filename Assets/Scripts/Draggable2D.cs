using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class Draggable2D : MonoBehaviour
{
    public float underZoom;

    Vector3 startScale;
    Vector3 zoomScale;
    Vector2 sizeCard;
    Vector2 cachedMousePosition;
    GameObject placeHolder;
    GameManager gameManager;

    Vector3 position;
    GameObject GO_Hand;

    public bool isDragged = false; // Est gliss� pour les cartes de la main
    public bool isDraggedTemp;
    public bool isHalfDragged = false; // Est a moiti� gliss� pour les cartes du terrain
    
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindAnyObjectByType<GameManager>();

        startScale = transform.localScale;
        sizeCard = transform.GetComponent<RectTransform>().rect.size;

        isDraggedTemp = isDragged;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDraggedTemp != isDragged) {
            isDraggedTemp = isDragged;
            GetComponent<BoxCollider2D>().enabled = !isDragged;
        }
    }



    private void OnMouseDown() {
        if (GetComponent<CardDisplay>().status == Status.Hand && !GetComponent<CardDisplay>().ownedByOppo && !GameManager.dragged) {
            GameManager.dragged = true;
            isDragged = true;

            position = transform.position;

            zoomScale = transform.localScale;
            transform.localScale = startScale;
            placeHolder = GetComponent<ZoomCard2D>().placeHolder;
            GO_Hand = this.transform.parent.gameObject;
            this.transform.SetParent(this.transform.parent.parent);
        }
        // Si c'est un sbire sur le terrain et qu'il n'a pas attaqu� pendant le tour
        else if (!GameManager.dragged
            && GetComponent<CardDisplay>().status == Status.SlotVisible 
            && GetComponent<CardDisplay>().card.type == Type.Sbire
            && !GetComponent<SbireDisplay>().sbireHasAttacked 
            && !GetComponent<CardDisplay>().ownedByOppo) {
            isHalfDragged = true;
            GameManager.dragged = true;
            Cursor.SetCursor(gameManager.cursorTargetTexture, Vector2.zero, CursorMode.Auto);
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
        }
        // Si ce n'est pas une carte de contre attaque face cach� sur le terrain
        else if (!GameManager.dragged && GetComponent<CardDisplay>().status == Status.SlotHidden && GetComponent<CardDisplay>().card.type != Type.CounterAttack) {
            GameManager.dragged = true;
            isHalfDragged = true;
            Cursor.SetCursor(gameManager.cursorNoTargetTexture, Vector2.zero, CursorMode.Auto);
        }
        // Si c'est une carte "Echo" sur le terrain qui n'a pas �t� pos� ce tour ci
        else if (!GameManager.dragged && GetComponent<CardDisplay>().status == Status.SlotVisible && GetComponent<CardDisplay>().card.type == Type.Echo
        && !GetComponent<CardDisplay>().putOnBoardThisTurn && !GetComponent<CardDisplay>().ownedByOppo) {
            GameManager.dragged = true;
            isHalfDragged = true;
            Cursor.SetCursor(gameManager.cursorNoTargetTexture, Vector2.zero, CursorMode.Auto);
        }

    }

    private void OnMouseDrag() {
        if (!isDragged && !isHalfDragged) return;

        // Si c'est une carte de la main
        if (GetComponent<CardDisplay>().status == Status.Hand) {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            this.transform.position = new Vector3(mousePosition.x, mousePosition.y, position.z - 0.2f);

            int indexPlaceHolder = placeHolder.transform.GetSiblingIndex();
            int parentPlaceHolderChildCount = GO_Hand.transform.childCount;
            float layoutSpacing = GO_Hand.GetComponent<HorizontalLayoutGroup>().spacing;

            if (mousePosition.y < (GO_Hand.transform.position.y + GO_Hand.GetComponent<RectTransform>().rect.height / 2)) {
                placeHolder.SetActive(true);
                for (int i = 0; i < GO_Hand.transform.childCount; i++) {
                    GameObject cardInHand = GO_Hand.transform.GetChild(i).gameObject;
                    if (mousePosition.x < cardInHand.transform.position.x) {
                        bool prevCardIsPlaceholder = false;
                        if (i > 0) {
                            GameObject cardInHandPrev = GO_Hand.transform.GetChild(i-1).gameObject;
                            if (cardInHandPrev == placeHolder) {
                                prevCardIsPlaceholder = true;
                            }
                        }
                        if (!prevCardIsPlaceholder) {
                            if (placeHolder.transform.GetSiblingIndex() < i) {
                                placeHolder.gameObject.transform.SetSiblingIndex(i - 1);
                            } else {
                                placeHolder.gameObject.transform.SetSiblingIndex(i);
                            }
                        }

                        break;
                    }

                    // Exception last card
                    if (cardInHand != placeHolder && i == GO_Hand.transform.childCount - 1) {
                        placeHolder.gameObject.transform.SetSiblingIndex(GO_Hand.transform.childCount - 1);
                    }
                }

                // Si le scale de la carte est inf�rieur a startScale, on le met �gale a startScale
                if (transform.localScale.x < startScale.x) {
                    transform.localScale = startScale;
                }
            } else {
                placeHolder.SetActive(false);
                transform.localScale = new Vector3(underZoom, underZoom, underZoom);
            }
        }


        // Changement du curseur en fonction des cibles valident pour jouer la carte qui est sur le terrain
        if (GetComponent<CardDisplay>().status == Status.SlotHidden || GetComponent<CardDisplay>().status == Status.SlotVisible) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null) {
                bool targetAvailable = false;
                // Si c'est une carte face cach� qui n'est pas une contre attaque
                if (GetComponent<CardDisplay>().status == Status.SlotHidden && GetComponent<CardDisplay>().card.type != Type.CounterAttack) {

                }
                // Si c'est une carte sbire face visible
                else if (GetComponent<CardDisplay>().status == Status.SlotVisible && GetComponent<CardDisplay>().card.type == Type.Sbire) {
                    //*****************
                    // Prendre en compte les possibles sbire avec le taunt
                    //*****************

                    if (hit.collider.GetComponent<CardDisplay>() != null) {
                        CardDisplay cardDisplay = hit.collider.GetComponent<CardDisplay>();
                        if (cardDisplay.card.type == Type.Sbire && cardDisplay.ownedByOppo) {
                            targetAvailable = true;
                        }
                    } else if (hit.collider.GetComponent<MonsterDisplay>() != null) {
                        MonsterDisplay monsterDisplay = hit.collider.GetComponent<MonsterDisplay>();
                        if (monsterDisplay.ownedByOppo) {
                            targetAvailable = true;
                        }
                    }
                }
                // Si c'est une carte echo face visible
                else if (GetComponent<CardDisplay>().status == Status.SlotVisible && GetComponent<CardDisplay>().card.type == Type.Echo) {

                }

                if (targetAvailable) {
                    Cursor.SetCursor(gameManager.cursorTargetTexture, Vector2.zero, CursorMode.Auto);
                } else {
                    Cursor.SetCursor(gameManager.cursorNoTargetTexture, Vector2.zero, CursorMode.Auto);
                }
            }
        }

    }

    private void OnMouseUp() {
        if (!isDragged && !isHalfDragged) return;

        bool dropZoneValid = false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        if (hit.collider != null) {
            GameObject dropZone = hit.collider.gameObject;
            // Contre attaque
            if (dropZone.GetComponent<SlotDisplay>() != null) {
                dropZoneValid = dropZone.GetComponent<SlotDisplay>().onDrop(gameObject);
            }
            // Aura
            else if (dropZone.GetComponent<AuraDisplay>() != null) {
                dropZoneValid = dropZone.GetComponent<AuraDisplay>().onDrop(gameObject);
            }
            // Enchantement
            else if (dropZone.GetComponent<EquipmentDisplay>() != null) {
                dropZoneValid = dropZone.GetComponent<EquipmentDisplay>().onDrop(gameObject);
            }
            // Monster
            else if (dropZone.GetComponent<MonsterDisplay>() != null) {
                dropZoneValid = dropZone.GetComponent<MonsterDisplay>().onDrop(gameObject);
            }
            // Card
            else if (dropZone.GetComponent<CardDisplay>() != null) {
                dropZoneValid = dropZone.GetComponent<CardDisplay>().onDrop(gameObject);
            }
        }

        // Si on drop sur aucune dropZone valide
        if (!dropZoneValid && !isHalfDragged) {
            transform.localScale = startScale;
            this.transform.SetParent(GO_Hand.transform);
            GetComponent<ZoomCard2D>().changeWithPlaceholder();
            GetComponentInParent<HandDisplay>().childHaveChanged = true;
        }
        

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        GameManager.dragged = false;

        if (!dropZoneValid && isHalfDragged) {
            ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
        }

        isDragged = false;
        isHalfDragged = false;
    }
}