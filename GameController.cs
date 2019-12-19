using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject[] deckCards; //holds all the basic card types from the deck
    [SerializeField] private GameObject[] playableCards; //holds all the special playable cards
    [SerializeField] private GameObject playerPlayArea; //holds a visual representation of the players cards
    [SerializeField] private GameObject aiPlayArea; //holds a visual representation of the AIs cards
    [SerializeField] private GameObject playerHandUI; //holds a visual representation of the players hand
    [SerializeField] private GameObject aiHandUI; //holds a visual representation of the AIs hand
    [SerializeField] private Text playerTotalText; //shows the players total for the current round
    [SerializeField] private Text aiTotalText; //shows the ais total for the current round
    [SerializeField] private GameObject[] playerScoreUI; //lights that turn on as the player wins rounds
    [SerializeField] private GameObject[] aiScoreUI; //lights that turn on as the AI wins rounds

    private List<GameObject> deck; //the current deck of cards, this is 'shuffled' before each round
    private List<GameObject> playableDeck; //the current playable deck of cards, shuffled and drawn from at the start of the game
    private List<GameObject> playerCards; //the current cards the player has played
    private List<GameObject> aiCards; //the current cards the ai has played

    private bool playerHolding; //true when the player is holding
    private bool aiHolding; //true when the AI is holding

    private int playerTotal; //the players running total for this round
    private int aiTotal; //the ais running total for this round

    private bool cardPlayed; //true if the player has played a card this turn

    private int aiHoldThreshold; //the threshold the AI will hold cards at when the player is not holding
    private int aiDrawThreshold; //the threshold the AI will hold cards at to draw with a holding player

    private int playerScore; //the total rounds the player has won. If they win 3 first, they win the game
    private int aiScore; //the total rounds the AI has won. If they win 3 first, they win the game

    private bool playerFirst; //if true, the player draws first this round, if false, the AI draws first

    [SerializeField] private AudioSource cardPlay; //audio to play when a card is drawn/played
    [SerializeField] private AudioSource roundOver; //audio to play when a round is over

    // Initialises the game
    void Start() {
        //initialising cards
        ResetCards();

        playerScore = 0;
        aiScore = 0;

        GameObject tempCard;
        //drawing cards to each players hand
        for(int i = 0; i < 4; i++) {
            tempCard = playableDeck[0];
            Instantiate(tempCard, playerHandUI.transform);
            playableDeck.RemoveAt(0);

            tempCard = playableDeck[0];
            Instantiate(tempCard.GetComponent<ICardController>().getPlayedCard(false), aiHandUI.transform);
            playableDeck.RemoveAt(0);
        }

        //determining who goes first
        if (Random.Range(0f, 1f) > 0.5f) playerFirst = true;
        else playerFirst = false;

        if (playerFirst) PlayerGo();
        else AIGo();
    }

    //simulates the players go
    private void PlayerGo() {
        if (!playerHolding) {
            DrawCard(true);
            cardPlayed = false;
        }
        else StartCoroutine(AIDelay());
    }

    //ends the players go
    public void EndPlayerGo() {
        cardPlayed = true; //stops the player playing cards during the opponents go
        if (!CheckTotals()) StartCoroutine(AIDelay());
        else StartCoroutine(RoundOver());
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
            else StartCoroutine(RoundOver());
        }
        else StartCoroutine(PlayerDelay());
    }

    //Simulates the AI
    private void SimulateAI() {
         CalculateAIThresholds(); //calculating thresholds for AI decision
         ICardController[] aiHand = aiHandUI.GetComponentsInChildren<ICardController>();
         if (aiTotal >= aiHoldThreshold && aiTotal >= playerTotal && aiTotal <= 20) AIHold(); //AI will choose to hold if near 20 and ahead/equal to the player
         else if (playerHolding) {
             if (aiTotal > playerTotal && aiTotal <= 20) AIHold(); //AI will hold if the player is holding, and the AI has a better score
             else if (aiTotal == playerTotal && aiTotal > aiDrawThreshold) AIHold(); //AI will hold if tied and likely to exceed 20 on next draw
             else { //will try to win/draw by playing cards in hand
                 int bestChoice = AICalculateChoices(aiHand); //calculating if theres a good card to play
                 if (bestChoice > -1) {
                     AIPlayCard(aiHand[bestChoice]); //if a good card to play is found... play it!
                     Destroy(aiHandUI.GetComponent<Transform>().GetChild(bestChoice).gameObject);
                     AIHold();
                 }
             }
         } else { //will play a card if one would help
             int bestChoice = AICalculateChoices(aiHand); //calculating if theres a good card to play
             if (bestChoice > -1) {
                 AIPlayCard(aiHand[bestChoice]); //if a good card to play is found... play it!
                 Destroy(aiHandUI.GetComponent<Transform>().GetChild(bestChoice).gameObject);
                 AIHold();
             }
         }
    }

    //Calculates which card in the AIs hand would be the best play
    private int AICalculateChoices(ICardController[] hand) {
        int bestChoice = -1; //value of -1 shows no decent choice was found, otherwise value will be the array index of the best card to play
        int bestTotal;
        if (aiTotal <= 20) bestTotal = aiTotal;
        else bestTotal = 0;
        int currentTotal;
        for (int i = 0; i < hand.Length; i++) {
            ICardController currentChoice = hand[i];
            if (currentChoice.IsSwitch()) {

                bool switchBack = true;
                //Testing value of original card sign
                currentTotal = aiTotal + currentChoice.getValue(); //the total if this card were played
                if (currentTotal > bestTotal && currentTotal <= 20) { //if playing this card would be the best option so far:
                    if (playerHolding) {
                        if (currentTotal > playerTotal) { //if playing this card would win this round:
                            bestTotal = currentTotal;
                            bestChoice = i;
                        } else if (currentTotal == playerTotal && currentTotal > aiDrawThreshold) { //if playing this card would draw this round:
                            bestTotal = currentTotal;
                            bestChoice = i;
                        }
                    } else if (currentTotal >= aiHoldThreshold && currentTotal >= playerTotal) {
                        bestTotal = currentTotal;
                        bestChoice = i;
                    }
                }

                currentChoice.OnSwitch(); //switching card sign and testing value of new sign
                currentTotal = aiTotal + currentChoice.getValue(); //the total if this card were played
                if (currentTotal > bestTotal && currentTotal <= 20) { //if playing this card would be the best option so far:
                    if (playerHolding) {
                        if (currentTotal > playerTotal) { //if playing this card would win this round:
                            bestTotal = currentTotal;
                            bestChoice = i;
                            switchBack = false;
                        } else if (currentTotal == playerTotal && currentTotal > aiDrawThreshold) { //if playing this card would draw this round:
                            bestTotal = currentTotal;
                            bestChoice = i;
                            switchBack = false;
                        }
                    } else if (currentTotal >= aiHoldThreshold && currentTotal >= playerTotal) {
                        bestTotal = currentTotal;
                        bestChoice = i;
                        switchBack = false;
                    }
                }

                if (switchBack) currentChoice.OnSwitch(); //if the first sign was better, switch the card back

            } else {
                currentTotal = aiTotal + currentChoice.getValue(); //the total if this card were played
                if (currentTotal > bestTotal && currentTotal <= 20) { //if playing this card would be the best option so far:
                    if (playerHolding) {
                        if (currentTotal > playerTotal) { //if playing this card would win this round:
                            bestTotal = currentTotal;
                            bestChoice = i;
                        } else if (currentTotal == playerTotal && currentTotal > aiDrawThreshold) { //if playing this card would draw this round:
                            bestTotal = currentTotal;
                            bestChoice = i;
                        }
                    } else if (currentTotal >= aiHoldThreshold && currentTotal >= playerTotal) {
                        bestTotal = currentTotal;
                        bestChoice = i;
                    }
                }
            }
        }
        return bestChoice;
    }

    //calculates the holding thresholds for the AIs decision making
    private void CalculateAIThresholds() {
        //AI Threshold increases based on the number of cards in it's hand, and it's opponents hand
        aiHoldThreshold = (int)(17 + ((float) aiHandUI.GetComponent<Transform>().childCount / 2f) 
            + ((float) playerHandUI.GetComponent<Transform>().childCount / 4f));
        aiDrawThreshold = aiHoldThreshold - 2; 
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
                aiScoreUI[aiScore++].SetActive(true); //increases the AIs score by 1, and turns on a visual representation
                return true;
            }
            else if (aiTotal > 20) {
                playerScoreUI[playerScore++].SetActive(true); //increases the Players score by 1, and turns on a visual representation
                return true;
            }
        } else {
            if (playerTotal > aiTotal && playerTotal < 21) playerScoreUI[playerScore++].SetActive(true);
            else if (aiTotal > playerTotal && aiTotal < 21) aiScoreUI[aiScore++].SetActive(true);
            else Debug.Log("DRAW");
            return true;
        }
        return false;
    }

    //called when a round ends
    private IEnumerator RoundOver() {
        Debug.Log("ROUND OVER");
        roundOver.Play();

        yield return new WaitForSeconds(5f);

        //if nobody has won the game (won 3 rounds) then start a new round
        if (aiScore < 3 && playerScore < 3) {
            ResetCards();
            if (playerFirst) {
                playerFirst = false;
                AIGo();
            } else {
                playerFirst = true;
                PlayerGo();
            }
        } else {
            if (aiScore == 3) Debug.Log("AI WINS! Score: " + aiScore + " - " + playerScore);
            else Debug.Log("PLAYER WINS! Score: " + playerScore + " - " + aiScore);
        }
    }

    //resets the cards
    private void ResetCards() {

        //deleting cards from previous rounds, if any
        DeleteChildren(aiPlayArea.transform);
        DeleteChildren(playerPlayArea.transform);

        deck = new List<GameObject>();
        playerCards = new List<GameObject>();
        aiCards = new List<GameObject>();
        playableDeck = new List<GameObject>();

        //starts the deck with 2 of each card type
        for (int i = 0; i < 10; i++) {
            deck.Add(deckCards[i]);
            deck.Add(deckCards[i]);
        }

        //starts the playable deck with 2 of each special card type
        for (int i = 0; i < playableCards.Length; i++) {
            playableDeck.Add(playableCards[i]);
            playableDeck.Add(playableCards[i]);
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

        //shuffles the playable deck
        System.Random rand2 = new System.Random();
        n = playableDeck.Count;
        for (int i = 0; i < n; i++)
        {
            int random = i + (int)(rand.NextDouble() * (n - i));
            temp = playableDeck[random];
            playableDeck[random] = playableDeck[i];
            playableDeck[i] = temp;
        }

        //reset round totals and 'Xholding' bools
        playerTotalText.text = "Total: 0";
        playerTotal = 0;
        playerHolding = false;
        aiTotalText.text = "Total: 0";
        aiTotal = 0;
        aiHolding = false; 
    }

    //Deletes the children of the provided Transform
    private void DeleteChildren(Transform parent) {
        Transform[] children = parent.GetComponentsInChildren<Transform>();
        for(int i = children.Length - 1; i > 0; i--) Destroy(children[i].gameObject);
    }


    //draws a card for the player (if isPlayer is true) or the ai (if false)
    private void DrawCard(bool isPlayer) {
        GameObject drawnCard = deck[0];
        cardPlay.Play();
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

    //returns cardPlayed, letting methods know if the player has played a card this turn
    public bool isCardPlayed() {
        return cardPlayed;
    }

    //called by cards in the players hand when played
    public void PlayCard(GameObject card) {
        cardPlay.Play();
        cardPlayed = true;
        GameObject playedCard = card.GetComponent<ICardController>().getPlayedCard(true);
        playerCards.Add(playedCard);
        Instantiate(playedCard, playerPlayArea.transform);
        playerTotal = GetTotal(playerCards);
        playerTotalText.text = "Total: " + playerTotal;
    }

    //called by the AI when playing a card
    private void AIPlayCard(ICardController card) {
        cardPlay.Play();
        GameObject playedCard = card.getPlayedCard(true);
        aiCards.Add(playedCard);
        Instantiate(playedCard, aiPlayArea.transform);
        aiTotal = GetTotal(aiCards);
        aiTotalText.text = "Total: " + aiTotal;
    }

    //swaps the signs on the players applicable +/- cards
    public void SwapSigns() {
        ICardController[] cards = playerHandUI.GetComponentsInChildren<ICardController>();
        for (int i = 0; i < cards.Length; i++) cards[i].OnSwitch();
    }
}
