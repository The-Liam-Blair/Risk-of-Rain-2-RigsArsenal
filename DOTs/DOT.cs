/*using R2API;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2;

namespace MoreItems.DOTs
{
    public abstract class DOT
    {
        public abstract string Name { get; }
        public BuffDef buffDef { get; set; }

        public abstract bool canStack { get; }
        public virtual bool isDebuff { get; } = false; // DOTs are (apparently?) classed as debuffs already so would double dip for death's mark etc.

        public virtual bool isHidden { get; } = false;
        public virtual bool isCooldown { get; } = false;

        public abstract Color BuffColor { get; }

        public abstract Sprite Icon { get; }

        public DotController.DotDef dotDef { get; set; }
        public DotController.DotIndex dotIndex { get; set; }

        public abstract float dotDamageCoefficient { get; }
        public abstract float dotInterval { get; }
        public abstract DamageColorIndex dotDamageColorIndex { get; }
        public virtual bool resetTimerOnAdd { get; } = false;

        public virtual void Init()
        {
            CreateDOT();
        }

        public virtual void CreateDOT()
        {
            DebugLog.Log("Creating DOT: " + Name);

            // Create buff definition for the DOT.
            buffDef = ScriptableObject.CreateInstance<BuffDef>();

            buffDef.name = Name;
            buffDef.buffColor = BuffColor;

            buffDef.canStack = canStack;
            buffDef.isDebuff = isDebuff;
            buffDef.isHidden = isHidden;
            buffDef.isCooldown = isCooldown;

            buffDef.iconSprite = (Icon != null)
                ? Icon
                : Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();

            ContentAddition.AddBuffDef(buffDef);

            // Create dot definition.
            dotDef = new DotController.DotDef();

            dotDef.damageCoefficient = dotDamageCoefficient;
            dotDef.interval = dotInterval;
            dotDef.associatedBuff = buffDef;
            dotDef.damageColorIndex = dotDamageColorIndex;
            dotDef.resetTimerOnAdd = resetTimerOnAdd;

            // Implemented DOTS will register the dot definition after this base method.
        }
    }
}
*/