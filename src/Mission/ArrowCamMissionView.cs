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
        private float _watchArrowTimeout = 0.0f;
        private float _upModifier = .5f;

        private bool _slowMotionActive = false;

        public override void OnCreated()
        {
            base.OnCreated();
        }

        /// <summary>
        ///     Each tick we check if the player holds down the key and update the camera accordingly.
        /// </summary>
        /// <param name="dt">Time since the last tick.</param>
        public override void OnMissionScreenTick(float dt)
        {
            // Let the game to everything else first
            base.OnMissionScreenTick(dt);

            // Check if the key is pressed
            if (Input.IsKeyDown(InputKey.MiddleMouseButton))
            {
                // Currently disable ballistas
                foreach (SiegeWeapon sw in Mission.Current.ActiveMissionObjects.FindAllWithType<SiegeWeapon>())
                    if (sw.PilotAgent == Agent.Main && sw is Ballista)
                        return;

                // Retrieve missile
                if (_lastMissileIndex == -1)
                {
                    _arrowControlMissionLogic.ResetTrackingMissile();
                    _missile = _arrowControlMissionLogic.GetLatestMissile();
                }
                else // Get the already tracked missile by index
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
                if (!Input.IsKeyDown(InputKey.MiddleMouseButton)            // When the key is not pressed anymore
                    || _missile == null                                     // When there is nothing to track
                    || _arrowControlMissionLogic.TrackedMissileCollided)    // When the missile collided
                {
                    // Slow motion
                    if (ArrowCamConfig.EnableSlowMotion && _arrowControlMissionLogic.CollidedWithPerson && !_slowMotionActive)
                    {
                        if (Mission.Current.GetRequestedTimeSpeed(1, out float _))
                            Mission.Current.RemoveTimeSpeedRequest(1);
                        Mission.Current.AddTimeSpeedRequest(new Mission.TimeSpeedRequest(.3f, 1));

                        _slowMotionActive = true;
                        _watchArrowTimeout = ArrowCamConfig.SlowMotionTime;
                    }

                    // Check if we watched the missile long enough
                    _watchArrowTimePassed += dt;
                    if (_watchArrowTimePassed > _watchArrowTimeout  // When the maximum timeout is reached
                        || (!Input.IsKeyDown(InputKey.MiddleMouseButton) && _watchArrowTimePassed > ArrowCamConfig.StopWatchingDelay)) // When the key is lifted and the base timeout is reached
                    {
                        RemoveCamera();

                        // Reset slow motion
                        if (_slowMotionActive)
                        {
                            if (Mission.Current.GetRequestedTimeSpeed(1, out float _))
                                Mission.Current.RemoveTimeSpeedRequest(1);
                            Mission.Current.AddTimeSpeedRequest(new Mission.TimeSpeedRequest(1, 1));

                            _slowMotionActive = false;
                        }
                        _watchArrowTimeout = ArrowCamConfig.StopWatchingDelay;
                        _watchArrowTimePassed = 0.0f;
                    }
                }
            }
        }

        /// <summary>
        ///     Initialization.
        /// </summary>
        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            _arrowControlMissionLogic = Mission.GetMissionBehavior<ArrowCamMissionLogic>();
        }

        /// <summary>
        ///     Creates the Camera that follow the missile.
        /// </summary>
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

            // Disable interactions with the mouse wheel while we are following it
            base.MissionScreen.SceneLayer.Input.IsMouseWheelAllowed = false;
            _cameraCreated = true;
        }

        /// <summary>
        ///     Updates the position and orientation of the custom camera.
        /// </summary>
        private void UpdateCamera()
        {
            // Position the camera slightly behind the arrow
            _camera.Position = _missile.GetPosition() - (_missile.GetVelocity().NormalizedCopy() * 2) + (Vec3.Up * _upModifier);

            // Calculate the orientation
            _camera.LookAt(_camera.Position, _camera.Position + _missile.GetVelocity(), Vec3.Up);

            // Make sure that no other mouse wheel action is allowed
            base.MissionScreen.SceneLayer.Input.IsMouseWheelAllowed = false;

            // Give other cameras priority
            if (base.MissionScreen.CustomCamera == null)
                base.MissionScreen.CustomCamera = _camera;
        }

        /// <summary>
        ///     Removes the custom camera.
        /// </summary>
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