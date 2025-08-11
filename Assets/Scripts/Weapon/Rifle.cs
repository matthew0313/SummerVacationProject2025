using UnityEngine;

public class Rifle : Gun
{
    public override int bullets
    {
        get => owner.inventory.rifleBullets;
        set => owner.inventory.rifleBullets = value;
    }

    public override bool Trigger() => InputManager.Instance.ItemUseHoldInput();
}