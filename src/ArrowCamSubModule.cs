using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;

namespace ArrowCam
{
    public class ArrowCamSubModule : MBSubModuleBase
    {
		public ArrowCamSubModule()
		{
		}

		protected override void OnSubModuleLoad()
		{
			base.OnSubModuleLoad();
		}

		protected override void OnApplicationTick(float dt)
		{
		}

		protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
		{
			if (game.GameType is Campaign)
			{
				CampaignGameStarter gameInitializer = (CampaignGameStarter)gameStarterObject;
				this.AddBehaviors(gameInitializer);
			}
		}

		private void AddBehaviors(CampaignGameStarter gameStarterObject)
		{
			gameStarterObject.AddBehavior(new ArrowCamBehavior());
		}

	}
}
