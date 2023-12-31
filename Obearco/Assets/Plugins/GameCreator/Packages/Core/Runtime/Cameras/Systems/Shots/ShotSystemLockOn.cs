using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Cameras
{
    [Serializable]
    public class ShotSystemLockOn : TShotSystem
    {
        public static readonly int ID = nameof(ShotSystemLockOn).GetHashCode();
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private PropertyGetGameObject m_Anchor = GetGameObjectPlayer.Create();

        [SerializeField]
        private PropertyGetOffset m_AnchorOffset = new PropertyGetOffset(
            new GetOffsetLocalSelf(new Vector3(0f, 0.25f, -1f))
        );
        
        [SerializeField] private PropertyGetDecimal m_Distance = GetDecimalDecimal.Create(5f);

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Vector3 m_AnchorPosition = Vector3.zero;
        [NonSerialized] private float m_Radius;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override int Id => ID;
        
        public GameObject Anchor
        {
            set => this.m_Anchor = GetGameObjectInstance.Create(value);
        }
        
        public Vector3 Offset
        {
            set => this.m_AnchorOffset = GetOffsetLocalSelf.Create(value);
        }
        
        public float Distance
        {
            set => this.m_Distance = GetDecimalDecimal.Create(value);
        }

        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public ShotSystemLockOn()
        { }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public Transform GetAnchorTarget(TShotType shotType)
        {
            GameObject target = this.m_Anchor.Get(shotType.Args);
            return target != null ? target.transform : null;
        }
        
        public void SyncWithZoom(Args args, ShotSystemZoom zoom)
        {
            float distance = (float) this.m_Distance.Get(args);
            float maxRadius = Mathf.Max(0f, distance - zoom.MinDistance);
            
            this.m_Radius = zoom.MinDistance + maxRadius * zoom.Level;
        }

        // IMPLEMENTS: ----------------------------------------------------------------------------

        public override void OnUpdate(TShotType shotType)
        {
            base.OnUpdate(shotType);

            Vector3 positionTarget = this.GetTargetPosition(shotType as TShotTypeLook);
            Vector3 positionAnchor = this.GetAnchorPosition(shotType);

            Vector3 direction = positionTarget - positionAnchor;
            Vector3 position = positionAnchor - direction.normalized * this.m_Radius;
            
            this.m_AnchorPosition = positionAnchor;
            shotType.Position = position;
        }

        // GIZMOS: --------------------------------------------------------------------------------

        public override void OnDrawGizmos(TShotType shotType, Transform transform)
        {
            base.OnDrawGizmos(shotType, transform);
            this.DoDrawGizmos(shotType as TShotTypeLook, GIZMOS_COLOR_GHOST);
        }

        public override void OnDrawGizmosSelected(TShotType shotType, Transform transform)
        {
            base.OnDrawGizmosSelected(shotType, transform);
            this.DoDrawGizmos(shotType as TShotTypeLook, GIZMOS_COLOR_ACTIVE);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private float GetRotationDamp(float current, float target, ref float velocity, 
            float smoothTime, float deltaTime)
        {
            return Mathf.SmoothDampAngle(
                current,
                target,
                ref velocity,
                smoothTime,
                Mathf.Infinity,
                deltaTime
            );
        }

        private Vector3 GetTargetPosition(TShotTypeLook shotType)
        {
            return shotType.Look.GetLookPosition(shotType);
        }
        
        private Vector3 GetAnchorPosition(TShotType shotType)
        {
            GameObject anchor = this.m_Anchor.Get(shotType.Args);
            if (anchor == null) return this.m_AnchorPosition;
            
            return anchor.transform.position + this.m_AnchorOffset.Get(shotType.Args);
        }

        private void DoDrawGizmos(TShotTypeLook shotType, Color color)
        {
            if (!Application.isPlaying) return;
            
            Gizmos.color = color;

            Vector3 positionTarget = this.GetTargetPosition(shotType);
            Vector3 positionAnchor = this.GetAnchorPosition(shotType);

            Gizmos.DrawSphere(positionTarget, 0.05f);
            Gizmos.DrawSphere(positionAnchor, 0.10f);
            
            Gizmos.DrawLine(positionTarget, positionAnchor);
            Gizmos.DrawLine(positionAnchor, shotType.Position);
        }
    }
}