using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCreator.Runtime.Characters
{
    [Serializable]
    public class Props
    {
        [field: NonSerialized] public static GameObject LastPropAttached { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeOnLoad()
        {
            LastPropAttached = null;
        }
        
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Dictionary<int, List<Prop>> m_Props;
        [NonSerialized] private Character m_Character;

        // EVENTS: --------------------------------------------------------------------------------

        public event Action<Transform, GameObject, GameObject> EventAdd;
        public event Action<Transform, GameObject> EventRemove;
        
        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public Props()
        {
            this.m_Props = new Dictionary<int, List<Prop>>();
        }
        
        // INITIALIZE METHODS: --------------------------------------------------------------------
        
        internal void OnStartup(Character character)
        {
            this.m_Character = character;
            this.m_Character.EventAfterChangeModel += this.OnChangeModel;
        }
        
        internal void AfterStartup(Character character)
        { }

        internal void OnDispose(Character character)
        {
            this.m_Character = character;
            this.m_Character.EventAfterChangeModel -= this.OnChangeModel;
        }

        internal void OnEnable()
        { }

        internal void OnDisable()
        { }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        /// <summary>
        /// Creates a new instance of the prefab at the specified bone location with the right
        /// coordinates.
        /// </summary>
        /// <param name="bone"></param>
        /// <param name="prefab"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public GameObject Attach(IBone bone, GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;
            
            int instanceID = prefab.GetInstanceID();
            if (!this.m_Props.TryGetValue(instanceID, out List<Prop> props))
            {
                props = new List<Prop>();
                this.m_Props.Add(instanceID, props);
            }
            
            Prop prop = new Prop(bone, prefab, position, rotation);
            prop.Create(this.m_Character.Animim.Animator);
            
            props.Add(prop);
            LastPropAttached = prop.InstancePrefab;
            
            this.EventAdd?.Invoke(
                prop.InstanceBone,
                prefab,
                prop.InstancePrefab
            );

            return prop.InstancePrefab;
        }

        /// <summary>
        /// Removes an instance of the prefab. If there are multiple instances, it removes the
        /// oldest one.
        /// </summary>
        /// <param name="prefab"></param>
        public void Remove(GameObject prefab)
        {
            if (prefab == null) return;
            int instanceID = prefab.GetInstanceID();
            
            if (!this.m_Props.TryGetValue(instanceID, out List<Prop> props)) return;
            if (props.Count <= 0) return;

            int removeIndex = props.Count - 1;
            Transform bone = props[removeIndex].InstanceBone;
            
            props[removeIndex].Destroy();
            props.RemoveAt(removeIndex);
            
            this.EventRemove?.Invoke(bone, prefab);
        }

        /// <summary>
        /// Removes the specific instance of an instance of a prefab
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="instanceID"></param>
        public void Remove(GameObject prefab, int instanceID)
        {
            if (prefab == null) return;
            int prefabInstanceID = prefab.GetInstanceID();

            if (!this.m_Props.TryGetValue(prefabInstanceID, out List<Prop> props)) return;
            if (props.Count <= 0) return;

            for (int i = 0; i < props.Count; i++)
            {
                Prop prop = props[i];
                
                if (prop.InstancePrefab == null) continue;
                if (prop.InstancePrefab.GetInstanceID() != instanceID) continue;

                Transform bone = prop.InstanceBone;

                prop.Destroy();
                props.RemoveAt(i);

                this.EventRemove?.Invoke(bone, prefab);
                return;
            }
        }

        /// <summary>
        /// Removes all props associated with a bone
        /// </summary>
        /// <param name="bone"></param>
        public void RemoveAtBone(IBone bone)
        {
            Transform boneTransform = bone.GetTransform(this.m_Character.Animim.Animator);
            if (boneTransform == null) return;
            
            foreach (KeyValuePair<int, List<Prop>> entry in this.m_Props)
            {
                for (int i = entry.Value.Count - 1; i >= 0; --i)
                {
                    Prop prop = entry.Value[i];
                    if (prop.InstanceBone != boneTransform) continue;

                    GameObject prefab = prop.Prefab;
                    
                    prop.Destroy();
                    entry.Value.RemoveAt(i);
                    
                    this.EventRemove?.Invoke(boneTransform, prefab);
                }
            }
        }

        /// <summary>
        /// Removes all props
        /// </summary>
        public void RemoveAll()
        {
            foreach (KeyValuePair<int, List<Prop>> entry in this.m_Props)
            {
                foreach (Prop prop in entry.Value)
                {
                    prop.Destroy();
                    this.EventRemove?.Invoke(prop.InstanceBone, prop.Prefab);
                }
            }
            
            this.m_Props.Clear();
        }

        /// <summary>
        /// Returns true if the specified bone contains at least one prop
        /// </summary>
        /// <param name="bone"></param>
        /// <returns></returns>
        public bool HasAtBone(IBone bone)
        {
            Transform boneTransform = bone.GetTransform(this.m_Character.Animim.Animator);
            if (boneTransform == null) return false;
            
            foreach (KeyValuePair<int,List<Prop>> entry in this.m_Props)
            {
                for (int i = entry.Value.Count - 1; i >= 0; --i)
                {
                    Prop prop = entry.Value[i];
                    if (prop?.InstanceBone != boneTransform) continue;

                    return true;
                }
            }

            return false;
        }
        
        // CALLBACKS: -----------------------------------------------------------------------------

        private void OnChangeModel()
        {
            foreach (KeyValuePair<int, List<Prop>> entry in this.m_Props)
            {
                foreach (Prop prop in entry.Value)
                {
                    prop.Destroy();
                    this.EventRemove?.Invoke(prop.InstanceBone, prop.Prefab);
                    
                    prop.Create(this.m_Character.Animim.Animator);
                    this.EventAdd?.Invoke(prop.InstanceBone, prop.InstancePrefab, prop.Prefab);
                }
            }
        }
    }
}