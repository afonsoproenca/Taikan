﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPG {
  [CreateAssetMenu(fileName = "Scrap", menuName = "SRPG/Item/Scrap")]
  [System.Serializable]
  public class dScrap : dItem {
    public void Awake() {
      this.type = iT.Scrap;
    }
  }
}