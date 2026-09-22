using System;
using System.Threading;
using Aiara;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using MoreMountains.Feedbacks;
using R3;
using UniState;
using Unity.Mathematics;
using UnityEngine;
using Yeolha.BeltScroll;


namespace MyGameNamespace
{
    public class NewEnemyController : MonoBehaviour
    {

        [SerializeField] private Transform body;
        [SerializeField] private float speed = 4;
        [SerializeField] private float speedAnimation = 1;
        [SerializeField] private Animator animator;
        [SerializeField] private Rigidbody rigidbody;
        [SerializeField] private Transform focusPoint;
        [SerializeField] private int attackStage = 1;
        [SerializeField] private float attackTime = 1;
        [SerializeField] private float attackTimeFeedback = 0.2f;
        [SerializeField] private float attackDelay = 0.2f;
        public int Health = 100;
        public int MaxHealth = 100;
        [SerializeField] private AttackingState.CloseAttack CloseAttack;
        [SerializeField] private AttackingState.CloseAttack FarAttack;

        NewCharacterController target;
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
            target = FindAnyObjectByType<NewCharacterController>();
            GameController.Ins.listFocusPoint.Add(focusPoint);
            stateMachine = new StateMachine();
            Health = MaxHealth;
            stateMachine.SetResolver(new Resolver());
            stateMachine.Execute<IntroState, NewEnemyController>(this, destroyCancellationToken);
        }

        public bool IsDeath() => Health <= 0;
        public void ResetAllAnim()
        {
            animator.ResetTrigger("Attack");
            animator.SetBool("Walking", false);
            animator.SetFloat("RelativeForwardSpeedNormalized", 0);
        }
        public class IntroState : StateBase<NewEnemyController>
        {
            public async override UniTask<StateTransitionInfo> Execute(CancellationToken token)
            {
                FeedbackManager.PlayFeedbackAtWorld("Enemy_Prepare_Landing", Payload.transform.position, Payload.transform.rotation);
                await UniTask.WaitForSeconds(1);
                FeedbackManager.PlayFeedbackAtWorld("Enemy_Landing", Payload.transform.position, Payload.transform.rotation);
                await UniTask.WaitForSeconds(2);
                return Transition.GoTo<NormalState, NewEnemyController>(Payload);
            }
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

                bool isCanAttack = false;
                moveSubscription?.Dispose();
                moveSubscription = Observable
                    .EveryUpdate(UnityFrameProvider.FixedUpdate)
                    .Subscribe(_ =>
                    {
                        // float x = Input.GetAxis("Horizontal");
                        // float y = Input.GetAxis("Vertical");
                        float x = 0;
                        float y = 0;

                        var attackRangeMin = Mathf.Min(Payload.CloseAttack.range, Payload.FarAttack.range);
                        var attackRangeMax = Mathf.Min(Payload.CloseAttack.range, Payload.FarAttack.range);

                        if (Vector3.Distance(Payload.transform.position, Payload.target.transform.position) >= attackRangeMax)//move toi tam co the danh
                        {
                            x = -math.sign(Payload.transform.position.x - Payload.target.transform.position.x);
                        }

                        else
                            isCanAttack = true;

                        if (x != 0 && Payload.body != null)
                        {
                            Vector3 scale = Payload.body.localScale;
                            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(x);
                            Payload.body.localScale = scale;
                            Payload.animator.SetFloat("RelativeForwardSpeedNormalized", math.sign(scale.x) * x);
                        }

                        // y = math.sign(Payload.transform.position.y - Payload.target.transform.position.y);

                        Payload.animator.SetBool("Walking", x != 0 || y != 0);
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
                            Payload.IsDeath() || isCanAttack ||
                            Input.GetButtonDown("Fire1"));

                    // Ưu tiên chết nếu cùng frame vừa attack vừa hết máu.
                    if (Payload.IsDeath())
                        return Transition.GoTo<DeathState, NewEnemyController>(Payload);

                    //attack
                    if (isCanAttack)
                        return Transition.GoTo<AttackingState, NewEnemyController>(Payload);

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

                //danh gan
                if (Vector3.Distance(Payload.transform.position, Payload.target.transform.position) <= Payload.CloseAttack.range)//move toi tam co the danh
                {
                    await Payload.CloseAttack.AttackAction();
                }
                //danh xa
                else if (Vector3.Distance(Payload.transform.position, Payload.target.transform.position) <= Payload.FarAttack.range)//move toi tam co the danh
                {
                    await Payload.FarAttack.AttackAction();
                }

                return Transition.GoTo<NormalState, NewEnemyController>(Payload);
            }


            [System.Serializable]
            public class CloseAttack
            {
                public float range = 3;
                public float waitTime = 1f;
                [SerializeField] private MMF_Player MMF_PlayerFeedBack;
                public async UniTask AttackAction()
                {
                    await MMF_PlayerFeedBack.PlayFeedbacksTask();
                    await UniTask.WaitForSeconds(waitTime);
                }
            }

            [System.Serializable]
            public class FarAttack
            {
                public float range = 6;
                public float waitTime = 0.5f;

                [SerializeField] private MMF_Player MMF_PlayerFeedBack;
                public async UniTask AttackAction()
                {
                    await MMF_PlayerFeedBack.PlayFeedbacksTask();
                    await UniTask.WaitForSeconds(waitTime);
                }
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
