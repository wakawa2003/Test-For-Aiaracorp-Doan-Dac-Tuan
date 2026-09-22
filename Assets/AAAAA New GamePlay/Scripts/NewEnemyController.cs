using System;
using System.Threading;
using Aiara;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UniState;
using UnityEngine;


namespace MyGameNamespace
{
    public class NewEnemyController : MonoBehaviour
    {
        [SerializeField] private float speed = 4;
        [SerializeField] private float speedAnimation = 1;
        [SerializeField] private Animator animator;
        [SerializeField] private Rigidbody rigidbody;
        [SerializeField] private TuanTool.AnimationEvent animationEvent;
        [SerializeField] private int attackStage = 1;
        [SerializeField] private float attackTime = 1;
        [SerializeField] private float attackTimeFeedback = 0.2f;
        public int Health = 100;
        public int MaxHealth = 100;


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
            stateMachine.Execute<NormalState, NewEnemyController>(this, destroyCancellationToken);
        }

        void Start()
        {
            FeedbackManager.PlayFeedbackAtWorld("Enemy_Landing", transform.position, transform.rotation);
        }

        public bool IsDeath() => Health <= 0;
        public void ResetAllAnim()
        {
            animator.ResetTrigger("Attack");
            animator.SetBool("Walking", false);
            animator.SetFloat("RelativeForwardSpeedNormalized", 0);
        }
        public class NormalState : StateBase<NewEnemyController>
        {
            private IDisposable moveSubscription;
            public override UniTask Initialize(CancellationToken token)
            {
                Payload.ResetAllAnim();
                return base.Initialize(token);
            }
            public override UniTask Exit(CancellationToken token)
            {
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

                        Payload.animator.SetBool("Walking", x != 0 || y != 0);
                        Payload.animator.SetFloat("RelativeForwardSpeedNormalized", x);
                        Payload.animator.SetFloat(
                            "WalkSpeedMultiplier",
                            Payload.speedAnimation);

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
                            Input.GetButtonDown("Fire1"));

                    // Ưu tiên chết nếu cùng frame vừa attack vừa hết máu.
                    if (Payload.IsDeath())
                        return Transition.GoTo<DeathState, NewEnemyController>(Payload);

                    return Transition.GoTo<AttackingState, NewEnemyController>(Payload);
                }
                finally
                {
                    // Rời NormalState thì ngừng di chuyển.
                    moveSubscription?.Dispose();
                }
            }
        }

        public class AttackingState : StateBase<NewEnemyController>
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
                            FeedbackManager.PlayFeedbackAtWorld("Character Attack", Payload.transform.position, Payload.transform.rotation);
                        });
                // Attack kéo dài 1 giây, nhưng chết là đổi state ngay.
                float endTime = Time.time + Payload.attackTime;

                await UniTask.WaitUntil(
                    () => Payload.IsDeath() || Time.time >= endTime,
                    cancellationToken: token);

                if (Payload.IsDeath())
                    return Transition.GoTo<DeathState, NewEnemyController>(Payload);

                return Transition.GoTo<NormalState, NewEnemyController>(Payload);
            }
        }

        public class DeathState : StateBase<NewEnemyController>
        {
            public override UniTask<StateTransitionInfo> Execute(CancellationToken token)
            {
                Payload.animator.SetTrigger("Death");
                return UniTask.FromResult(Transition.GoToExit());
            }
        }
    }
}
