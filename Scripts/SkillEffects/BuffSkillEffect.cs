﻿// A simple skill effect that follows the target until it ends.
// -> Can be used for buffs.
//
// Note: Particle Systems need Simulation Space = Local for it to work.
using UnityEngine;

public class BuffSkillEffect : SkillEffect
{
    float lastRemainingTime = Mathf.Infinity;
    [ HideInInspector] public string buffName;

    void Update()
    {
        // only while target still exists, buff still active and hasn't been
        // recasted
        if (target != null)
        {
            int index = target.skills.GetBuffIndexByName(buffName);
            if (index != -1)
            {
                Buff buff = target.skills.buffs[index];
                if (lastRemainingTime >= buff.BuffTimeRemaining()) {
                    transform.position = target.collider.bounds.center;
                    lastRemainingTime = buff.BuffTimeRemaining();
                    return;
                }
            }
        }

        // if we got here then something wasn't good, let's destroy self
        if (isServer) NetworkServer.Destroy(gameObject);
    }
}
