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
        data = SkinManager.Instance.GetSkinData();
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
            data = SkinManager.Instance.GetSkinData();
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

        // 3. [추가] 애니메이션 재생 로직
        string targetAnimName = "";
        bool isLoop = false;

        // 모드에 따라 데이터에서 애니메이션 정보 가져오기
        if (skinMode == SkinMode.StartScene)
        {
            targetAnimName = data.startSceneAnimationName;
            isLoop = data.startSceneAnimationLoop;
        }
        else
        {
            targetAnimName = data.playSceneAnimationName;
            isLoop = data.playSceneAnimationLoop;
        }

        // 애니메이션 이름이 설정되어 있다면 재생
        if (!string.IsNullOrEmpty(targetAnimName))
        {
            // 0번 트랙에 애니메이션 설정
            controllerSpine.AnimationState.SetAnimation(0, targetAnimName, isLoop);
            //Debug.Log($"[SkinController] Playing Animation: {targetAnimName} (Loop: {isLoop})");
        }
    }
}
