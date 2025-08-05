using UnityEngine;

namespace UniformGridPathfinder.Samples.Assets.Scripts.AI.Skills
{
    public abstract class BaseAbility : IAbility
    {
        public string abilityName;
        private Sprite image;

        protected BaseAbility(string name, Sprite image)
        {
            abilityName = name;
            this.image = image;
        }

        public abstract void Execute();

        public abstract void Update();

        public Sprite GetImage()
        {
            return image;
        }

        public string GetName()
        {
            return abilityName;
        }
    }
}
