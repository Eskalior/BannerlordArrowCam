using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace ArrowCam
{
    class ArrowCamMissionLogic : MissionLogic
    {
        private bool _trackedMissileCollided = false;
        private bool _collidedWithPerson = false;

        public bool TrackedMissileCollided
        {
            get => _trackedMissileCollided;
        }

        public bool CollidedWithPerson
        {
            get => _collidedWithPerson;
        }

        /// <summary>
        ///     Adds custom behavior to the moment when a missile collides with something.
        /// </summary>
        /// <param name="collisionReaction">We just forward this.</param>
        /// <param name="attackerAgent">The person that shot the missile.</param>
        /// <param name="attachedAgent">The person that got shot.</param>
        /// <param name="attachedBoneIndex">We just forward this, probably a way to add special behavior for headshots.</param>
        public override void OnMissileCollisionReaction(Mission.MissileCollisionReaction collisionReaction, Agent attackerAgent, Agent attachedAgent, sbyte attachedBoneIndex)
        {
            // We only care about missiles shot by the player
            if (attackerAgent == base.Mission.MainAgent)
            {
                _trackedMissileCollided = true;

                // Make sure a person was hit
                if (attachedAgent != null)
                    _collidedWithPerson = true;
            }
            // Call the base behavior
            base.OnMissileCollisionReaction(collisionReaction, attackerAgent, attachedAgent, attachedBoneIndex);
        }

        /// <summary>
        ///     Returns the last missile shot by the player.
        /// </summary>
        /// <returns>The missile object.</returns>
        public Mission.Missile GetLatestMissile()
        {
            // If the missile we track already collided we skip this
            // This prevents looking at an old missile
            if (_trackedMissileCollided)
                return null;

            Mission.Missile foundMissile = null;
            foreach (Mission.Missile missile in base.Mission.Missiles)
            {
                if (missile.ShooterAgent == base.Mission.MainAgent)
                {
                    foundMissile = missile;
                }
            }
            return foundMissile;
        }

        /// <summary>
        ///     Return a missile fired by the player by index.
        /// </summary>
        /// <param name="index">Index of the missile.</param>
        /// <returns>The missile object.</returns>
        public Mission.Missile GetMissileByIndex(int index)
        {
            // We skip if it already collided
            if (_trackedMissileCollided)
                return null;

            foreach (Mission.Missile missile in base.Mission.Missiles)
            {
                if (missile.ShooterAgent == base.Mission.MainAgent && missile.Index == index)
                {
                    return missile;
                }
            }
            return null;
        }

        /// <summary>
        ///     Resets the currently tracked missile such that another can be tracked.
        /// </summary>
        public void ResetTrackingMissile()
        {
            _trackedMissileCollided = false;
            _collidedWithPerson = false;
        }

        /// <summary>
        ///     The game asks us if it can end the mission here. We won't block that.
        /// </summary>
        /// <param name="canLeave">Whether or not we allow the game to end the mission.</param>
        /// <returns>null</returns>
        public override InquiryData OnEndMissionRequest(out bool canLeave)
        {
            canLeave = true;
            return null;
        }
    }
}