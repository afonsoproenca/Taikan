﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;
using MLAPI.NetworkedVar;


namespace Postcarbon {
  [System.Serializable]
  public class Humanoid: Pawn {
    [Header("Components")]
    public DynamicCharacterAvatar avatar;
    public NetworkedVarString recipeAvatar = new NetworkedVarString(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendChannel = "Avatar", SendTickrate = 0f }, null);
    [Header("Movement")]
    [HideInInspector] public bool aiming = false;
    [HideInInspector] public bool crouching = false;
    [HideInInspector] public Vector3 velocity = Vector3.zero;
    [HideInInspector] public Vector3 direction = Vector3.zero;

    #region Init
    public void recipeAvatarChanged(string oldRecipe, string newRecipe) {
      if (oldRecipe == newRecipe) { return; }
      LoadAvatar();
    }

    public void LoadAvatar() {
      if (recipeAvatar.Value != null) {
        avatar.LoadFromRecipeString(recipeAvatar.Value);
        avatar.LoadDefaultWardrobe();
        avatar.BuildCharacter();
      }
    }

    public override void NetworkStart() {
      base.NetworkStart();
      recipeAvatar.OnValueChanged += recipeAvatarChanged;
      if (IsLocalPlayer || IsServer) { return; }
      recipeAvatar.Value = avatar.GetCurrentRecipe();
    }

    public void RandomGender() {
      float male = Random.Range(0, 2);
      if (male < 1) {
        if (avatar.activeRace.name == "HumanMale") { avatar.ChangeRace("HumanFemale"); }
        else if (avatar.activeRace.name == "o3n Male") { avatar.ChangeRace("o3n Female"); }
      }
      else {
        if (avatar.activeRace.name == "HumanFemale") { avatar.ChangeRace("HumanMale"); }
        else if (avatar.activeRace.name == "o3n Female") { avatar.ChangeRace("o3n Male"); }
      }
    }

    public void SetHeight(float height) {
      avatar.GetDNA()["height"].Set(height);
    }
    #endregion



    #region Actions
    public void Block(bool block) {
      if (block) {
        if (state.Value == 0 || state.Value == (int)pS.Sprint) {
          if (!equipment.holstered.Value) {
            equipment.holstered.Value = true;
            equipment.Holster(equipment.holstered.Value);
          }
          if (equipment.weapon[0] && equipment.weapon[1] == null && equipment.weapon[0] is dGun) {
            if (!aiming) {
              aiming = true;
              anim.SetBool("Aiming", true);
              AniTrig("Aim");
              SetSpeed();
              if (GetComponent<Player>()) { GetComponent<Player>().cam.fieldOfView = 45; }
            }
          }
          else {
            state.Value = (int)pS.Block;
            AniTrig("Block");
          }
        }
      }
      // else if (aiming) {
      //   aiming = false;
      //   anim.SetBool("Aiming", false);
      //   SetSpeed();
      //   if (GetComponent<Player>()) { GetComponent<Player>().cam.fieldOfView = 60; }
      // }
    }

    public void Crouch(bool crouch) {
      if (crouching != crouch) {
        crouching = crouch;
        anim.SetBool("Crouching", crouch);
        SetSpeed();
      }
    }

    public void Sprint(bool sprint) {
      if (sprint) {
        if (grounded && !crouching) {
          if (state.Value == 0) { state.Value = (int)pS.Sprint; }
          else if (state.Value == (int)pS.Sprint) {
            if (GetComponent<Hero>()) {
              Hero hero = GetComponent<Hero>();
              if (!hero.StaminaCost(hero.sprintCost * Time.deltaTime)) { state.Value = 0; }
            }
          }
        }
        else if (crouching || !grounded) { if (state.Value == (int)pS.Sprint) { state.Value = 0; } }
      }
    }
    #endregion
  }
}
