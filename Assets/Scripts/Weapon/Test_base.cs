using UnityEngine;

public class Test_base : MonoBehaviour
{
    public int hp = 100;

    public void GetDamage(int damage)
    {
        hp -= damage;
    }
}
