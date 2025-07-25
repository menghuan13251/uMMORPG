﻿// Base type for heal skill templates.
// => there may be target heal, targetless heal, aoe heal, etc.
using System.Text;
using UnityEngine;

public abstract class HealSkill : ScriptableSkill
{
    public LinearInt healsHealth;
    public LinearInt healsMana;
    public OneTimeTargetSkillEffect effect;

    // helper function to spawn the skill effect on someone
    // (used by all the buff implementations and to load them after saving)
    public void SpawnEffect(Entity caster, Entity spawnTarget)
    {
        if (effect != null)
        {
            GameObject go = Instantiate(effect.gameObject, spawnTarget.transform.position, Quaternion.identity);
            OneTimeTargetSkillEffect effectComponent = go.GetComponent<OneTimeTargetSkillEffect>();
            effectComponent.caster = caster;
            effectComponent.target = spawnTarget;
            NetworkServer.Spawn(go);
        }
    }

    // tooltip /////////////////////////////////////////////////////////////////
    public override string ToolTip(int skillLevel, bool showRequirements = false)
    {
        StringBuilder tip = new StringBuilder(base.ToolTip(skillLevel, showRequirements));
        tip.Replace("{HEALSHEALTH}", healsHealth.Get(skillLevel).ToString());
        tip.Replace("{HEALSMANA}", healsMana.Get(skillLevel).ToString());
        return tip.ToString();
    }
}
