using UnityEngine;

namespace Assets.Scripts.Skills
{
    public interface IAbility : IAbilityBase
    {
        string GetName();
        Sprite GetImage();
    }
}
