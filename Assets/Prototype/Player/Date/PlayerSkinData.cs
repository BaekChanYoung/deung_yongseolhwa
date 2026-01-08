using Spine.Unity;
using UnityEngine;
using UnityEngine.U2D.Animation;


[CreateAssetMenu(fileName = "New Skin Date", menuName = "Date/Skin Date")]
public class PlayerSkinData : ScriptableObject
{
    public int serialNumber;

    public SpriteLibraryAsset spriteLibrary;

    public SkeletonDataAsset playSceneSpine;

    [SpineSkin(dataField: "playSceneSpine")]
    public string playSceneSkinName;

    [SpineAnimation(dataField: "playSceneSpine")]
    public string playSceneAnimationName;

    public bool playSceneAnimationLoop;

    public SkeletonDataAsset startSceneSpine;

    [SpineSkin(dataField: "startSceneSpine")]
    public string startSceneSkinName;

    [SpineAnimation(dataField: "playSceneSpine")]
    public string startSceneAnimationName;

    public bool startSceneAnimationLoop;
}

