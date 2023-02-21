using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = System.Random;

public class GameManager : MonoBehaviour
{
    public bool dragged; // TRUE si on drag&drop une carte

    public GameObject GO_Card; // Prefab
    public GameObject GO_Hand;
    public GameObject GO_DeckText;
    public GameObject GO_GraveText;
    public GameObject GO_GravePlayerList;
    public GameObject GO_Monster; // Prefab
    public GameObject GO_MonsterArea;
    public GameObject GO_MonsterInvoked;
    public List<GameObject> monstersGOList; // Liste des GO de tous les monstres
    public GameObject GO_MonsterAreaOppo;
    public GameObject GO_MonsterInvokedOppo;
    public List<GameObject> monstersGOListOppo; // Liste des GO de tous les monstres de l'adversaire
    public GameObject GO_MonsterTeamLayout;
    public GameObject GO_TeamArea;

    

    Dictionary<ElementalAffinity, float> fireDico;
    Dictionary<ElementalAffinity, float> waterDico;
    Dictionary<ElementalAffinity, float> electricDico;
    Dictionary<ElementalAffinity, float> earthDico;
    Dictionary<ElementalAffinity, float> combatDico;
    Dictionary<ElementalAffinity, float> mentalDico;
    Dictionary<ElementalAffinity, float> neutralDico;

    // Start is called before the first frame update
    void Start()
    {
        dragged = false;
        initializeArrayAffinity();

        Monster[] DBMonsters = Resources.LoadAll<Monster>("Monsters");
        // Cr�ation de l'�quipe de 4 monstres avec des monstres al�atoires de la DB
        List<Monster> addedMonster = new List<Monster>();
        while (monstersGOList.Count < 4) {
            Random rand = new Random();
            int randomInt = rand.Next(DBMonsters.Length);
            if (!addedMonster.Contains(DBMonsters[randomInt])) {
                // Instantie le monstre
                GameObject newMonster = Instantiate(GO_Monster);
                newMonster.name = "Monster";
                newMonster.GetComponent<MonsterDisplay>().monster = DBMonsters[randomInt];
                newMonster.transform.SetParent(GO_MonsterArea.transform);
                newMonster.transform.localPosition = new Vector3(273f, 28f, 0f);
                newMonster.GetComponent<MonsterDisplay>().ownedByOppo = false;

                // On ajoute le nouveau monstre a la liste
                monstersGOList.Add(newMonster);

                addedMonster.Add(DBMonsters[randomInt]);

                // On instantie le monstre dans la fen�tre d'�quipe du joueur
                GameObject newMonsterTeamLayout = Instantiate(GO_MonsterTeamLayout);
                newMonsterTeamLayout.name = "MonsterTeamLayout";
                newMonsterTeamLayout.GetComponent<MonsterDisplay>().monster = DBMonsters[randomInt];
                newMonsterTeamLayout.transform.SetParent(GO_TeamArea.transform);
                newMonsterTeamLayout.GetComponent<MonsterDisplay>().ownedByOppo = false;
            }
        }


        // Cr�ation de l'�quipe de 4 monstre pour l'adversaire
        addedMonster = new List<Monster>();
        while (monstersGOListOppo.Count < 4) {
            Random rand = new Random();
            int randomInt = rand.Next(DBMonsters.Length);
            if (!addedMonster.Contains(DBMonsters[randomInt])) {
                // Instantie le monstre
                GameObject newMonster = Instantiate(GO_Monster);
                newMonster.name = "Monster";
                newMonster.GetComponent<MonsterDisplay>().monster = DBMonsters[randomInt];
                newMonster.transform.SetParent(GO_MonsterAreaOppo.transform);
                newMonster.transform.localPosition = new Vector3(273f, 28f, 0f);
                newMonster.GetComponent<MonsterDisplay>().ownerOppo();
                newMonster.SetActive(false);

                // On ajoute le nouveau monstre a la liste
                monstersGOListOppo.Add(newMonster);

                addedMonster.Add(DBMonsters[randomInt]);
            }
        }        


        // On affiche le premier monstre
        monstersGOList[0].SetActive(true);
        monstersGOListOppo[0].SetActive(true);
        GO_MonsterInvoked = monstersGOList[0];
        GO_MonsterInvokedOppo = monstersGOListOppo[0];

        refreshDeckText();
        refreshGraveText();

    }

    // Update is called once per frame
    void Update()
    {
        if (!GO_MonsterInvoked.activeSelf) {
            GO_MonsterInvoked.SetActive(true);
            refreshDeckText();
        }
        if (!GO_MonsterInvokedOppo.activeSelf) {
            GO_MonsterInvokedOppo.SetActive(true);
        }
        if (!GO_TeamArea.transform.GetChild(0).gameObject.activeSelf) {
            foreach (Transform child in GO_TeamArea.transform) {
                child.gameObject.SetActive(true);
            }
        }

    }

    // Termine le tour en cours
    public void endTurn() {

        newTurn();
    }

    // Commende un nouveau tour
    public void newTurn() {
        draw(1);
        GO_MonsterInvoked.GetComponent<MonsterDisplay>().newTurn();
    }

    // Ajoute X carte dans la main
    public void draw(int drawAmount) {
        for (int i = 0; i < drawAmount; i++) {
            GameObject newCard = Instantiate(GO_Card);
            newCard.gameObject.transform.SetParent(GO_Hand.gameObject.transform);
            Random rand = new Random();
            int iRand = rand.Next(GO_MonsterInvoked.GetComponent<MonsterDisplay>().deckList.Count);
            newCard.GetComponent<CardDisplay>().card = GO_MonsterInvoked.GetComponent<MonsterDisplay>().deckList[iRand];
            newCard.name = newCard.GetComponent<CardDisplay>().card.name;
            newCard.GetComponent<CardDisplay>().status = Status.Hand;
            GO_MonsterInvoked.GetComponent<MonsterDisplay>().deckList.RemoveAt(iRand);
        }
        refreshDeckText();
    }
    
    // Actualise le nombre de carte restant dans le deck
    public void refreshDeckText() {
        GO_DeckText.gameObject.GetComponent<TMP_Text>().SetText(GO_MonsterInvoked.GetComponent<MonsterDisplay>().deckList.Count.ToString());
    }

    // Envoi une carte au cimeti�re
    public void inGrave(GameObject activedCard) {
        GO_MonsterInvoked.GetComponent<MonsterDisplay>().graveList.Add(activedCard.GetComponent<CardDisplay>().card);

        // On classe la liste des carte du cimeti�re par ordre alpha sur le nom de la carte
        GO_MonsterInvoked.GetComponent<MonsterDisplay>().graveList = GO_MonsterInvoked.GetComponent<MonsterDisplay>().graveList.OrderBy(o => o.name).ToList();

        Destroy(activedCard);
        dragged = false;

        // On detruit les cartes actuel du cimeti�re
        foreach (Transform child in GO_GravePlayerList.transform) {
            Destroy(child.gameObject);
        }

        refreshGrave();
        refreshGraveText();
    }

    // On refresh les cartes du cimeti�re
    public void refreshGrave() {
        // On detruit les cartes actuel du cimeti�re
        foreach (Transform child in GO_GravePlayerList.transform) {
            Destroy(child.gameObject);
        }

        // On instantie les cartes dans le cimeti�re
        foreach (Card nextCard in GO_MonsterInvoked.GetComponent<MonsterDisplay>().graveList) {
            GameObject newCard = Instantiate(GO_Card);
            newCard.gameObject.transform.SetParent(GO_GravePlayerList.transform);
            newCard.GetComponent<CardDisplay>().card = nextCard;
            newCard.name = newCard.GetComponent<CardDisplay>().card.name;
            newCard.GetComponent<CardDisplay>().status = Status.Graveyard;
        }
    }

    // Actualise le nombre de carte dans le cimeti�re
    public void refreshGraveText() {
        GO_GraveText.gameObject.GetComponent<TMP_Text>().SetText(GO_MonsterInvoked.GetComponent<MonsterDisplay>().graveList.Count.ToString());
    }

    // Affiche ou cache la cible
    public void showHide(GameObject target) {
        if (target.activeSelf) {
            target.SetActive(false);
        } else {
            target.SetActive(true);

            // On reset la scrollbar
            if (target.GetComponentInChildren<Scrollbar>() != null) {
                target.GetComponentInChildren<Scrollbar>().value = 1;
            }

            // On actualise les infos des monstres dans la fen�tre d'�quipe du joueur
            if (GO_TeamArea.transform.IsChildOf(target.transform)) {
                refreshTeamAreaLayout();
            }
        }
    }

    // On actualise les infos des monstres dans la fen�tre d'�quipe du joueur
    public void refreshTeamAreaLayout() {
        foreach (Transform child in GO_TeamArea.transform) {
            int indexChild = child.GetSiblingIndex();
            MonsterDisplay monsterDisplay = child.gameObject.GetComponent<MonsterDisplay>();
            monsterDisplay.healthAvailable = monstersGOList[indexChild].GetComponent<MonsterDisplay>().healthAvailable;
            monsterDisplay.healthMax = monstersGOList[indexChild].GetComponent<MonsterDisplay>().healthMax;
            monsterDisplay.manaAvailable = monstersGOList[indexChild].GetComponent<MonsterDisplay>().manaAvailable;
            monsterDisplay.manaMax = monstersGOList[indexChild].GetComponent<MonsterDisplay>().manaMax;
            
            monsterDisplay.refreshHealthPoint();
            monsterDisplay.refreshManaPoint();

            GameObject buttonSwap = GO_TeamArea.transform.parent.Find("LayoutSelection").GetChild(indexChild).GetComponentInChildren<Button>().gameObject;
            if (monstersGOList[indexChild] == GO_MonsterInvoked || monsterDisplay.healthAvailable <= 0) {
                buttonSwap.GetComponent<Button>().interactable = false;
                if (monsterDisplay.healthAvailable <= 0) {
                    buttonSwap.GetComponentInChildren<TMP_Text>().text = "K.O.";
                } else {
                    buttonSwap.GetComponentInChildren<TMP_Text>().text = "In battle";
                }
            } else {
                buttonSwap.GetComponent<Button>().interactable = true;
                buttonSwap.GetComponentInChildren<TMP_Text>().text = "Swap";
            }
        }
    }

    // On active une carte
    public void activeCardOnTarget(GameObject cardPlayed, GameObject target) {
        // On active la carte si son cout en mana est inf�rieur ou �gal au mana disponible
        if (cardPlayed.GetComponent<CardDisplay>().card.manaCost <= GO_MonsterInvoked.GetComponent<MonsterDisplay>().manaAvailable) {
            cardPlayed.GetComponent<CardDisplay>().activeCard(target);
            GO_MonsterInvoked.GetComponent<MonsterDisplay>().manaAvailable -= cardPlayed.GetComponent<CardDisplay>().card.manaCost;
            GO_MonsterInvoked.GetComponent<MonsterDisplay>().refreshManaPoint();
        } else {
            // On affiche un message d'erreur
            Debug.Log("ERR : no mana available");
        }
    }

    // Initialisation du tableau d'affinit�
    void initializeArrayAffinity() {
        fireDico = new Dictionary<ElementalAffinity, float>() {
            {ElementalAffinity.Fire, 1 },
            {ElementalAffinity.Water, 0.5f },
            {ElementalAffinity.Electric, 1 },
            {ElementalAffinity.Earth, 2 },
            {ElementalAffinity.Combat, 1 },
            {ElementalAffinity.Mental, 2 },
            {ElementalAffinity.Neutral, 1 }
        };

        waterDico = new Dictionary<ElementalAffinity, float>() {
            {ElementalAffinity.Fire, 2 },
            {ElementalAffinity.Water, 1 },
            {ElementalAffinity.Electric, 0.5f },
            {ElementalAffinity.Earth, 1 },
            {ElementalAffinity.Combat, 1 },
            {ElementalAffinity.Mental, 2 },
            {ElementalAffinity.Neutral, 1 }
        };

        electricDico = new Dictionary<ElementalAffinity, float>() {
            {ElementalAffinity.Fire, 1 },
            {ElementalAffinity.Water, 2 },
            {ElementalAffinity.Electric, 1 },
            {ElementalAffinity.Earth, 0.5f },
            {ElementalAffinity.Combat, 1 },
            {ElementalAffinity.Mental, 2 },
            {ElementalAffinity.Neutral, 1 }
        };

        earthDico = new Dictionary<ElementalAffinity, float>() {
            {ElementalAffinity.Fire, 0.5f },
            {ElementalAffinity.Water, 1 },
            {ElementalAffinity.Electric, 2 },
            {ElementalAffinity.Earth, 1 },
            {ElementalAffinity.Combat, 2 },
            {ElementalAffinity.Mental, 0.5f },
            {ElementalAffinity.Neutral, 1 }
        };

        combatDico = new Dictionary<ElementalAffinity, float>() {
            {ElementalAffinity.Fire, 1 },
            {ElementalAffinity.Water, 1 },
            {ElementalAffinity.Electric, 1 },
            {ElementalAffinity.Earth, 0.5f },
            {ElementalAffinity.Combat, 2 },
            {ElementalAffinity.Mental, 0.5f },
            {ElementalAffinity.Neutral, 2 }
        };

        mentalDico = new Dictionary<ElementalAffinity, float>() {
            {ElementalAffinity.Fire, 0.5f },
            {ElementalAffinity.Water, 0.5f },
            {ElementalAffinity.Electric, 0.5f },
            {ElementalAffinity.Earth, 2 },
            {ElementalAffinity.Combat, 2 },
            {ElementalAffinity.Mental, 1 },
            {ElementalAffinity.Neutral, 2 }
        };

        neutralDico = new Dictionary<ElementalAffinity, float>() {
            {ElementalAffinity.Fire, 1 },
            {ElementalAffinity.Water, 1 },
            {ElementalAffinity.Electric, 1 },
            {ElementalAffinity.Earth, 1 },
            {ElementalAffinity.Combat, 0.5f },
            {ElementalAffinity.Mental, 0.5f },
            {ElementalAffinity.Neutral, 1 }
        };
    }

    // Calcule du coef entre deux affinit�
    private float coefAffinity(ElementalAffinity affinityAttack, ElementalAffinity affinityDefenser) {
        switch (affinityAttack) {
            case ElementalAffinity.Fire:
                return fireDico[affinityDefenser];
            case ElementalAffinity.Water:
                return waterDico[affinityDefenser];
            case ElementalAffinity.Electric:
                return electricDico[affinityDefenser];
            case ElementalAffinity.Earth:
                return earthDico[affinityDefenser];
            case ElementalAffinity.Combat:
                return combatDico[affinityDefenser];
            case ElementalAffinity.Mental:
                return mentalDico[affinityDefenser];
            case ElementalAffinity.Neutral:
                return neutralDico[affinityDefenser];
            default:
                Debug.Log("Affinity not found");
                return 0;
        }
    }

    // Calcule le plus grand coef d'affinit� entre une attaque et un monstre d�fenseur
    private float coefAffinityMax(ElementalAffinity affinityAttack, GameObject target) {
        float coefMax = 0;
        foreach(ElementalAffinity affinityDefenser in target.GetComponent<MonsterDisplay>().monster.elementalAffinity) {
            float coefCal = coefAffinity(affinityAttack, affinityDefenser);
            if (coefCal > coefMax) {
                coefMax = coefCal;
            }
        }

        return coefMax;
    }

    // On calcule les d�gats r�el
    public int calculateDamage(GameObject target, ElementalAffinity attackAffinity, int amountDamage) {
        GameObject attacker;
        GameObject defenser;
        if (target == GO_MonsterInvoked) {
            attacker = GO_MonsterInvokedOppo;
            defenser = GO_MonsterInvoked;
        } else {
            attacker = GO_MonsterInvoked;
            defenser = GO_MonsterInvokedOppo;
        }

        int power = attacker.GetComponent<MonsterDisplay>().monster.powerPoint;
        int guard = defenser.GetComponent<MonsterDisplay>().monster.guardPoint;
        float diffPowerGuard = 0;
        // On v�rifie si le power > guard
        if (power > guard) {
            diffPowerGuard = (float) (power - guard) / 100;
        }

        // On calcule le bonus d'affinit� du monstre attaquant (+25% de d�g�ts si l'attaquant poss�de la m�me affinit� que l'attaque)
        float bonusAffinity = 1;
        foreach(ElementalAffinity affinity in attacker.GetComponent<MonsterDisplay>().monster.elementalAffinity) {
            if (affinity == attackAffinity) {
                bonusAffinity = 1.25f;
                break;
            }
        }

        // On calcule le coef d'affinit� max
        float affinityCoef = coefAffinityMax(attackAffinity, target);

        float resultDamage = amountDamage * (1 + Mathf.Log(1 + diffPowerGuard) * 2 / 3) * bonusAffinity * affinityCoef;
        return Mathf.RoundToInt(resultDamage);
    }

    // Change le monstre actif
    public void swapMonster(int indexMonster) {
        GameObject nextMonster = monstersGOList[indexMonster];

        // On desactive tous les GO des monstres
        foreach (Transform child in GO_MonsterArea.transform) {
            child.gameObject.SetActive(false);
        }

        // On compte le nombre de carte dans la main
        int amountCardInHand = GO_Hand.transform.childCount;
        // On remet les cartes de la main dans le deck du monstre pr�c�dant
        foreach (Transform cardInHand in GO_Hand.transform) {
            GO_MonsterInvoked.GetComponent<MonsterDisplay>().deckList.Add(cardInHand.gameObject.GetComponent<CardDisplay>().card);
            Destroy(cardInHand.gameObject);
        }

        // On change de monstre actif
        nextMonster.SetActive(true);
        GO_MonsterInvoked = nextMonster;

        // On pioche de nouvelle carte dans le deck du nouveau monstre
        draw(amountCardInHand);
        // On r�initialise son mana
        GO_MonsterInvoked.GetComponent<MonsterDisplay>().resetMana();

        // On actualise le deck et le cimeti�re
        refreshDeckText();
        refreshGrave();
        refreshGraveText();
    }

    public void swapMonsterOppo(int indexMonster) {
        GameObject nextMonster = monstersGOListOppo[indexMonster];

        // On desactive tous les GO des monstres
        foreach (Transform child in GO_MonsterAreaOppo.transform) {
            child.gameObject.SetActive(false);
        }

        // On change de monstre actif
        nextMonster.SetActive(true);
        GO_MonsterInvokedOppo = nextMonster;

        // On r�initialise son mana
        GO_MonsterInvokedOppo.GetComponent<MonsterDisplay>().resetMana();

        // On actualise le deck et le cimeti�re
        // De l'adversaire
    }

    // DEBUG Affiche le prochain monstre
    public void showNextMonster() {
        if (GO_MonsterInvoked.transform.GetSiblingIndex() == monstersGOList.Count - 1) {
            swapMonster(0);
        } else {
            swapMonster(GO_MonsterInvoked.transform.GetSiblingIndex() + 1);
        }
    }

    // DEBUG Affiche le prochain monstre de l'adversaire
    public void showNextMonsterOppo() {
        if (GO_MonsterInvokedOppo.transform.GetSiblingIndex() == monstersGOListOppo.Count - 1) {
            swapMonsterOppo(0);
        } else {
            swapMonsterOppo(GO_MonsterInvokedOppo.transform.GetSiblingIndex() + 1);
        }
    }
}
