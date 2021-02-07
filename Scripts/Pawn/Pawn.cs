﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkedVar;
using MLAPI.Messaging;

namespace Postcarbon {
  [System.Serializable]
  public class Pawn: NetworkedBehaviour {
    [Header("Components")]
    public Animator anim;
    public Rigidbody rb;
    public Collider col;
    public Equipment equipment;
    public GameObject floatingHealthBar;

    [Header("Stats")]
    public float maxHealth = 100;
    public float healthRegen = 0;
    public Faction faction = Faction.Loner;
    public NetworkedVarInt state = new NetworkedVarInt(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, 0);
    public NetworkedVarFloat health = new NetworkedVarFloat(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, 100f);
    public NetworkedVarFloat damage = new NetworkedVarFloat(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, 1f);
    public NetworkedVarFloat defense = new NetworkedVarFloat(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, 1f);

    [Header("Movement")]
    [HideInInspector] public float speed = 5;
    [HideInInspector] public bool grounded = true;
    [HideInInspector] public Vector3 spawnPoint;

    [Header("Combat")]
    public GameObject head;
    public GameObject spine;
    public float baseDamage = 1;
    public float baseDefense = 1;
    [HideInInspector] public int combo = 0;
    [HideInInspector] public bool attacking = false;
    [HideInInspector] public bool resetAttack = true;
    [HideInInspector] public Coroutine resetCombo;
    [HideInInspector] public Coroutine disableHealthBar;
    [HideInInspector] public Coroutine meleeRoutine;

    [Header("Ragdoll")]
    public int respawnTime = 30;
    [HideInInspector] public Rigidbody[] bonesRB;

    void stateChanged(int prevState, int newState) {
      anim.SetInteger("State", newState);
      if (prevState == (int)pS.Dead) { DisableRagdoll(); }
      else if (prevState == (int)pS.Aim) {
        if (GetComponent<Player>()) { GetComponent<Player>().cam.fieldOfView = 60; }
      }
      if (newState == (int)pS.Dead) { EnableRagdoll(); }
      else if (newState == (int)pS.Aim) {
        anim.SetTrigger("Aim");
        if (GetComponent<Player>()) { GetComponent<Player>().cam.fieldOfView = 45; }
      }
      else if (newState == (int)pS.Block) { anim.SetTrigger("Block"); }
      SetSpeed();
    }

    public override void NetworkStart() {
      base.NetworkStart();
      state.OnValueChanged += stateChanged;
    }

    #region Init
    public void initRagdoll() {
      bonesRB = transform.GetChild(0).GetComponentsInChildren<Rigidbody>(true);
      DisableRagdoll();
    }

    public void DisableRagdoll() {
      bonesRB = transform.GetChild(0).GetComponentsInChildren<Rigidbody>(true);
      for (int i = 0; i < bonesRB.Length; i++) {
        bonesRB[i].isKinematic = true;
        bonesRB[i].GetComponent<Collider>().enabled = false;
      }
      anim.enabled = true;
      col.enabled = true;
      if (rb) { rb.isKinematic = false; }
    }

    public void EnableRagdoll() {
      bonesRB = transform.GetChild(0).GetComponentsInChildren<Rigidbody>(true);
      for (int i = 0; i < bonesRB.Length; i++) {
        bonesRB[i].isKinematic = false;
        bonesRB[i].GetComponent<Collider>().enabled = true;
      }
      anim.enabled = false;
      col.enabled = false;
      if (rb) { rb.isKinematic = true; }
    }

    public virtual void Respawn() {
      transform.position = spawnPoint;
      health.Value = maxHealth;
      grounded = true;
      state.Value = 0;
    }
    #endregion

    #region Utils
    public void IsGrounded() {
      Ray ray = new Ray(col.bounds.center, Vector3.down);
      grounded = Physics.SphereCast(ray, 0.15f, col.bounds.extents.y);
      anim.SetBool("Grounded", grounded);
    }

    public void Equip() {
      if (state.Value == 0 || state.Value == (int)pS.Sprint) {
        equipment.holstered.Value = !equipment.holstered.Value;
        equipment.Holster(equipment.holstered.Value);
      }
    }

    public void PunchActive(bool active) {
      equipment.weaponSlot[0].GetComponent<Collider>().enabled = active;
    }
    #endregion

    #region Combat
    public void Attack() {
      if (state.Value == 0 || state.Value == (int)pS.Aim || state.Value == (int)pS.Sprint) {
        if (!equipment.holstered.Value) {
          equipment.holstered.Value = true;
          equipment.Holster(equipment.holstered.Value);
        }
        if (equipment.weapon[0] && equipment.weapon[0].dWeapon is dGun) { Shoot(); }
        else { Melee(); }
      }
    }

    public void StartMeleeAttack() {
      resetAttack = true;
      attacking = true;
      if (equipment.weapon[0] == null) { PunchActive(true); }
    }

    public void Melee() {
      state.Value = (int)pS.Attack;
      Timer.rDelay(this, StartMeleeAttack, 0.1f, meleeRoutine);
      anim.SetInteger("Combo", combo);
      AniTrig("Attack");
      resetCombo = Timer.rDelay(this, ResetCombo, 2, resetCombo);
      combo += 1;
      combo %= 2;
    }

    public void Shoot() {
      state.Value = (int)pS.Attack;
      AniTrig("Attack");
      Ray ray;
      RaycastHit hit;
      bool raycast = false;
      int prevLayer = gameObject.layer;
      gameObject.layer = 2;
      if (GetComponent<Player>()) {
        Player player = GetComponent<Player>();
        ray = new Ray(player.cam.transform.position, player.cam.transform.forward);
      }
      else { ray = new Ray(col.bounds.center, transform.forward); }
      raycast = Physics.Raycast(ray, out hit, 100);
      gameObject.layer = prevLayer;
      if (raycast && hit.collider.GetComponent<Pawn>()) {
        hit.collider.GetComponent<Pawn>().TakeDamage(damage.Value);
        if (hit.collider.GetComponent<NPC>() && hit.collider.GetComponent<Pawn>().faction != faction) {
          NPC npc = hit.collider.GetComponent<NPC>();
          npc.enemy.Add(this);
        }
      }
    }

    public void ResetCombo() {
      combo = 0;
      resetCombo = null;
    }
    #endregion

    #region Stats
    public void TakeDamage(float amount) {
      UpdateHealth(-amount);
      Timer.Delay(this, UpdateFloatingHealthBar, 0.1f);
    }

    public void UpdateHealth(float amount) {
      InvokeServerRpc(sUpdateHealth, amount);
    }

    [ServerRPC(RequireOwnership = false)]
    public void sUpdateHealth(float amount) {
      health.Value += amount;
      if (health.Value > maxHealth) { health.Value = maxHealth; }
      else if (health.Value <= 0) {
        health.Value = 0f;
        Die();
      }
    }

    public void UpdateFloatingHealthBar() {
      float percent = health.Value / maxHealth;
      floatingHealthBar.SetActive(true);
      floatingHealthBar.GetComponent<UI_Bar>().SetPercent(percent);
      Timer.rDelay(this, DisableHealthBar, 5, disableHealthBar);
    }

    public void DisableHealthBar() {
      floatingHealthBar.SetActive(false);
    }

    public bool HealthCost(float cost) {
      if (health.Value - cost > 0) {
        health.Value -= cost;
        return true;
      }
      else { return false; }
    }

    public void Die() {
      state.Value = (int)pS.Dead;
      Timer.Delay(this, Respawn, respawnTime);
    }
    #endregion

    #region Animator
    public virtual void SetSpeed() { }

    [ServerRPC(RequireOwnership = false)]
    public void AniTrig(string name) {
      if (IsServer) { InvokeClientRpcOnEveryone(nAniTrig, name); }
      else { InvokeServerRpc(AniTrig, name); }
    }
    [ClientRPC]
    public void nAniTrig(string name) {
      anim.SetTrigger(name);
    }
    #endregion
  }
}