using System;
using UnityEngine;

namespace GameCreator.Runtime.Characters
{
    public readonly struct ReactionInput
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        
        /// <summary>
        /// The normalized direction in Local Space from the hit character's perspective
        /// </summary>
        [field: NonSerialized] public Vector3 Direction { get; }
        
        /// <summary>
        /// A value defined by the weapon that describes how strong the attack is
        /// </summary>
        [field: NonSerialized] public float Force { get; }

        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public ReactionInput(Vector3 direction, float force)
        {
            this.Direction = direction;
            this.Force = force;
        }
    }
}