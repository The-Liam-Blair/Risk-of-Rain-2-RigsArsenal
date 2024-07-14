using System;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MoreItems.Buffs
{
    public abstract class Buff
    {
        public abstract string Name { get; }
        public BuffDef buffDef { get; set; }

        public abstract bool canStack { get; }
        public abstract bool isDebuff { get; }

        public virtual bool isHidden { get; } = false;
        public virtual bool isCooldown { get; } = false;

        public abstract Color BuffColor { get; }

        public abstract Sprite Icon { get; }

        public virtual void Init()
        {
            CreateBuff();
            SetupHooks();
        }

        public virtual void CreateBuff()
        {
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
        }

        public virtual void SetupHooks() {}
    }
}
