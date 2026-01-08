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
    SkinMode skinMode;

    void Start()
    {
        data = PlayerDataManager.instance.GetSkinData();
        SpineChange();
    }

    void Update()
    {
        
    }
    void LateUpdate()
    {
        if(PlayerDataManager.instance.IsChangeSkin)
        {
            PlayerDataManager.instance.IsChangeSkin = false;
            data = PlayerDataManager.instance.GetSkinData();
            SpineChange();
        }
    }

    void SpineChange()
    {
        switch(skinMode)
        {
            case SkinMode.StartScene:
                controllerSpine.skeletonDataAsset = data.startSceneSpine;
                break;
            case SkinMode.GameScene:
                controllerSpine.skeletonDataAsset = data.playSceneSpine;
                break;
        }

        controllerSpine.Initialize(true);

        string targetSkinName = (skinMode == SkinMode.StartScene) ? data.startSceneSkinName : data.playSceneSkinName;

        if (!string.IsNullOrEmpty(targetSkinName))
        {
            // 실제 스파인 뼈대에 스킨 적용
            controllerSpine.Skeleton.SetSkin(targetSkinName);
            controllerSpine.Skeleton.SetSlotsToSetupPose(); // 슬롯을 초기 상태로 정렬(이미지 변경 핵심)
            controllerSpine.AnimationState.Apply(controllerSpine.Skeleton); // 상태 업데이트
        }

        // 기본 애니메이션 재생
        //controllerSpine.AnimationState.SetAnimation(0, "idle", true);
    }
}
