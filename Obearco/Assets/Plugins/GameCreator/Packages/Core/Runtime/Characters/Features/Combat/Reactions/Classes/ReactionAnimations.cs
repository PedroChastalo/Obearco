using System;
using UnityEngine;

namespace GameCreator.Runtime.Characters
{
    [Serializable]
    public class ReactionAnimations
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
        
        [SerializeField] private AnimationClip[] m_Animations = Array.Empty<AnimationClip>();

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private int m_LastIndex = -1;

        // PROPERTIES: ----------------------------------------------------------------------------

        public AnimationClip AnimationClip
        {
            get
            {
                if (this.m_Animations.Length <= 0) return null;
                if (this.m_Animations.Length == 1) return this.m_Animations[0];

                int index = this.m_LastIndex >= 0
                    ? UnityEngine.Random.Range(0, this.m_Animations.Length - 1)
                    : UnityEngine.Random.Range(0, this.m_Animations.Length);

                if (index == this.m_LastIndex) index += 1;
                this.m_LastIndex = index;
                
                Debug.Log($"Index is {index}");
                return this.m_Animations[index];
            }
        }
    }
}