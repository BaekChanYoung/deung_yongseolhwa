using Spine;
using Spine.Unity;
using UnityEngine;

[System.Serializable] 
public enum SkinMode
{
    StartScene,
    GameScene
}

public class SkinController : MonoBehaviour
{
    PlayerSkinData data;

    [SerializeField]
    SkeletonAnimation controllerSpine;

    [SerializeField]
    bool UiSpine = false;

    [SerializeField]
    SkeletonGraphic controllerSpineUi;

    [SerializeField]
    SkinMode skinMode;

    [Header("setting")]

    [SerializeField]
    bool useLateUpdate = true;

    void Start()
    {
        data = SkinManager.Instance.GetSkinData();
        SpineChange();
    }

    void Update()
    {
        
    }
    void LateUpdate()
    {
        if(PlayerDataManager.instance.IsChangeSkin && useLateUpdate)
        {
            PlayerDataManager.instance.IsChangeSkin = false;
            data = SkinManager.Instance.GetSkinData();
            SpineChange();
        }
    }

    public void SpineChange()
    {
        if(UiSpine && controllerSpineUi == null) return;
        if(!UiSpine && controllerSpine == null) return;

        // 1. 공통 데이터 추출 (if문 밖에서 한 번만 처리해서 중복을 없앱니다)
        bool isStartMode = (skinMode == SkinMode.StartScene);
        
        var targetAsset = isStartMode ? data.startSceneSpine : data.playSceneSpine;
        string targetSkinName = isStartMode ? data.startSceneSkinName : data.playSceneSkinName;
        string targetAnimName = isStartMode ? data.startSceneAnimationName : data.playSceneAnimationName;
        bool isLoop = isStartMode ? data.startSceneAnimationLoop : data.playSceneAnimationLoop;

        // 2. 스파인 타입에 맞춰 적용 (UI용인지 일반용인지 여기서 분기)
        if (UiSpine)
        {
            controllerSpineUi.skeletonDataAsset = targetAsset;
            controllerSpineUi.Initialize(true);

            if (!string.IsNullOrEmpty(targetSkinName))
            {
                controllerSpineUi.Skeleton.SetSkin(targetSkinName);
                controllerSpineUi.Skeleton.SetSlotsToSetupPose(); 
                controllerSpineUi.AnimationState.Apply(controllerSpineUi.Skeleton); 
            }

            if (!string.IsNullOrEmpty(targetAnimName))
            {
                controllerSpineUi.AnimationState.SetAnimation(0, targetAnimName, isLoop);
            }
        }
        else
        {
            controllerSpine.skeletonDataAsset = targetAsset;
            controllerSpine.Initialize(true);

            if (!string.IsNullOrEmpty(targetSkinName))
            {
                controllerSpine.Skeleton.SetSkin(targetSkinName);
                controllerSpine.Skeleton.SetSlotsToSetupPose(); 
                controllerSpine.AnimationState.Apply(controllerSpine.Skeleton); 
            }

            if (!string.IsNullOrEmpty(targetAnimName))
            {
                controllerSpine.AnimationState.SetAnimation(0, targetAnimName, isLoop);
            }
        }
    
    }

    public void SpineChange(PlayerSkinData playerdata)
    {
        if(UiSpine && controllerSpineUi == null) return;
        if(!UiSpine && controllerSpine == null) return;

        // 1. 공통 데이터 추출 (if문 밖에서 한 번만 처리해서 중복을 없앱니다)
        bool isStartMode = (skinMode == SkinMode.StartScene);
        
        var targetAsset = isStartMode ? playerdata.startSceneSpine : playerdata.playSceneSpine;
        string targetSkinName = isStartMode ? playerdata.startSceneSkinName : playerdata.playSceneSkinName;
        string targetAnimName = isStartMode ? playerdata.startSceneAnimationName : playerdata.playSceneAnimationName;
        bool isLoop = isStartMode ? playerdata.startSceneAnimationLoop : playerdata.playSceneAnimationLoop;

        // 2. 스파인 타입에 맞춰 적용 (UI용인지 일반용인지 여기서 분기)
        if (UiSpine)
        {
            controllerSpineUi.skeletonDataAsset = targetAsset;
            controllerSpineUi.Initialize(true);

            if (!string.IsNullOrEmpty(targetSkinName))
            {
                controllerSpineUi.Skeleton.SetSkin(targetSkinName);
                controllerSpineUi.Skeleton.SetSlotsToSetupPose(); 
                controllerSpineUi.AnimationState.Apply(controllerSpineUi.Skeleton); 
            }

            if (!string.IsNullOrEmpty(targetAnimName))
            {
                controllerSpineUi.AnimationState.SetAnimation(0, targetAnimName, isLoop);
            }
        }
        else
        {
            controllerSpine.skeletonDataAsset = targetAsset;
            controllerSpine.Initialize(true);

            if (!string.IsNullOrEmpty(targetSkinName))
            {
                controllerSpine.Skeleton.SetSkin(targetSkinName);
                controllerSpine.Skeleton.SetSlotsToSetupPose(); 
                controllerSpine.AnimationState.Apply(controllerSpine.Skeleton); 
            }

            if (!string.IsNullOrEmpty(targetAnimName))
            {
                controllerSpine.AnimationState.SetAnimation(0, targetAnimName, isLoop);
            }
        }
    }

    /// <summary>
    /// 외부에서 스킨 변경을 강제로 지시할 때 사용합니다.
    /// </summary>
    public void ForceUpdateSkin()
    {
        data = SkinManager.Instance.GetSkinData();
        SpineChange();
    }
}
