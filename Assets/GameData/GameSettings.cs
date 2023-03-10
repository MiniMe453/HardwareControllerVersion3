
namespace Rover.Settings
{
    public static class GameSettings
    {
        public const float MESSAGE_TIMEOUT = 10f;
        public const float INPUT_TIMER_DELAY = 0.05f;
        public const string INPUT_BUNDLE_NAME = "InputRead";
        public const string OUTPUT_BUNDLE_NAME = "OutputSend";
        public const float PROXIMITY_CHECK_DELAY = 0.1f;
        public const int GAME_RES_X = 800;
        public const int GAME_RES_Y = 600;
        public const float PHOTO_LOAD_TIME = 5f;
        public const float PHOTO_VIEWER_LOAD_TIME = 2f;
        public const int PROXIMITY_LAYER_INDEX = 6;
        public const float GPS_COORD_X_MIN = 46;
        public const float GPS_COORD_X_MAX = 53;
        public const float GPS_COORD_Y_MIN = -27;
        public const float GPS_COORD_Y_MAX = -20;
        public const float TERRAIN_MAX = 1200;
        public const float LIDAR_SCAN_RANGE = 20f;
        public const float PHOTO_VIEWER_FPS = 10f;
        public const int HORIZONTAL_CENTER_VAL = 560;
        public const int VERTICAL_CENTER_VAL = 495;
        public const float JOYSTICK_DEADZONE = 0.1f;
        public const float RADIO_FREQ_CHART_UPDATE_TIMER = 1f/10f;
        public const float ROLL_DANGER_ZONE = 35f;
        public const float PITCH_DANGER_ZONE = 35f;
        public const float PROXIMITY_CHECK_RADIUS = 5f;
        public const float MAG_MAX_VALUE = 50f;
        public const float RAD_MAX_VALUE = 0.01f;
        public const float TEMP_MAX_VALUE = 20f;
    }
}

namespace Rover.Can
{
    public static class CanIDs
    {
        public const ushort SYSTEM_CAM = 0x2000;
    }
}

