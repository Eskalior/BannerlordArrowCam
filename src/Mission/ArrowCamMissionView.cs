using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace ArrowCam
{
    [DefaultView]
    class ArrowCamMissionView : MissionView
    {
        private ArrowCamMissionLogic _arrowControlMissionLogic;
        private Camera _camera;
        private Mission.Missile _missile;
        private int _lastMissileIndex = -1;
        private bool _cameraCreated = false;
        private float _watchArrowTimePassed = 0.0f;
        private float _watchArrowTimeoutDefault = .4f;
        private float _watchArrowTimeout = .4f;
        private float _upModifier = .5f;

        private bool _slowMotionActive = false;

        public override void OnCreated()
        {
            base.OnCreated();
        }

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            if (Input.IsKeyDown(InputKey.MiddleMouseButton))
            {
                foreach (SiegeWeapon sw in Mission.Current.ActiveMissionObjects.FindAllWithType<SiegeWeapon>())
                    if (sw.PilotAgent == Agent.Main && sw is Ballista)
                        return;

                // Retrieve missile
                if (_lastMissileIndex == -1)
                {
                    _arrowControlMissionLogic.ResetTrackingMissile();
                    _missile = _arrowControlMissionLogic.GetLatestMissile();
                }
                else
                    _missile = _arrowControlMissionLogic.GetMissileByIndex(_lastMissileIndex);

                // Make sure there is a missile
                if (_missile != null)
                {
                    // Create camera if it doesnt exist yet
                    if (!_cameraCreated)
                        CreateCamera();
                    // Update camera
                    if (!Game.Current.GameStateManager.ActiveStateDisabledByUser)
                        UpdateCamera();
                }
            }
            // Check if it needs to be deleted
            if (_cameraCreated)
            {
                if (!Input.IsKeyDown(InputKey.MiddleMouseButton)
                    || _missile == null
                    || _arrowControlMissionLogic.TrackedMissileCollided)
                {
                    // Slow motion
                    if (_arrowControlMissionLogic.CollidedWithPerson && !_slowMotionActive)
                    {
                        if (Mission.Current.GetRequestedTimeSpeed(1, out float _))
                            Mission.Current.RemoveTimeSpeedRequest(1);
                        Mission.Current.AddTimeSpeedRequest(new Mission.TimeSpeedRequest(.3f, 1));
                        
                        _slowMotionActive = true;
                        _watchArrowTimeout *= 3.5f;
                    }
                    _watchArrowTimePassed += dt;
                    if (_watchArrowTimePassed > _watchArrowTimeout)
                    {
                        RemoveCamera();
                        
                        if (_slowMotionActive)
                        {
                            if (Mission.Current.GetRequestedTimeSpeed(1, out float _))
                                Mission.Current.RemoveTimeSpeedRequest(1);
                            Mission.Current.AddTimeSpeedRequest(new Mission.TimeSpeedRequest(1, 1));

                            _slowMotionActive = false;
                        }
                        _watchArrowTimeout = _watchArrowTimeoutDefault;
                        _watchArrowTimePassed = 0.0f;
                    }
                }
            }
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            _arrowControlMissionLogic = Mission.GetMissionBehavior<ArrowCamMissionLogic>();
        }

        private void CreateCamera()
        {
            // Save missile index to track
            _lastMissileIndex = _missile.Index;

            _camera = Camera.CreateCamera();
            Camera combatCamera = base.MissionScreen.CombatCamera;
            if (combatCamera != null)
            {
                _camera.FillParametersFrom(combatCamera);
            }
            base.MissionScreen.SceneLayer.Input.IsMouseWheelAllowed = false;
            _cameraCreated = true;
        }

        private Camera lastCamera { get; set; }
        private void UpdateCamera()
        {
            _camera.Position = _missile.GetPosition() - (_missile.GetVelocity().NormalizedCopy() * 2) + (Vec3.Up * _upModifier);
            _camera.LookAt(_camera.Position, _camera.Position + _missile.GetVelocity(), Vec3.Up);
            base.MissionScreen.SceneLayer.Input.IsMouseWheelAllowed = false;
            // Give other cameras priority
            if (base.MissionScreen.CustomCamera == null)
                base.MissionScreen.CustomCamera = _camera;
        }

        private void RemoveCamera()
        {
            _arrowControlMissionLogic.ResetTrackingMissile();
            _lastMissileIndex = -1;

            base.MissionScreen.CustomCamera = null;
            base.MissionScreen.SceneLayer.Input.IsMouseWheelAllowed = true;
            _camera = null;
            _cameraCreated = false;
        }
    }
}