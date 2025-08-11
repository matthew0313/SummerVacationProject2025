using UnityEngine;

public class Rifle : Gun
{
    public override int bullets
    {
        get => owner.inventory.rifleBullets;
        set => owner.inventory.rifleBullets = value;
    }

    public override bool Trigger()
    {
        return Input.GetMouseButton(0) && UIScanner.ScanUI(Input.mousePosition).Count <= 0;
    }
}