using Unity.Netcode.Components;

public static class NetworkAnimatorExtensions
{
    public static void SetBool(this NetworkAnimator anim, int id, bool value) => anim.Animator.SetBool(id, value);
    public static void SetBool(this NetworkAnimator anim, string name, bool value) => anim.Animator.SetBool(name, value);

    public static void SetFloat(this NetworkAnimator anim, int id, float value) => anim.Animator.SetFloat(id, value);
    public static void SetFloat(this NetworkAnimator anim, string name, float value) => anim.Animator.SetFloat(name, value);

    public static void SetInteger(this NetworkAnimator anim, int id, int value) => anim.Animator.SetInteger(id, value);
    public static void SetInteger(this NetworkAnimator anim, string name, int value) => anim.Animator.SetInteger(name, value);
}