using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject[] deckCards; //holds all the basic card types from the deck
    [SerializeField] private GameObject playerPlayArea; //holds a visual representation of the players cards
    [SerializeField] private GameObject aiPlayArea; //holds a visual representation of the AIs cards
    [SerializeField] private Text playerTotalText; //shows the players total for the current round
    [SerializeField] private Text aiTotalText; //shows the ais total for the current round

    private List<GameObject> deck; //the current deck of cards, this is 'shuffled' before each round
    private List<GameObject> playerCards; //the current cards the player has played
    private List<GameObject> aiCards; //the current cards the ai has played

    private bool playerHolding; //true when the player is holding
    private bool aiHolding; //true when the AI is holding

    private int playerTotal; //the players running total for this round
    private int aiTotal; //the ais running total for this round

    // Initialises the game
    void Start() {
        //initialising cards
        ResetCards();
        if (Random.Range(0f, 1f) > 0.5f) AIGo(); //50% chance for AI to go first
        else PlayerGo();
    }

    //simulates the players go
    private void PlayerGo() {
        if (!playerHolding) DrawCard(true);
        else StartCoroutine(AIDelay());
    }

    //ends the players go
    public void EndPlayerGo() {
        if (!CheckTotals()) StartCoroutine(AIDelay());
    }

    //will cause the player to hold, locking in their current total
    public void PlayerHold() {
        playerHolding = true;
        EndPlayerGo();
    }

    //simulates the AIs go
    private void AIGo() {
        if (!aiHolding) {
            DrawCard(false);
            SimulateAI();
            if (!CheckTotals()) StartCoroutine(PlayerDelay());
        }
        else StartCoroutine(PlayerDelay());
    }

    //Simulates the AI
    private void SimulateAI() {
        //if the AI's score is under 21 still
        if (!CheckTotals()) {
            if (aiTotal >= 18 && aiTotal >= playerTotal) AIHold(); //AI will choose to hold if near 20 and ahead/equal to the player
            else if (playerHolding)
            {
                if (aiTotal == playerTotal && aiTotal > 16) AIHold(); //AI will hold if tied and likely to exceed 20 on next draw
                else if (aiTotal > playerTotal) AIHold(); //AI will hold if the player is holding, and the AI has a better score
            }
        }
    }

    //will cause the AI to hold, locking in their current total
    private void AIHold() {
        aiHolding = true;
        Debug.Log("AI HOLDING...");
    }

    //creates a slight delay before the AIs go starts
    private IEnumerator AIDelay() {
        yield return new WaitForSeconds(1.5f);
        AIGo();
    }

    //creates a slight delay before the players go starts
    private IEnumerator PlayerDelay() {
        yield return new WaitForSeconds(1f);
        PlayerGo();
    }

    //checks the totals between gos, returns true if the round is over
    private bool CheckTotals() {
        if (!playerHolding || !aiHolding) {
            if (playerTotal > 20) {
                Debug.Log("AI WINS");
                return true;
            }
            else if (aiTotal > 20) {
                Debug.Log("PLAYER WINS");
                return true;
            }
        } else {
            if (playerTotal > aiTotal) Debug.Log("PLAYER WINS");
            else if (aiTotal > playerTotal) Debug.Log("AI WINS");
            else Debug.Log("DRAW");
            return true;
        }
        return false;
    }

    //resets the cards
    private void ResetCards() {
        deck = new List<GameObject>();
        playerCards = new List<GameObject>();
        aiCards = new List<GameObject>();

        //starts the deck with 2 of each card type
        for(int i = 0; i < 10; i++) {
            deck.Add(deckCards[i]);
            deck.Add(deckCards[i]);
        }

        //shuffles the deck
        System.Random rand = new System.Random();
        int n = deck.Count;
        GameObject temp;
        for(int i = 0; i < n; i++) {
            int random = i + (int)(rand.NextDouble() * (n - i));
            temp = deck[random];
            deck[random] = deck[i];
            deck[i] = temp;
        } 
    }

    //draws a card for the player (if isPlayer is true) or the ai (if false)
    private void DrawCard(bool isPlayer) {
        GameObject drawnCard = deck[0];
        deck.RemoveAt(0); //removes the top card from the deck
        if (isPlayer) {
            playerCards.Add(drawnCard);
            Instantiate(drawnCard, playerPlayArea.transform);
            playerTotal = GetTotal(playerCards);
            playerTotalText.text = "Total: " + playerTotal;
        } else {
            aiCards.Add(drawnCard);
            Instantiate(drawnCard, aiPlayArea.transform);
            aiTotal = GetTotal(aiCards);
            aiTotalText.text = "Total: " + aiTotal;
        }
    }

    //gets the total of a set of played cards
    private int GetTotal(List<GameObject> cardsPlayed) {
        int total = 0;
        for (int i = 0; i < cardsPlayed.Count; i++) total += cardsPlayed[i].GetComponent<Card>().getValue();
        return total;
    }
}
