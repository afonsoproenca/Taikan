﻿// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.EventSystems;
// using MLAPI;

// namespace Postcarbon {
//   public class ESlot: NetworkedBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
//     public Transform slotIcon;
//     [HideInInspector] public dItem dItem;
//     [HideInInspector] public bool empty = true;
//     [HideInInspector] public Sprite background;
//     [HideInInspector] public Image bgImage;
//     [HideInInspector] public Hero hero;
//     [HideInInspector] public int number;

//     void Start() {
//       bgImage = GetComponent<Image>();
//       background = slotIcon.GetComponent<Image>().sprite;
//     }

//     public void UpdateSlot() {
//       if (hero.equipment.item[number]) {
//         dItem = hero.equipment.item[number];
//         slotIcon.GetComponent<Image>().sprite = dItem.icon;
//         empty = false;
//       }
//       else {
//         dItem = null;
//         slotIcon.GetComponent<Image>().sprite = background;
//         empty = true;
//       }
//     }

//     public void OnPointerClick(PointerEventData pointerEventData) {
//       if (!hero) { return; }
//       if (hero.equipment.item[number]) {
//         dItem = hero.equipment.item[number];
//         int slot = hero.equipment.GetSlot(dItem);
//         hero.hud.WriteWorldInfo("Unequipped " + dItem.name);
//         hero.equipment.UnequipItem(slot);
//       }
//       UpdateSlot();
//     }

//     public void OnPointerEnter(PointerEventData pointerEventData) {
//       if (!hero) { return; }
//       bgImage.color = new Vector4(255, 255, 0, 200);
//       if (hero.equipment.item[number]) {
//         dItem = hero.equipment.item[number];
//         hero.hud.DisplayItemInfo(dItem);
//       }
//       UpdateSlot();
//     }

//     public void OnPointerExit(PointerEventData pointerEventData) {
//       if (!hero) { return; }
//       bgImage.color = new Vector4(255, 255, 255, 100);
//       hero.hud.ResetInfo();
//     }
//   }
// }