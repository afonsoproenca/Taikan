﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SRPG {
  //pawnState
  public enum pS { Idle, Attack, Block, Sprint, Dodge, Climb, Swim, Dead };
  //itemType
  public enum iT { Scrap, Consumable, Weapon, Armor };
  //armorType
  public enum aT { Light, Medium, Heavy }
  //weaponType
  public enum wT { Unarmed, Sword, Rifle, Gun, WoodAxe, Shield }
  //armorSlot
  public enum aS { Helmet, Chest, Hands, Legs, Feet };
  //weaponSlot
  public enum wS { RightHand, LeftHand, TwoHand, AnyHand };
  //equipmenSlot
  public enum eS { RightHand, LeftHand, Helmet, Chest, Hands, Legs, Feet };
  //Factions
  public enum Faction { Loner, Survivor, Mutant, Cyborg, Raider, Plagued, Feral, Critter};

  public static class Timer {
    public static Coroutine Delay(this MonoBehaviour monoBehaviour, Action action, float time) {
      return monoBehaviour.StartCoroutine(DelayRoutine(action, time));
    }

    public static Coroutine rDelay(MonoBehaviour monoBehaviour, Action action, float time, Coroutine coroutine) {
      if (coroutine != null) {
        monoBehaviour.StopCoroutine(coroutine);
      }
      return monoBehaviour.StartCoroutine(DelayRoutine(action, time));
    }

    private static IEnumerator DelayRoutine(Action action, float time) {
      yield return new WaitForSeconds(time);
      action();
    }
  }
}