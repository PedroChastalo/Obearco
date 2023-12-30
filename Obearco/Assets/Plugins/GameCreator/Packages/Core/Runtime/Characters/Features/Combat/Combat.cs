using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Characters
{
    [Serializable]
    public class Combat
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Target m_Target;
        
        [NonSerialized] private List<Weapon> m_Weapons;
        [NonSerialized] private Dictionary<int, IMunition> m_Munitions;
        [NonSerialized] private Dictionary<int, IStance> m_Stances;

        [NonSerialized] private Character m_Character;

        // PROPERTIES: ----------------------------------------------------------------------------

        public GameObject Target
        {
            get => this.m_Target.On;
            set => this.m_Target.On = value;
        }
        
        public Weapon[] Weapons => this.m_Weapons.ToArray();

        public IMunition[] Munitions
        {
            get
            {
                List<IMunition> munitions = new List<IMunition>();
                foreach (KeyValuePair<int, IMunition> entry in this.m_Munitions)
                {
                    munitions.Add(entry.Value);
                }

                return munitions.ToArray();
            }
        }

        // EVENTS: --------------------------------------------------------------------------------

        public event Action<IWeapon, GameObject> EventEquip;
        public event Action<IWeapon, GameObject> EventUnequip;
        
        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public Combat()
        {
            this.m_Target = new Target();
            this.m_Weapons = new List<Weapon>();
            this.m_Munitions = new Dictionary<int, IMunition>();
            this.m_Stances = new Dictionary<int, IStance>();
        }
        
        // INITIALIZE METHODS: --------------------------------------------------------------------
        
        internal void OnStartup(Character character)
        {
            this.m_Character = character;
        }
        
        internal void AfterStartup(Character character)
        { }

        internal void OnDispose(Character character)
        {
            this.m_Character = character;
        }

        internal void OnEnable()
        {
            foreach (KeyValuePair<int, IStance> entry in this.m_Stances)
            {
                entry.Value.OnEnable(this.m_Character);
            }
        }

        internal void OnDisable()
        {
            foreach (KeyValuePair<int, IStance> entry in this.m_Stances)
            {
                entry.Value.OnDisable(this.m_Character);
            }
        }
        
        // UPDATE METHODS: ------------------------------------------------------------------------

        internal void OnLateUpdate()
        {
            foreach (KeyValuePair<int, IStance> entry in this.m_Stances)
            {
                entry.Value.OnUpdate();
            }
        }

        // GETTERS: -------------------------------------------------------------------------------

        public TMunitionValue RequestMunition(IWeapon weapon)
        {
            if (weapon == null) return null;
            if (this.m_Munitions.TryGetValue(weapon.Id.Hash, out IMunition munition))
            {
                return munition.Value;
            }

            munition = new Munition(weapon.Id.Hash, weapon.CreateMunition()); 
            this.m_Munitions.Add(weapon.Id.Hash, munition);

            return munition.Value;
        }

        public T RequestStance<T>() where T : IStance, new()
        {
            int stanceId = typeof(T).GetHashCode();
            if (this.m_Stances.TryGetValue(stanceId, out IStance stance))
            {
                return (T) stance;
            }

            T newStance = new T();
            newStance.OnEnable(this.m_Character);

            this.m_Stances.Add(stanceId, newStance);
            return newStance;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool IsEquipped(IWeapon weapon)
        {
            if (weapon == null) return false;

            foreach (Weapon reference in this.m_Weapons)
            {
                if (reference.Asset.Id.Hash == weapon.Id.Hash) return true;
            }
            
            return false;
        }
        
        public bool IsEquipped(IWeapon weapon, GameObject instance)
        {
            if (weapon == null) return false;

            foreach (Weapon reference in this.m_Weapons)
            {
                if (reference.Asset.Id.Hash != weapon.Id.Hash) continue;
                if (reference.Instance != instance) continue;

                return true;
            }
            
            return false;
        }

        public async Task Equip(IWeapon asset, GameObject instance, Args args)
        {
            if (asset == null) return;
            if (this.IsEquipped(asset, instance)) return;
            
            Weapon weapon = new Weapon(asset, instance);
            this.m_Weapons.Add(weapon);

            if (!this.m_Munitions.ContainsKey(asset.Id.Hash))
            {
                Munition munition = new Munition(asset.Id.Hash, asset.CreateMunition());
                this.m_Munitions.Add(asset.Id.Hash, munition);
            }

            await asset.RunOnEquip(args);
            this.EventEquip?.Invoke(asset, instance);
        }

        public async Task Unequip(IWeapon weapon, Args args)
        {
            if (weapon == null) return;
            if (!this.IsEquipped(weapon)) return;

            GameObject instance = null;
            for (int i = this.m_Weapons.Count - 1; i >= 0; --i)
            {
                Weapon reference = this.m_Weapons[i];
                if (reference.Asset.Id.Hash != weapon.Id.Hash) continue;

                instance = reference.Instance;
                this.m_Weapons.RemoveAt(i);
            }
            
            await weapon.RunOnUnequip(args);
            this.EventUnequip?.Invoke(weapon, instance);
        }
        
        public async Task Unequip(IWeapon weapon, GameObject instance, Args args)
        {
            if (weapon == null) return;
            if (!this.IsEquipped(weapon, instance)) return;

            for (int i = this.m_Weapons.Count - 1; i >= 0; --i)
            {
                Weapon reference = this.m_Weapons[i];
                if (reference.Asset.Id.Hash != weapon.Id.Hash) continue;
                if (reference.Instance != instance) continue;

                this.m_Weapons.RemoveAt(i);
            }
            
            await weapon.RunOnUnequip(args);
            this.EventUnequip?.Invoke(weapon, instance);
        }
    }
}