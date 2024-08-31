using System.Collections;
using GWBase;

public interface IAnimatable {
    IEnumerator PlayAnimation(AnimationSheet animationSheet);
    void StopCurrentAnimation();
    bool IsPlayingAnimation();
    void SetAnimationSpeed(float speed);
    void SetAnimationLooping(bool isLooping);
    AnimationSheet GetAnimation(string animationName);
}