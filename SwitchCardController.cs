using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchCardController : MonoBehaviour, ICardController
{

    [SerializeField] private bool isPositive; //whether the card is currently positive or negative
    [SerializeField] private int value; //the current value of the card
    [SerializeField] private GameObject positiveSprite; //will be displayed when the card is positive
    [SerializeField] private GameObject negativeSprite; //will be displayed when the card is negative
    [SerializeField] private GameObject positiveCard; //the card object this represents when positive
    [SerializeField] private GameObject negativeCard; //the card object this represents when negative
    [SerializeField] private GameObject unplayedCard; //the card object this represents when unplayed (used in AIs hand)
    private GameController controller;

    //called when the game object is instantiated
    void Start() {
        controller = GameObject.Find("GameController").GetComponent<GameController>();
        isPositive = true;
    }

    //What to do when the card is clicked by the player
    public void OnClick() {
        if (!controller.isCardPlayed()) {
            controller.PlayCard(this.gameObject);
            Destroy(this.gameObject);
        }
    }

    //What happens when the player clicks the 'switch' button
    public void OnSwitch() {
        value *= -1;
        if (isPositive) {
            isPositive = false;
            positiveSprite.SetActive(false);
            negativeSprite.SetActive(true);
        } else {
            isPositive = true;
            positiveSprite.SetActive(true);
            negativeSprite.SetActive(false);
        }
    }

    //returns the object this card would play
    public GameObject getPlayedCard(bool isPlayed) {
        if (!isPlayed) return unplayedCard;
        else if (isPositive) return positiveCard;
        else return negativeCard;
    }
}
