using System;
using System.Threading;
using Aiara;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using MoreMountains.Feedbacks;
using R3;
using R3.Triggers;
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
        [SerializeField] private MMFeedbacks deathFeedBack;
        public int Health = 100;
        public int MaxHealth = 100;
        [SerializeField] private AttackingState.CloseAttack CloseAttack;
        [SerializeField] private AttackingState.FarAttack FarAttack;

        NewCharacterController target;
        IStateMachine stateMachine = new StateMachine();

        public Rigidbody Rigidbody { get => rigidbody; }

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
            stateMachine = new StateMachine();
            Health = MaxHealth;
            stateMachine.SetResolver(new Resolver());
            stateMachine.Execute<IntroState, NewEnemyController>(this, destroyCancellationToken);
        }
        void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, CloseAttack.rangeExplosion);
        }
        public void Push(Vector3 pushForce)
        {
            rigidbody.AddForce(pushForce, ForceMode.Impulse);
        }
        public int TakeDamage(int damage)
        {
            Health -= damage;
            Health = Mathf.Max(0, Health);
            return damage;
        }

        public bool IsDeath() => Health <= 0;
        public void ResetAllAnim()
        {
            animator.ResetTrigger("Attack");
            animator.SetBool("Walking", false);
            animator.SetBool("Jumping", false);
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
                        var attackRangeMax = Mathf.Max(Payload.CloseAttack.range, Payload.FarAttack.range);

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
                            Payload.IsDeath() || (isCanAttack && Payload.target.IsCanAttack) ||
                            Input.GetButtonDown("Fire1"));

                    // Ưu tiên chết nếu cùng frame vừa attack vừa hết máu.
                    if (Payload.IsDeath())
                        return Transition.GoTo<DeathState, NewEnemyController>(Payload);

                    //attack
                    if (isCanAttack)
                        return Transition.GoTo<AttackingState, NewEnemyController>(Payload);

                    return Transition.GoTo<NormalState, NewEnemyController>(Payload);
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

                else
                {
                    await UniTask.WaitForSeconds(1);//doi de tranh loop
                }

                return Transition.GoTo<NormalState, NewEnemyController>(Payload);
            }


            [System.Serializable]
            public class CloseAttack
            {
                public float range = 3;
                public float rangeExplosion = 5;
                public float forceToVictim = 1000;
                public float waitTime = 1f;
                public bool IsAttacking;
                [SerializeField] private MMF_Player MMF_PlayerFeedBack;
                [SerializeField] private NewEnemyController newEnemyController;
                public async UniTask AttackAction()
                {
                    IsAttacking = true;
                    await MMF_PlayerFeedBack.PlayFeedbacksTask();

                    //check gay damage
                    var l = Physics.SphereCastAll(newEnemyController.transform.position, rangeExplosion, Vector3.up);

                    foreach (var item in l)
                    {

                        if (item.transform.GetComponentInParent<NewCharacterController>() != null)
                        {
                            var vectorPush = item.transform.position - newEnemyController.transform.position;
                            vectorPush.y = 0;
                            item.transform.GetComponentInParent<NewCharacterController>().Push(vectorPush.normalized * forceToVictim);
                            item.transform.GetComponentInParent<NewCharacterController>().Lockdown(true);
                        }
                    }

                    await UniTask.WaitForSeconds(waitTime);
                    IsAttacking = false;
                }
            }

            [System.Serializable]
            public class FarAttack
            {
                public float forceToVictim = 1000;
                public float range = 6;
                public float waitTime = 0.5f;
                public float jumpPower = 0.5f;
                public float jumpSpeedAttackMultiple = 1;
                public float jumpDuration = 0.5f;
                public int numJump = 1;
                public AnimationCurve easeLucLayDa;
                public AnimationCurve easeLanTroLai;
                public AnimationCurve easeJump;
                public AnimationCurve easeSpeedAnimJump;
                public MMF_Player feedBackStartJump;
                public MMF_Player feedBackEndJump;
                [SerializeField] private NewEnemyController newCharacterController;
                [SerializeField] private MMF_Player MMF_PlayerFeedBack;
                Sequence jumpTween;
                public bool IsAttacking;
                IDisposable streamLockDown;
                public async UniTask AttackAction()
                {
                    Debug.Log($"attack far");
                    IsAttacking = true;


                    //check lock down
                    streamLockDown?.Dispose();
                    streamLockDown = newCharacterController.OnCollisionEnterAsObservable().Subscribe(obj =>
                    {
                        if (obj.gameObject.GetComponentInParent<NewCharacterController>() != null)
                        {

                            var vectorPush = obj.gameObject.transform.position - newCharacterController.transform.position;
                            vectorPush.y = 0;
                            obj.gameObject.GetComponentInParent<NewCharacterController>().Push(vectorPush.normalized * forceToVictim);
                            obj.gameObject.GetComponentInParent<NewCharacterController>().Lockdown(false);
                        }
                    });



                    newCharacterController.animator.SetFloat("Jump Side", -math.sign(newCharacterController.target.transform.position.x - newCharacterController.transform.position.x));
                    newCharacterController.ResetAllAnim();
                    newCharacterController.animator.SetBool("Jumping", true);
                    jumpTween?.Kill();

                    // jumpTween.Append(newCharacterController.rigidbody.DOMove(newCharacterController.target.transform.position, jumpDuration).SetEase(easeJump));
                    // jumpTween.Append(newCharacterController.rigidbody.DOMoveY(newCharacterController.target.transform.position.y + jumpPower, jumpDuration).SetEase(easeJump));
                    var viTriBanDau = newCharacterController.transform.position;
                    var viTriLayDa = newCharacterController.transform.position - (newCharacterController.target.transform.position - newCharacterController.transform.position).normalized * 3f;
                    jumpTween = DOTween.Sequence();
                    jumpTween.Append(newCharacterController.rigidbody.DOMove(viTriLayDa, jumpDuration).SetUpdate(UpdateType.Fixed).SetEase(easeLucLayDa));
                    await jumpTween.AsyncWaitForCompletion();//doi den khi lay da xong
                    newCharacterController.animator.SetFloat("Speed Anim Jump", easeSpeedAnimJump.Evaluate(0) * jumpSpeedAttackMultiple);
                    newCharacterController.animator.SetFloat("Jump Side", math.sign(newCharacterController.target.transform.position.x - newCharacterController.transform.position.x));

                    jumpTween = DOTween.Sequence();
                    jumpTween.Append(newCharacterController.rigidbody.DOMove(viTriBanDau, jumpDuration).SetUpdate(UpdateType.Fixed).SetEase(easeLanTroLai));
                    await jumpTween.AsyncWaitForCompletion();//tro ve vi tri ban dau roi moi nhay



                    jumpTween = DOTween.Sequence();
                    var viTriEnd = newCharacterController.target.transform.position + (newCharacterController.target.transform.position - newCharacterController.transform.position).normalized * 1;
                    jumpTween.Append(newCharacterController.rigidbody.DOJump(viTriEnd, jumpPower, numJump, jumpDuration).SetUpdate(UpdateType.Fixed).SetEase(easeJump));
                    // jumpTween = ;
                    jumpTween.Join(DOVirtual.Float(0, 1, jumpDuration, t =>
                    {
                        newCharacterController.animator.SetFloat("Speed Anim Jump", t * jumpSpeedAttackMultiple);
                    }).SetEase(easeSpeedAnimJump));

                    jumpTween.Play();
                    feedBackStartJump.PlayFeedbacks();
                    await jumpTween.AsyncWaitForCompletion();
                    feedBackEndJump.PlayFeedbacks();
                    newCharacterController.animator.SetBool("Jumping", false);
                    streamLockDown?.Dispose();
                    await MMF_PlayerFeedBack.PlayFeedbacksTask();
                    await UniTask.WaitForSeconds(waitTime);
                    IsAttacking = false;
                }
            }
        }

        public class DeathState : StateBase<NewEnemyController>
        {
            public override async UniTask<StateTransitionInfo> Execute(CancellationToken token)
            {
                Payload.animator.SetTrigger("Death");
                Payload.GetComponentInParent<Collider>().enabled = false;
                Payload.GetComponentInParent<Rigidbody>().isKinematic = true;
                await Payload.deathFeedBack.PlayFeedbacksTask();

                return await UniTask.FromResult(Transition.GoToExit());
            }
        }
    }
}
