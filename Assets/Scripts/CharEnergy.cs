using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharEnergy : MonoBehaviour
{
    public int energy = 4;
    public GameObject batteryUI;
    public Sprite[] batterySprite;
    float timeRecharged = 0f;
    Image batteryImage;
    bool recharging = false;

    void Start(){
        batteryImage = batteryUI.GetComponent<Image>();
    }

    void Update() {
        if (!recharging) {
            UpdateBattery(false);
        }
    }

    private void OnTriggerStay(Collider other) {
        if (other.tag == "Recharge" && energy <= 4) {
            recharging = true;
            if (energy == 0) UpdateBattery(true);
            else UpdateBattery(false);
            timeRecharged += Time.deltaTime;
            if (timeRecharged >= 5.0f && energy < 4) {
                timeRecharged = 0;
                energy++;
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "Recharge") {
            timeRecharged = 0;
            recharging = false;
        }
    }

    private void UpdateBattery(bool isRecharging) {
        if (isRecharging) {
            batteryImage.sprite = batterySprite[0];
        }
        else {
            batteryImage.sprite = batterySprite[energy + 1];
        }
    }

    public void DecEnergy() {
        if (energy > 0) {
            energy--;
        }
    }
}
