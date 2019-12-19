using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private bool growing; //whether the card is currently growing or not
    private Vector3 initialScale; //initial scale of the card
    private Vector3 growScale = new Vector3(0.015f, 0.015f, 0); //how much the card will grow each interval
    private int growIntervals = 10; //how many intervals the card will grow for
    private float intervalLength = 0.005f; //the time between each growth interval

    [SerializeField] private AudioSource audio; //the audio that will play when the card is hovered over

    //initialises card
    void Start() {
        initialScale = transform.localScale;
    }

    //what to do when the card is moused over
    public void OnPointerEnter(PointerEventData data) {
        if (!growing) StartCoroutine(GrowCard());
        audio.Play();
    }

    //what to do when the mouse is no longer over the card
    public void OnPointerExit(PointerEventData data) {
        transform.localScale = initialScale;
        growing = false;
    }

    //makes the card grow
    private IEnumerator GrowCard() {
        growing = true;
        for (int i = 0; i < growIntervals; i++){
            if (growing) {
                yield return new WaitForSeconds(intervalLength);
                transform.localScale += growScale;
            } else transform.localScale = initialScale;
        }
        growing = false;
    }
}
