using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace ArrowCam
{
    class ArrowCamBehavior : CampaignBehaviorBase
    {

        public override void RegisterEvents()
        {
            CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener(this, new Action<IMission>(this.OnMissionStarted));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnMissionStarted(IMission mission)
        {
            Mission current_mission = (Mission)mission;
            current_mission.AddMissionBehaviour(new ArrowCamMissionLogic());
        }


    }
}
