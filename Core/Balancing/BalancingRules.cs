﻿using CalamityMod;
using System.Linq;
using Terraria;

namespace InfernumMode.Core.Balancing
{
    public class PierceResistBalancingRule : IBalancingRule
    {
        public float DamageMultiplier;
        public PierceResistBalancingRule(float damageMultiplier) => DamageMultiplier = damageMultiplier;

        public bool AppliesTo(NPC npc, NPCHitContext hitContext) => hitContext.Pierce is > 1 or (-1);

        public void ApplyBalancingChange(NPC npc, ref NPC.HitModifiers modifiers) => modifiers.SourceDamage *= DamageMultiplier;
    }

    public class ProjectileResistBalancingRule : IBalancingRule
    {
        public float DamageMultiplier;
        public int[] ApplicableProjectileTypes;
        public ProjectileResistBalancingRule(float damageMultiplier, params int[] projTypes)
        {
            DamageMultiplier = damageMultiplier;
            ApplicableProjectileTypes = projTypes;
        }

        public bool AppliesTo(NPC npc, NPCHitContext hitContext)
        {
            if (hitContext.DamageSource != DamageSourceType.FriendlyProjectile)
                return false;
            if (!ApplicableProjectileTypes.Contains(hitContext.ProjectileType ?? -1))
                return false;

            return true;
        }

        public void ApplyBalancingChange(NPC npc, ref NPC.HitModifiers modifiers) => modifiers.SourceDamage *= DamageMultiplier;
    }

    public class ClassResistBalancingRule : IBalancingRule
    {
        public float DamageMultiplier;

        public ClassType ApplicableClass;

        public ClassResistBalancingRule(float damageMultiplier, ClassType classType)
        {
            DamageMultiplier = damageMultiplier;
            ApplicableClass = classType;
        }

        public bool AppliesTo(NPC npc, NPCHitContext hitContext)
        {
            return hitContext.Class == ApplicableClass;
        }

        public void ApplyBalancingChange(NPC npc, ref NPC.HitModifiers modifiers) => modifiers.SourceDamage *= DamageMultiplier;
    }

    public class StealthStrikeBalancingRule : IBalancingRule
    {
        public float DamageMultiplier;
        public int[] ApplicableProjectileTypes;
        public StealthStrikeBalancingRule(float damageMultiplier, params int[] projTypes)
        {
            DamageMultiplier = damageMultiplier;
            ApplicableProjectileTypes = projTypes;
        }

        public bool AppliesTo(NPC npc, NPCHitContext hitContext)
        {
            if (hitContext.DamageSource != DamageSourceType.FriendlyProjectile)
                return false;
            if (!ApplicableProjectileTypes.Contains(hitContext.ProjectileType ?? -1))
                return false;

            return hitContext.IsStealthStrike;
        }

        public void ApplyBalancingChange(NPC npc, ref NPC.HitModifiers modifiers) => modifiers.SourceDamage *= DamageMultiplier;
    }

    public class TrueMeleeBalancingRule : IBalancingRule
    {
        public float DamageMultiplier;
        public TrueMeleeBalancingRule(float damageMultiplier)
        {
            DamageMultiplier = damageMultiplier;
        }

        public bool AppliesTo(NPC npc, NPCHitContext hitContext)
        {
            if (hitContext.DamageSource == DamageSourceType.FriendlyProjectile)
                return Main.projectile[hitContext.ProjectileIndex.Value].IsTrueMelee();

            return hitContext.DamageSource == DamageSourceType.TrueMeleeSwing;
        }

        public void ApplyBalancingChange(NPC npc, ref NPC.HitModifiers modifiers) => modifiers.SourceDamage *= DamageMultiplier;
    }

    public class NPCSpecificRequirementBalancingRule : IBalancingRule
    {
        public NPCApplicationRequirement Requirement;
        public delegate bool NPCApplicationRequirement(NPC npc);
        public NPCSpecificRequirementBalancingRule(NPCApplicationRequirement npcApplicationRequirement)
        {
            Requirement = npcApplicationRequirement;
        }

        public bool AppliesTo(NPC npc, NPCHitContext hitContext) => Requirement(npc);

        // This "balancing" rule doesn't actually perform any changes. It simply serves as a means of enforcing NPC-specific requirements.
        // As such, this method is empty.
        public void ApplyBalancingChange(NPC npc, ref NPC.HitModifiers modifiers) { }
    }
}
