using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Characters
{
    [Image(typeof(IconReaction), ColorTheme.Type.TextLight)]
    
    [Serializable]
    public class ReactionItem : TPolymorphicItem<ReactionItem>
    {
        private const float MIN_MAGNITUDE = 0.1f;
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private EnablerFloat m_MinForce = new EnablerFloat(false, 1f);
        [SerializeField] private ReactionDirection m_Direction = ReactionDirection.FromAny;
        
        [SerializeField] private RunConditionsList m_Conditions = new RunConditionsList();

        [SerializeField] private ReactionAnimations m_Animations = new ReactionAnimations();
        [SerializeField] private AvatarMask m_AvatarMask;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public AnimationClip AnimationClip => this.m_Animations.AnimationClip;
        public AvatarMask AvatarMask => this.m_AvatarMask;

        public override string Title
        {
            get
            {
                string direction = TextUtils.Humanize(this.m_Direction).ToLower();
                direction = this.m_Direction == ReactionDirection.FromAny
                    ? $"{char.ToUpper(direction[0])}{direction[1..]} direction"
                    : $"{char.ToUpper(direction[0])}{direction[1..]}";

                string force = this.m_MinForce.IsEnabled
                    ? $" with Force > {this.m_MinForce.Value}"
                    : "";

                string conditions = this.m_Conditions.ToString();
                conditions = !string.IsNullOrEmpty(conditions)
                    ? $" and {char.ToLower(conditions[0])}{conditions[1..]}"
                    : conditions;

                return $"{direction}{force}{conditions}";
            }
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool CheckForce(float force) => !this.m_MinForce.IsEnabled || this.m_MinForce.Value >= force;

        public bool CheckDirection(Vector3 direction)
        {
            return this.m_Direction switch
            {
                ReactionDirection.FromAny => true,
                ReactionDirection.FromLeft => direction.x >= 0.5f,
                ReactionDirection.FromRight => direction.x <= -0.5f,
                ReactionDirection.FromFront => direction.z <= -0.5f,
                ReactionDirection.FromBack => direction.z >= 0.5f,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public bool CheckConditions(Args args) => this.m_Conditions.Check(args);
    }
}