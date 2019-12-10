using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//interface to control cards
public interface ICardController
{
    //what to do when the player clicks the card
    void OnClick();

    //what to do when the player clicks the 'switch' button
    void OnSwitch();

    //returns the object this card would play
    GameObject getPlayedCard(bool isPlayed);
}
