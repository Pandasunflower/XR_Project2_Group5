#if GPU_INSTANCER
using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancer.CrowdAnimations
{
    public class GPUIMecanimAnimator : GPUIAnimatorWorkflow
    {
        public GPUIAnimatorState animatorState;
        
        public GPUIMecanimAnimator()
        {
            activeClipCount = 0;
            currentAnimationClipData = new GPUIAnimationClipData[4];
            currentAnimationClipDataWeights = Vector4.zero;
        }

        public void UpdateDataFromMecanimAnimator(GPUICrowdRuntimeData runtimeData, int arrayIndex, Animator animatorRef, List<AnimatorClipInfo> animatorClipInfos)
        {
            bool readAnimations = false;
            AnimatorStateInfo stateInfo = animatorRef.GetCurrentAnimatorStateInfo(0);
            int hashCode = stateInfo.fullPathHash;
            if (activeClipCount == 0 || hashCode != animatorState.hashCode)
            {
                if (!runtimeData.animatorStateDict.TryGetValue(hashCode, out animatorState))
                {
                    Debug.Log("GPUI Mecanim Animator can not find state: " + stateInfo.ToString());
                    return;
                }
                readAnimations = true;
            }
            if (readAnimations || animatorState.isBlend)
            {
                animatorRef.GetCurrentAnimatorClipInfo(0, animatorClipInfos);
                activeClipCount = animatorClipInfos.Count > 4 ? 4 : animatorClipInfos.Count;
                for (int i = 0; i < 4; i++)
                {
                    if (i < activeClipCount)
                    {
                        AnimatorClipInfo clipInfo = animatorClipInfos[i];
                        currentAnimationClipData[i] = runtimeData.animationClipDataDict[clipInfo.clip.GetHashCode()];
                        currentAnimationClipDataWeights[i] = clipInfo.weight;
                    }
                    else
                        currentAnimationClipDataWeights[i] = 0;
                }
            }

            float stateTime = stateInfo.normalizedTime + animatorState.cycleOffset;
            int index = arrayIndex * 2;
            Vector4 animationData = runtimeData.animationData[index];
            for (int i = 0; i < activeClipCount; i++)
            {
                GPUIAnimationClipData animationClipData = currentAnimationClipData[i];
                if (!animationClipData.IsClipLooping() && stateTime >= 1)
                {
                    animationData[i] = animationClipData.clipStartFrame + animationClipData.clipFrameCount - 1;
                }
                else
                {
                    animationData[i] = animationClipData.clipStartFrame + (animationClipData.clipFrameCount - 1) * (stateTime % 1.0f);
                }
            }

            if (animatorRef.IsInTransition(0) && activeClipCount < 4)
            {
                stateInfo = animatorRef.GetNextAnimatorStateInfo(0);
                hashCode = stateInfo.fullPathHash;
                if (!runtimeData.animatorStateDict.TryGetValue(hashCode, out animatorState))
                {
                    Debug.Log("GPUI Mecanim Animator can not find state: " + stateInfo.ToString());
                    return;
                }
                animatorRef.GetNextAnimatorClipInfo(0, animatorClipInfos);
                int transitioningClipCount = animatorClipInfos.Count;
                for (int i = 0; i < transitioningClipCount; i++)
                {
                    if (i + activeClipCount >= 4)
                        break;
                    AnimatorClipInfo clipInfo = animatorClipInfos[i];
                    currentAnimationClipData[activeClipCount + i] = runtimeData.animationClipDataDict[clipInfo.clip.GetHashCode()];
                    currentAnimationClipDataWeights[activeClipCount + i] = clipInfo.weight;
                }

                int activeStateClipCount = activeClipCount;
                activeClipCount = activeClipCount + animatorClipInfos.Count;
                if (activeClipCount > 4)
                    activeClipCount = 4;

                stateTime = stateInfo.normalizedTime + animatorState.cycleOffset;
                for (int i = activeStateClipCount; i < activeClipCount; i++)
                {
                    GPUIAnimationClipData animationClipData = currentAnimationClipData[i];
                    if (!animationClipData.IsClipLooping() && stateTime >= 1)
                    {
                        animationData[i] = animationClipData.clipStartFrame + animationClipData.clipFrameCount - 1;
                    }
                    else
                    {
                        animationData[i] = animationClipData.clipStartFrame + (animationClipData.clipFrameCount - 1) * (stateTime % 1.0f);
                    }
                }
            }

            runtimeData.animationData[index] = animationData;
            runtimeData.animationData[index + 1] = currentAnimationClipDataWeights;
        }
    }
}
#endif //GPU_INSTANCER