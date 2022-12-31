
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
        public const float GPS_COORD_X_MIN = 8;
        public const float GPS_COORD_X_MAX = 12;
        public const float GPS_COORD_Y_MIN = 19;
        public const float GPS_COORD_Y_MAX = 23;
        public const float TERRAIN_MAX = 1000;
        public const float LIDAR_SCAN_RANGE = 20f;
        public const float PHOTO_VIEWER_FPS = 5f;
    }
}

namespace Rover.Can
{
    public static class CanIDs
    {
        public const ushort SYSTEM_CAM = 0x2000;
    }
}

