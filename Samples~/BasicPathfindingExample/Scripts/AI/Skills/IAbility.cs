using UnityEngine;

namespace UniformGridPathfinder.Samples.Assets.Scripts.AI.Skills
{
    public interface IAbility : IAbilityBase
    {
        string GetName();
        Sprite GetImage();
    }
}
