using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quantum;

public unsafe partial class Unity_PlayerCharacter : MonoBehaviour
{
    [ReadOnly] Animator animator = null;
    private AnimatorOverrideController overrideController = null;

    [SerializeField] GameObject shieldEffect;
    private GameObject skin_character = null;
    private Unity_Skin_Character skinInfo_character = null;
    private GameObject skin_weapon = null;
    private Unity_Skin_Weapon skinInfo_weapon = null;

    [ReadOnly] public EntityRef enityRef;
    [ReadOnly] public string PID;
    private int characterSkinID = -1;
    private int weaponSkinID = -1;

    private Frame frame = null;

    private const string ANIM_BOOL_IDLE = "isIdle";
    private const string ANIM_BOOL_RUN = "isRun";
    private const string ANIM_TRIGGER_DIE = "isDie";
    private const string ANIM_TRIGGER_JUMP_FIRST = "isJump_First";
    private const string ANIM_TRIGGER_JUMP_SECOND = "isJump_Second";
    private const string ANIM_TRIGGER_ATTACK_ATTEMP = "isAttackAttemp";
    private const string ANIM_TRIGGER_ATTACK_0 = "isAttack_0";
    private const string ANIM_TRIGGER_ATTACK_1= "isAttack_1";
    private const string ANIM_TRIGGER_ATTACK_2 = "isAttack_2";
    private const string ANIM_TRIGGER_HIT = "isHit";
    private const string ANIM_TRIGGER_WIN = "isWin";
    private const string ANIM_TRIGGER_LOSE = "isLose";
    private const string ANIM_TRIGGER_DASH = "isDash";

    private const int ANIM_LAYER_BASE = 0;
    private const int ANIM_LAYER_ATTACK = 1;

    private const string ANIM_CLIP_ATTACK_0_NAME = "Attack0";
    private const string ANIM_CLIP_ATTACK_1_NAME = "Attack1";
    private const string ANIM_CLIP_ATTACK_2_NAME = "Attack2";

    List<AnimationClip> allAnimationClip = new List<AnimationClip>();

    public const string ANIM_STATE_IDLE = "idle";
    public const string ANIM_STATE_RUN = "run";
    public const string ANIM_STATE_HIT = "hit";
    public const string ANIM_STATE_DIE = "die";
    public const string ANIM_STATE_JUMP_FIRST = "JumpFirst";
    public const string ANIM_STATE_JUMP_SECOND = "JumpSecond";
    public const string ANIM_STATE_WIN = "Win";
    public const string ANIM_STATE_LOSE = "Lose";
    public const string ANIM_STATE_ATTACK_WAIT = "AttackWait";
    public const string ANIM_STATE_ATTACK_0 = "Attack0";
    public const string ANIM_STATE_ATTACK_1 = "Attack1";
    public const string ANIM_STATE_ATTACK_2 = "Attack2";

    public enum AnimState 
    {
        None, 
        Idle, 
        Run, 
        Die, 
        Jump_First, 
        Jump_Second, 
        Attack_Attemp,
        Attack_0, 
        Attack_1, 
        Attack_2, 
        Hit, 
        Win,
        Lose,
        Dash,
    }

    public enum CharacterSceneType { None, Ingame, Outgame}
    public CharacterSceneType CurrentCharType { get; set; } = CharacterSceneType.None;

    public PlayerRules* playerRules = null;


    public enum WeaponPosition
    { 
        None,
        Waist,
        Back,
        Hand_Right,
        Hand_Left,
    }

    private class ActivatedSkillInfo
    {
        public GameObject effectObj;
        public Unity_Effect effectScript;
        public Quantum.PlayerActiveSkill type;
    }
    private List<ActivatedSkillInfo> activatedSkillInfoList = new List<ActivatedSkillInfo>();

    private void Awake()
    {
        enityRef = EntityRef.None;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            var ac = animator.runtimeAnimatorController;
            allAnimationClip.AddRange(ac.animationClips);

            overrideController = new AnimatorOverrideController(ac);
        }
    }

    private void OnEnable()
    {
        frame = NetworkManager_Client.Instance.GetFrameVerified();

        QuantumEvent.Subscribe<EventPlayerEvents>(this, OnEventPlayerEvents);
    }

    private void OnDisable()
    {
        frame = null;

        QuantumEvent.UnsubscribeListener<EventPlayerEvents>(this);
    }

    public void SetData_Ingame(EntityRef _enityRef, string _PID, int _characterSkinID, int _weaponSkinID)
    {
        CurrentCharType = CharacterSceneType.Ingame;

        enityRef = _enityRef;
        PID = _PID;
        characterSkinID = _characterSkinID;
        weaponSkinID = _weaponSkinID;

        SetSkin(characterSkinID, weaponSkinID, WeaponPosition.Waist);
        skinInfo_character?.ChangeLayer(Unity_Skin_Character.LayerType.Player);
        skinInfo_weapon?.ChangeLayer(Unity_Skin_Weapon.LayerType.Player);
    }

    public void SetData_OutGame()
    {
        CurrentCharType = CharacterSceneType.Outgame;

        var data = AccountManager.Instance.AccountData;
        if (data != null)
        {
            characterSkinID = data.characterSkinID;
            weaponSkinID = data.weaponSkinID;

            SetSkin(characterSkinID, weaponSkinID, WeaponPosition.Hand_Right);
            skinInfo_character?.ChangeLayer(Unity_Skin_Character.LayerType.Default);
            skinInfo_weapon?.ChangeLayer(Unity_Skin_Weapon.LayerType.Default);
        }
    }

    public void SetData_RenderTexture(int _characterSkinID = -1, int _weaponSkinID = -1)
    {
        CurrentCharType = CharacterSceneType.Outgame;

        var data = AccountManager.Instance.AccountData;
        if (data != null)
        {
            if (_characterSkinID == -1)
                characterSkinID = data.characterSkinID;
            else
                characterSkinID = _characterSkinID;

            if (_weaponSkinID == -1)
                weaponSkinID = data.weaponSkinID;
            else
                weaponSkinID = _weaponSkinID;

            SetSkin(characterSkinID, weaponSkinID, WeaponPosition.Waist);
        }
    }

    public unsafe void Update()
    {
        if (CurrentCharType == CharacterSceneType.Ingame)
        {
            if (enityRef != EntityRef.None)
            {
                if (frame != null)
                {
                    if (frame.Unsafe.TryGetPointer<PlayerRules>(enityRef, out PlayerRules* pr))
                    {
                        playerRules = pr;

                        if (pr->isGrounded)
                        {
                            if (pr->isIdle)
                            {
                                ForceAnimState(AnimState.Idle);
                            }

                            if (pr->isRunning)
                            {
                                ForceAnimState(AnimState.Run);
                            }

                            if (pr->isDead)
                            {
                                animator?.SetBool(ANIM_BOOL_IDLE, false);
                                animator?.SetBool(ANIM_BOOL_RUN, false);
                            }
                        }
                        else
                        {
                            animator?.SetBool(ANIM_BOOL_IDLE, false);
                            animator?.SetBool(ANIM_BOOL_RUN, false);
                        }

                        if (pr->isAttacking == false)
                        {
                            if (shieldEffect.SafeIsActive())
                                shieldEffect?.SafeSetActive(false);
                        }

                        if (pr->IsAlive)
                        {
                            if (IsAnimationPlaying(ANIM_CLIP_ATTACK_0_NAME, ANIM_LAYER_ATTACK) || IsAnimationPlaying(ANIM_CLIP_ATTACK_1_NAME, ANIM_LAYER_ATTACK) || IsAnimationPlaying(ANIM_CLIP_ATTACK_2_NAME, ANIM_LAYER_ATTACK))
                                SetWeaponPositon(WeaponPosition.Hand_Right);
                            else
                                SetWeaponPositon(WeaponPosition.Waist);
                        }
                        else
                        {
                            SetWeaponPositon(WeaponPosition.None);
                        }
                    }

                    //타겟의 경우 Skin Color 바꿔서 표시해주자
                    if (InGame_Quantum.Instance.ball != null
                        && frame.Unsafe.TryGetPointer<BallRules>(InGame_Quantum.Instance.ball.enityRef, out BallRules* br))
                    {
                        if (skinInfo_character != null && playerRules != null
                            && br->TargetEntity == playerRules->SelfEntity
                            && frame.Global->gamePlayState == GamePlayState.Play)
                        {
                            skinInfo_character.ChangeSkinColor(Unity_Skin_Character.SkinColor.Red);
                        }
                        else
                        {
                            skinInfo_character.ChangeSkinColor(Unity_Skin_Character.SkinColor.Normal);
                        }
                    }
                }
            }
        }
    }

    private unsafe void OnEventPlayerEvents(EventPlayerEvents _event)
    {
        if (_event.Entity.Equals(enityRef) == false)
            return;

        switch (_event.PlayerEvent)
        {
            case PlayerEvent.Event_Jump_First:
                {
                    ForceAnimState(AnimState.Jump_First);
                }
                break;

            case PlayerEvent.Event_Jump_Second:
                {
                    ForceAnimState(AnimState.Jump_Second);
                }
                break;

            case PlayerEvent.Event_AttackAttempt:
                {
                    ForceAnimState(AnimState.Attack_Attemp);

                    shieldEffect?.SafeSetActive(false);
                    shieldEffect?.SafeSetActive(true);
                }
                break;

            case PlayerEvent.Event_AttackSuccess:
                {
                    ForceAnimState(AnimState.Attack_0);

                    shieldEffect?.SafeSetActive(false);

                    var pos = transform.position + UnityEngine.Vector3.up * 1f;
                    var fowardRot = Quaternion.LookRotation(transform.forward);
                    var rot = Quaternion.Euler(Random.Range(-40f, 40f), fowardRot.y, 0f);
                    InGameManager.Instance.ActivatePooledObj(InGameManager.PooledType.Effect_Slash, pos, rot, 1f, this.transform);

                    if (skinInfo_weapon != null)
                    {
                        var weaponPos = skinInfo_weapon.transform.position;
                        InGameManager.Instance.ActivatePooledObj(InGameManager.PooledType.Effect_SwordHitBlueCritical, weaponPos, Quaternion.identity, 1f, null);
                    }

                    SoundManager.Instance.PlaySound(SoundManager.SoundClip.Ingame_AttackSuccess);
                }
                break;

            case PlayerEvent.Event_Hit:
                {
                    if (frame != null && frame.Unsafe.TryGetPointer<PlayerRules>(enityRef, out PlayerRules* pr))
                    {
                        if (pr->isGrounded)
                            ForceAnimState(AnimState.Hit);
                    }

                    InGameManager.Instance.ActivatePooledObj(InGameManager.PooledType.Effect_PickupExplodeYellow, transform.position, Quaternion.identity, 1f, this.transform);
                }
                break;

            case PlayerEvent.Event_Die:
                {
                    ForceAnimState(AnimState.Die);

                    shieldEffect?.SafeSetActive(false);

                    InGameManager.Instance.ActivatePooledObj(InGameManager.PooledType.Effect_SoulGenericDeath, transform.position, Quaternion.identity, 1f, this.transform);

                    var ingameUI = PrefabManager.Instance.UI_InGame;

                    if (InGame_Quantum.Instance.myPlayer != null
                        && enityRef.Equals(InGame_Quantum.Instance.myPlayer.entityRef)
                        && InGame_Quantum.Instance.GamePlayState == GamePlayState.Play)
                    {
                        ingameUI.ActiavteGamePlayUI_Dead();
                    }
                       
                    var playerInfo = InGame_Quantum.Instance.GetPlayerInfo(enityRef);
                    if (playerInfo != null
                        && InGame_Quantum.Instance.GamePlayState == GamePlayState.Play)
                    {
                        ingameUI.ActivateIngameMessage_Localized("INGAME_ELIMINATED", arg : playerInfo.nickname);
                    }
                }
                break;

            case PlayerEvent.Event_Skill_Active_Dash:
                {
                    ForceAnimState(AnimState.Dash);
                    InGameManager.Instance.ActivatePooledObj(InGameManager.PooledType.Effect_MagicBuffYellow, transform.position + Vector3.up, Quaternion.LookRotation(-transform.forward), 1f, this.transform);
                }
                break;

            case PlayerEvent.Event_Skill_Active_FreezeBall:
                {

                }
                break;


            case PlayerEvent.Event_Skill_Register_FastBall:
                {
                    InGameManager.Instance.ActivatePooledObj(InGameManager.PooledType.Effect_MagicEnchantYellow, transform.position + Vector3.up, Quaternion.identity, 1f, this.transform);
                }
                break;
            case PlayerEvent.Event_Skill_Active_FastBall:
                {
                    InGameManager.Instance.ActivatePooledObj(InGameManager.PooledType.Effect_SwordWaveWhite, transform.position, Quaternion.identity, 1f, null);
                }
                break;

            case PlayerEvent.Event_Skill_Active_Shield:
                {
                    //혹시나 기존 effect가 남아 있으면 중독 되지 않게... 제거해주자
                    if (activatedSkillInfoList != null & activatedSkillInfoList.Count > 0)
                    {
                        foreach (var i in activatedSkillInfoList)
                        {
                            if (i != null && i.effectObj != null && i.type == PlayerActiveSkill.Shield)
                            {
                                if (i.effectScript != null)
                                    i.effectScript.Hide();
                                else
                                    i.effectObj.SafeSetActive(false);
                            }
                        }
                        activatedSkillInfoList.RemoveAll(x => x.type == PlayerActiveSkill.Shield);
                    }

                    var shieldeffect = InGameManager.Instance.ActivatePooledObj(InGameManager.PooledType.Effect_ShieldSoftPurple, transform.position, Quaternion.identity, 1f, this.transform);
                    activatedSkillInfoList.Add(new ActivatedSkillInfo() { effectObj = shieldeffect, effectScript = shieldeffect.GetComponent<Unity_Effect>(), type = PlayerActiveSkill.Shield } );
                }
                break;

            case PlayerEvent.Event_Skill_Deactive_Shield:
                {
                    if (activatedSkillInfoList != null & activatedSkillInfoList.Count > 0)
                    {
                        foreach (var i in activatedSkillInfoList)
                        {
                            if (i != null && i.effectObj != null && i.type == PlayerActiveSkill.Shield)
                            {
                                if (i.effectScript != null)
                                    i.effectScript.Hide();
                                else
                                    i.effectObj.SafeSetActive(false);
                            }
                        }
                        activatedSkillInfoList.RemoveAll(x => x.type == PlayerActiveSkill.Shield);
                    }
                }
                break;

            case PlayerEvent.Event_Skill_Active_TakeBallTarget:
                {
                    InGameManager.Instance.ActivatePooledObj(InGameManager.PooledType.Effect_MagicCircleSimpleYellow, transform.position, Quaternion.Euler(-90f, 0f, 0f), 1f, this.transform);
                    var laser = InGameManager.Instance.ActivatePooledObj(InGameManager.PooledType.Effect_LaserBeamYellow, transform.position, Quaternion.Euler(-90f, 0f, 0f), 1f, this.transform);
                    laser.TryGetComponent<Unity_Effect_Laser>(out var laserScript);
                    if (laserScript && InGame_Quantum.Instance.ball != null && InGame_Quantum.Instance.ball.enityGameObj != null)
                    {
                        laserScript.Setup(this.transform, InGame_Quantum.Instance.ball.enityGameObj.transform);
                    }
                }
                break;

            case PlayerEvent.Event_Skill_Active_BlindZone:
                {

                }
                break;

            case PlayerEvent.Event_Skill_Active_ChangeBallTarget:
                {
                    var laser = InGameManager.Instance.ActivatePooledObj(InGameManager.PooledType.Effect_LaserBeamRed, transform.position, Quaternion.Euler(-90f, 0f, 0f), 1f, this.transform);
                    laser.TryGetComponent<Unity_Effect_Laser>(out var laserScript);
                    if (laserScript && InGame_Quantum.Instance.ball != null && InGame_Quantum.Instance.ball.enityGameObj != null)
                    {
                        laserScript.Setup(this.transform, InGame_Quantum.Instance.ball.enityGameObj.transform);
                    }
                }
                break;

            case PlayerEvent.Event_Skill_Register_CurveBall:
                {

                }
                break;

            case PlayerEvent.Event_Skill_Active_CurveBall:
                {

                }
                break;

            case PlayerEvent.Event_Skill_Register_SkyRocketBall:
                { 
                
                }
                break;

            case PlayerEvent.Event_Skill_Active_SkyRocketBall:
                {

                }
                break;


        }
    }

    public void ForceAnimState(AnimState type)
    {
        if(animator == null)
            animator = GetComponentInChildren<Animator>();

        if (type != AnimState.Idle && type != AnimState.Run)
        {
            animator?.ResetTrigger(ANIM_TRIGGER_DIE);
            animator?.ResetTrigger(ANIM_TRIGGER_JUMP_FIRST);
            animator?.ResetTrigger(ANIM_TRIGGER_JUMP_SECOND);
            animator?.ResetTrigger(ANIM_TRIGGER_ATTACK_ATTEMP);
            animator?.ResetTrigger(ANIM_TRIGGER_ATTACK_0);
            animator?.ResetTrigger(ANIM_TRIGGER_ATTACK_1);
            animator?.ResetTrigger(ANIM_TRIGGER_ATTACK_2);
            animator?.ResetTrigger(ANIM_TRIGGER_HIT);
            animator?.ResetTrigger(ANIM_TRIGGER_WIN);
            animator?.ResetTrigger(ANIM_TRIGGER_LOSE);
            animator?.ResetTrigger(ANIM_TRIGGER_DASH);
        }

        switch (type)
        {
            case AnimState.None:
                break;

            case AnimState.Idle:
                {
                    animator?.SetBool(ANIM_BOOL_IDLE, true);
                    animator?.SetBool(ANIM_BOOL_RUN, false);
                }
                break;
            case AnimState.Run:
                {
                    animator?.SetBool(ANIM_BOOL_IDLE, false);
                    animator?.SetBool(ANIM_BOOL_RUN, true);
                }
                break;
            case AnimState.Die:
                {
                    animator?.SetBool(ANIM_BOOL_IDLE, false);
                    animator?.SetBool(ANIM_BOOL_RUN, false);
                    animator?.SetTrigger(ANIM_TRIGGER_DIE);
                }
                break;
            case AnimState.Jump_First:
                {
                    animator?.SetBool(ANIM_BOOL_IDLE, false);
                    animator?.SetBool(ANIM_BOOL_RUN, false);
                    animator?.SetTrigger(ANIM_TRIGGER_JUMP_FIRST);
                }
                break;
            case AnimState.Jump_Second:
                {
                    animator?.SetBool(ANIM_BOOL_IDLE, false);
                    animator?.SetBool(ANIM_BOOL_RUN, false);
                    animator?.SetTrigger(ANIM_TRIGGER_JUMP_SECOND);
                }
                break;
            case AnimState.Attack_Attemp:
                {
                    //animator?.SetTrigger(ANIM_TRIGGER_ATTACK_ATTEMP);
                }
                break;
            case AnimState.Attack_0:
                {
                    animator?.SetTrigger(ANIM_TRIGGER_ATTACK_0);
                }
                break;
            case AnimState.Attack_1:
                {
                    animator?.SetTrigger(ANIM_TRIGGER_ATTACK_1);
                }
                break;
            case AnimState.Attack_2:
                {
                    animator?.SetTrigger(ANIM_TRIGGER_ATTACK_2);
                }
                break;
            case AnimState.Hit:
                {
                    animator?.SetTrigger(ANIM_TRIGGER_HIT);
                }
                break;
            case AnimState.Win:
                {
                    animator?.SetBool(ANIM_BOOL_IDLE, false);
                    animator?.SetTrigger(ANIM_TRIGGER_WIN);
                }
                break;
            case AnimState.Lose:
                {
                    animator?.SetBool(ANIM_BOOL_IDLE, false);
                    animator?.SetTrigger(ANIM_TRIGGER_LOSE);
                }
                break;
            case AnimState.Dash:
                {
                    animator?.SetTrigger(ANIM_TRIGGER_DASH);
                }
                break;
        }
   
    }

    private void SetSkin(int characterSkinID = -1, int weaponSkinID = -1, WeaponPosition weaponPosition = WeaponPosition.Waist)
    {
        if (skin_character != null)
            Destroy(skin_character.gameObject);

        if (skin_weapon != null)
            Destroy(skin_weapon.gameObject);

        var charlist = ResourceManager.Instance.CharacterSkinDataList;
        var charSkin = charlist.Find(x => x.id.Equals(characterSkinID));
        if (charSkin != null)
        {
            skin_character = GameObject.Instantiate(charSkin.obj);
            skin_character.transform.SetParent(this.transform);
            skin_character.transform.localPosition = Vector3.zero;
            skin_character.transform.localRotation = Quaternion.Euler(Vector3.zero);
            skinInfo_character = skin_character.GetComponent<Unity_Skin_Character>();

            animator = GetComponentInChildren<Animator>();
            animator?.SetBool(ANIM_BOOL_IDLE, true);
            animator?.SetBool(ANIM_BOOL_RUN, false);
        }

        var weaponlist = ResourceManager.Instance.WeaponSkinDataList;
        var weaponSkin = weaponlist.Find(x => x.id.Equals(weaponSkinID));
        if (weaponSkin != null)
        {
            skin_weapon = GameObject.Instantiate(weaponSkin.obj);
            skin_weapon.transform.SetParent(this.transform);
            skin_weapon.transform.localPosition = Vector3.zero;
            skinInfo_weapon = skin_weapon.GetComponent<Unity_Skin_Weapon>();

            if (skin_character != null)
            {
                if (skinInfo_character != null)
                {
                    SetWeaponPositon(weaponPosition);
                }
            }
        }
    }

    public void SetWeaponPositon(WeaponPosition posi)
    {
        if (skin_weapon == null)
            return;

        switch (posi)
        {
            case WeaponPosition.None:
                {
                    skin_weapon.transform.SafeSetActive(false);
                }
                break;

            case WeaponPosition.Hand_Right: //오른손
                {
                    if (skinInfo_character.rightHandPosition != null)
                    {
                        skin_weapon.transform.SafeSetActive(true);
                        skin_weapon.transform.SetParent(skinInfo_character.rightHandPosition);
                        skin_weapon.transform.localPosition = new Vector3(0.028f, -0.109f, -0.011f);
                        skin_weapon.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    }
                }
                break;

            case WeaponPosition.Hand_Left: //왼손
                {
                    if (skinInfo_character.leftHandPosition != null)
                    {
                        skin_weapon.transform.SafeSetActive(true);
                        skin_weapon.transform.SetParent(skinInfo_character.leftHandPosition);
                        skin_weapon.transform.localPosition = new Vector3(0.029f, -0.049f, -0.012f);
                        skin_weapon.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    }
                }
                break;

            case WeaponPosition.Waist: //허리
                {
                    if (skinInfo_character.waistPosition != null)
                    {
                        skin_weapon.transform.SafeSetActive(true);
                        skin_weapon.transform.SetParent(skinInfo_character.waistPosition);
                        skin_weapon.transform.localPosition = new Vector3(0.043f, -0.03f, -0.179f);
                        skin_weapon.transform.localRotation = Quaternion.Euler(-182.316f, -91.095f, 73.667f);
                    }
                }
                break;

        }
    }

    bool IsAnimationPlaying(string stateName, int layer = 0)
    {
        if (animator == null)
            return false;

        // Check the current state in the specified layer (0 is the default layer)
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);

        // Check if the current state's name matches the provided stateName
        // Note: stateName should match the name in the Animator controller
        return stateInfo.IsName(stateName);
    }

}
