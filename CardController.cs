using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardController : MonoBehaviour, ICardController
{

    [SerializeField] private int value; //the value of the card
    [SerializeField] private GameObject playedCard; //the card object will play
    private GameController controller;

    //called when the game object is instantiated
    void Start() {
        controller = GameObject.Find("GameController").GetComponent<GameController>();
    }

    //What to do when the card is clicked by the player
    public void OnClick() {
        //plays the card if the player hasn't already played one this turn
        if (!controller.isCardPlayed()) {
            controller.PlayCard(this.gameObject);
            Destroy(this.gameObject);
        }
    }

    //What happens when the player clicks the 'switch' button
    public void OnSwitch() {
        //Regular cards do nothing here
    }

    //returns the object this card would play
    public GameObject getPlayedCard(bool isPlayed) {
        return playedCard;
    }
}
