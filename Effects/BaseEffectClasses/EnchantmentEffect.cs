﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using WeaponEnchantments.Common;
using WeaponEnchantments.Common.Utility;

namespace WeaponEnchantments.Effects {
    public abstract class EnchantmentEffect {
        public EnchantmentEffect() {
            
        }

        private string name;
        public string Name {
			get {
                if (name == null)
                    name = GetType().Name;

                return name;
			}
        }

        /// <summary>
        /// The EnchantmentStrength modifier based on damage type of the weapon.<br/>
        /// It maps <see cref="DamageClass">damage classes</see> to efficiency percentages.<br/>
        /// For example, if DamageClass.Melee is mapped to 0.8f, EnchantmentStrength will be reduced to 80% on melee weapons.<br/>
        /// </summary>
        protected Dictionary<DamageClass, float> EnchantmentDamageEfficiency = new Dictionary<DamageClass, float>();

        protected DifficultyStrength EnchantmentStrengthData;
        /// <summary>
        /// The strength of the effect.<br/>
        /// For example, if LifeSteal is mapped to an EnchantmentStrength of 1f, LifeSteal would heal 100% of the damage done.<br \>
        /// Another example is if Defense is mapped to an enchantment power of 5.5f, the defense would increase by 5.<br/>
        /// Affected by EnchantmentamageEfficiency and EfficiencyMultiplier<br/>
        /// </summary>
        public virtual float EffectStrength { get; protected set; }
        
        /// <summary>
        /// Used to modify the EnchantmentStrength<br/>
        /// Affected by the item type the enchantment is applied on. (Weapon, Armor, Accesory)<br/>
        /// </summary>
        public virtual float CombinedMultiplier {
            get => combinedMultiplier;
            protected set => combinedMultiplier = value;
        }
        protected float combinedMultiplier = 1f;
        public virtual float AllowedListMultiplier {
            get => allowedListMultiplier;
            set {
                allowedListMultiplier = value;
                CombinedMultiplier = allowedListMultiplier * damageClassMultiplier;
            }
        }
        protected float allowedListMultiplier = 1f;
        protected virtual float DamageClassMultiplier {
            get => damageClassMultiplier;
            set {
                damageClassMultiplier = value;
                CombinedMultiplier = allowedListMultiplier * damageClassMultiplier;
            }
        }
        protected float damageClassMultiplier = 1f;


        public virtual string DisplayName {
            get {
                if (displayName == null) {
                    if (DisplayNameArgs == null) {
                        displayName = $"{GetType().Name}{(DisplayNameNum > -1 ? DisplayNameNum : "")}".Lang(L_ID1.Tooltip, L_ID2.EffectDisplayName);
                    }
					else {
                        displayName = $"{GetType().Name}{(DisplayNameNum > -1 ? DisplayNameNum : "")}".Lang(L_ID1.Tooltip, L_ID2.EffectDisplayName, DisplayNameArgs);
                    }
				}

                return displayName;
			} 
        }
        protected string displayName;
        public virtual int DisplayNameNum => -1;
        //public static virtual Dictionary<string, string> LocalizationTooltips => new() { { GetType().Name, Tooltip } };//doesnt work. 
        public virtual IEnumerable<object> TooltipArgs => new object[] { DisplayName };
        public virtual IEnumerable<object> DisplayNameArgs { get; }
        public virtual string Tooltip => TooltipKey != null || TooltipArgs != null ? StandardTooltip : DisplayName;
        protected string StandardTooltip => this.GetEffectTooltip(TooltipArgs, TooltipKey);
        public virtual string TooltipName => Name;
        public virtual string TooltipKey => null;
        public virtual Color TooltipColor { get; protected set; } = Color.White;
        public virtual bool showTooltip => true;
        public abstract string TooltipValue { get; }

        public virtual float SelfStackingPenalty { get; protected set; } = 0f;

        public void SetDamageClassMultiplier(DamageClass dc) {
            DamageClassMultiplier = EnchantmentDamageEfficiency.ContainsKey(dc) ? EnchantmentDamageEfficiency[dc] : 1f;
        }
        public abstract EnchantmentEffect Clone();
    }
}
