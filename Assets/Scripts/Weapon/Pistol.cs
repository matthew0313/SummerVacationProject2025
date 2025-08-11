using UnityEngine;

public class Pistol : Gun
{
    public override int bullets
    {
        get => owner.inventory.pistolBullets;
        set => owner.inventory.pistolBullets = value;
    }
}