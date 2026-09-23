using System;
using System.Threading;
using System.Threading.Tasks;
using Aiara;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoreMountains.Feedbacks;
using R3;
using Sirenix.OdinInspector;
using UniState;
using Unity.Mathematics;
using UnityEngine;

namespace MyGameNamespace
{
    public class NewCharacterController : MonoBehaviour
    {
        [SerializeField] private Transform body;
        [SerializeField] private float speed = 4;
        [SerializeField] private float speedAnimation = 1;
        [SerializeField] private Animator animator;
        [SerializeField] private Rigidbody rigidbody;
        [SerializeField] private int attackStage = 1;
        [SerializeField] private float attackRange = 3;
        [SerializeField] private float attackTime = 1;
        [SerializeField] private float attackTimeFeedback = 0.2f;
        [SerializeField] private float lockDownDuration = 0.2f;
        [SerializeField] private GameObject fireEffect;
        [SerializeField] private MMFeedbacks attackFeedBack;
        [SerializeField] private MMFeedbacks deathFeedBack;

        bool isLockdownWithFire = false;
        public int Damage = 10;
        public float DamageForce = 1000;
        public int Health = 100;
        public int MaxHealth = 100;
        LockdownState lockdownState;
        NormalState normalState;

        public bool IsCanAttack => normalState != null;

        IStateMachine stateMachine = new StateMachine();

        public class Resolver : ITypeResolver
        {
            public object Resolve(Type type)
            {
                return Activator.CreateInstance(type);
            }
        }

        void Awake()
        {
            stateMachine = new StateMachine();
            Health = MaxHealth;
            stateMachine.SetResolver(new Resolver());
            stateMachine.Execute<NormalState, NewCharacterController>(this, destroyCancellationToken);
        }

        public bool IsDeath() => Health <= 0;
        public void ResetAllAnim()
        {
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Lockdown");
            animator.SetBool("Walking", false);
            animator.SetFloat("RelativeForwardSpeedNormalized", 0);
            animator.SetFloat("RelativeLateralSpeedNormalized", 0);
        }

        public void Push(Vector3 pushForce)
        {
            rigidbody.AddForce(pushForce, ForceMode.Impulse);
        }

        [Button]
        public void Lockdown(bool isLockdownWithFire)
        {
            this.isLockdownWithFire = isLockdownWithFire;
            normalState?.Lockdown();
        }

        public class NormalState : StateBase<NewCharacterController>
        {
            private IDisposable moveSubscription;
            public bool isLockdown = false;
            public override UniTask Initialize(CancellationToken token)
            {
                Payload.normalState = this;
                Payload.ResetAllAnim();
                return base.Initialize(token);
            }
            public override UniTask Exit(CancellationToken token)
            {
                Payload.normalState = null;
                moveSubscription?.Dispose();
                return base.Exit(token);
            }
            public override async UniTask<StateTransitionInfo> Execute(CancellationToken token)
            {
                moveSubscription?.Dispose();
                moveSubscription = Observable
                    .EveryUpdate(UnityFrameProvider.FixedUpdate)
                    .Subscribe(_ =>
                    {
                        float x = Input.GetAxis("Horizontal");
                        float y = Input.GetAxis("Vertical");

                        if (x != 0 && Payload.body != null)
                        {
                            Vector3 scale = Payload.body.localScale;
                            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(x);
                            Payload.body.localScale = scale;
                            Payload.animator.SetFloat("RelativeForwardSpeedNormalized", math.sign(scale.x) * x);
                        }

                        Payload.animator.SetBool("Walking", x != 0 || y != 0);

                        Payload.animator.SetFloat("RelativeLateralSpeedNormalized", y);
                        Payload.animator.SetFloat(
                            "WalkSpeedMultiplier",
                            Payload.speedAnimation * Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)));

                        Vector3 direction = new Vector3(x, 0, y);

                        Payload.rigidbody.MovePosition(
                            Payload.transform.position
                            + direction * Payload.speed * Time.fixedDeltaTime);
                    });

                try
                {
                    // Chờ đến khi bấm attack HOẶC nhân vật chết.
                    await Observable
                        .EveryUpdate(UnityFrameProvider.Update)
                        .FirstAsync(_ =>
                            Payload.IsDeath() ||
                            Input.GetButtonDown("Fire1") || isLockdown);

                    // Ưu tiên chết nếu cùng frame vừa attack vừa hết máu.
                    if (Payload.IsDeath())
                        return Transition.GoTo<DeathState, NewCharacterController>(Payload);


                    //khi bi lock down
                    if (isLockdown)
                        return Transition.GoTo<LockdownState, NewCharacterController>(Payload);

                    return Transition.GoTo<AttackingState, NewCharacterController>(Payload);
                }
                finally
                {
                    // Rời NormalState thì ngừng di chuyển.
                    moveSubscription?.Dispose();
                }
            }

            public void Lockdown()
            {
                isLockdown = true;
            }

        }

        public class LockdownState : StateBase<NewCharacterController>
        {
            public override UniTask Initialize(CancellationToken token)
            {
                Payload.ResetAllAnim();
                Payload.lockdownState = this;
                Payload.animator.SetTrigger("Lockdown");
                Payload.fireEffect.SetActive(Payload.isLockdownWithFire);
                return base.Initialize(token);
            }

            public override UniTask Exit(CancellationToken token)
            {
                Payload.fireEffect.SetActive(false);
                Payload.lockdownState = null;
                return base.Exit(token);
            }

            public override async UniTask<StateTransitionInfo> Execute(CancellationToken token)
            {
                await UniTask.WaitForSeconds(Payload.lockDownDuration);
                return Transition.GoTo<NormalState, NewCharacterController>(Payload);
            }
        }

        public class AttackingState : StateBase<NewCharacterController>
        {
            public override UniTask Initialize(CancellationToken token)
            {
                Payload.ResetAllAnim();
                return base.Initialize(token);
            }

            public override async UniTask<StateTransitionInfo> Execute(CancellationToken token)
            {
                Payload.animator.SetTrigger("Attack");
                Payload.animator.SetInteger("AttackStage", Payload.attackStage);


                DOVirtual.DelayedCall(Payload.attackTimeFeedback, delegate
                        {
                            Payload.attackFeedBack.PlayFeedbacks(Payload.attackFeedBack.transform.position);

                            var hits = Physics.SphereCastAll(Payload.transform.position, Payload.attackRange,
                                                             Payload.body.right, Payload.attackRange, ~0);
                            foreach (var item in hits)
                            {
                                var enemy = item.transform.GetComponentInParent<NewEnemyController>();
                                if (enemy != null)
                                {
                                    enemy.TakeDamage(Payload.Damage);
                                    var dir = enemy.transform.position - Payload.body.position;
                                    dir.y = 0;
                                    enemy.Push(dir * Payload.DamageForce);
                                }
                            }
                        });
                // Attack kéo dài 1 giây, nhưng chết là đổi state ngay.
                float endTime = Time.time + Payload.attackTime;

                await UniTask.WaitUntil(
                    () => Payload.IsDeath() || Time.time >= endTime,
                    cancellationToken: token);

                if (Payload.IsDeath())
                    return Transition.GoTo<DeathState, NewCharacterController>(Payload);

                return Transition.GoTo<NormalState, NewCharacterController>(Payload);
            }
        }

        public class DeathState : StateBase<NewCharacterController>
        {
            public override async UniTask<StateTransitionInfo> Execute(CancellationToken token)
            {
                Payload.animator.SetTrigger("Death");
                await Payload.deathFeedBack.PlayFeedbacksTask();
                return await UniTask.FromResult(Transition.GoToExit());
            }
        }
    }
}
