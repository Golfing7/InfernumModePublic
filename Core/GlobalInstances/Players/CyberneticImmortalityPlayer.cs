using CalamityMod.NPCs.ExoMechs;
using InfernumMode.Assets.Sounds;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace InfernumMode.Core.GlobalInstances.Players
{
    public class CyberneticImmortalityPlayer : ModPlayer
    {
        public bool CyberneticImmortalityIsActive
        {
            get;
            set;
        }

        public int HurtSoundCountdown
        {
            get;
            set;
        }

        public void ToggleImmortality()
        {
            CyberneticImmortalityIsActive = !CyberneticImmortalityIsActive;
            Utilities.DisplayText($"Cybernetic immortality has been {(CyberneticImmortalityIsActive ? "enabled" : "disabled")}.", Draedon.TextColor);
        }

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter)
        {
            if (CyberneticImmortalityIsActive && HurtSoundCountdown <= 0)
            {
                HurtSoundCountdown = 60;
                SoundEngine.PlaySound(InfernumSoundRegistry.AresTeslaShotSound, Player.Center);
                playSound = false;
            }

            return !CyberneticImmortalityIsActive;
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            return !CyberneticImmortalityIsActive;
        }

        public override void PreUpdate()
        {
            if (HurtSoundCountdown > 0)
                HurtSoundCountdown--;

            if (!CyberneticImmortalityIsActive)
                return;

            Player.statLife = Player.statLifeMax2;
        }

        public override void SaveData(TagCompound tag)
        {
            tag["CyberneticImmortalityIsActive"] = CyberneticImmortalityIsActive;
        }

        public override void LoadData(TagCompound tag)
        {
            CyberneticImmortalityIsActive = tag.GetBool("CyberneticImmortalityIsActive");
        }
    }
}