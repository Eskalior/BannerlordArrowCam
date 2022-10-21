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

        public bool TrackedMissileCollided{
            get => _trackedMissileCollided;
        }

        public bool CollidedWithPerson
        {
            get => _collidedWithPerson;
        }

        public override void OnMissileCollisionReaction(Mission.MissileCollisionReaction collisionReaction, Agent attackerAgent, Agent attachedAgent, sbyte attachedBoneIndex)
        {
            if (attackerAgent == base.Mission.MainAgent)
            {
                _trackedMissileCollided = true;
                if (attachedAgent != null)
                    _collidedWithPerson = true;
            }
            base.OnMissileCollisionReaction(collisionReaction, attackerAgent, attachedAgent, attachedBoneIndex);
        }

        public Mission.Missile GetLatestMissile()
        {
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

        public Mission.Missile GetMissileByIndex(int index)
        {
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

        public void ResetTrackingMissile()
        {
            _trackedMissileCollided = false;
            _collidedWithPerson = false;
        }

        public override InquiryData OnEndMissionRequest(out bool canLeave)
		{
			canLeave = true;
			return null;
			
		}
	}
}
