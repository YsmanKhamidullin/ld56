using UnityEngine;

public class Ant : MonoBehaviour
{
    public int Health;
    public int Damage;
    public float MoveSpeed;
    public float AttackRange;
    public float AttackDelay;
    public float LastAttackTime;
    public float InvinsibleTime = 0.1f;
    public float LastGetDamageTime;
    public Rigidbody2D rb;
    public Transform ProjectilePrefab;
    public SpriteRenderer HitImpactSprite;
    public Animator Animator;
    public Transform SlashMaskTransform;
}