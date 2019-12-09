using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] private int value; //the value of the card

    //gets the value of the card
    public int getValue() {
        return value;
    }
}
