﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MLAPI;

namespace SRPG {
  public class ISlot: NetworkedBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    [HideInInspector] public Hero hero;
    [HideInInspector] public int number;
    [HideInInspector] public Slot slot;
    [HideInInspector] public Image bgImage;
    [HideInInspector] public Sprite background;
    [HideInInspector] public Text textAmount;
    [HideInInspector] public GameObject slotIcon;
    [HideInInspector] public GameObject thisSlot;


    void Awake() {
      bgImage = GetComponent<Image>();
      slotIcon = transform.GetChild(0).gameObject;
      background = slotIcon.GetComponent<Image>().sprite;
      textAmount = slotIcon.GetComponentInChildren<Text>(true);
    }

    public void UpdateSlot() {
      slot = hero.inventory.slot[number];
      if (slot.amount > 0) {
        textAmount.text = slot.amount.ToString();
        if (slot.amount == 1) { textAmount.gameObject.SetActive(false); } else { textAmount.gameObject.SetActive(true); }
        slotIcon.GetComponent<Image>().sprite = slot.dItem.icon;
      }
      else {
        textAmount.text = slot.amount.ToString();
        textAmount.gameObject.SetActive(false);
        slotIcon.GetComponent<Image>().sprite = background;
      }
    }

    public void OnPointerClick(PointerEventData pointerEventData) {
      slot = hero.inventory.slot[number];
      if (slot.amount > 0) {
        hero.ClickItem(slot.dItem, number);
        UpdateSlot();
      }
    }

    public void OnPointerEnter(PointerEventData pointerEventData) {
      slot = hero.inventory.slot[number];
      bgImage.color = new Vector4(255, 255, 0, 200);
      if (slot.amount > 0) { hero.hud.DisplayInfo(slot.dItem); }
    }

    public void OnPointerExit(PointerEventData pointerEventData) {
      bgImage.color = new Vector4(255, 255, 255, 100);
      hero.hud.ResetInfo();
    }
  }
}