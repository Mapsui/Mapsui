namespace Mapsui.UI
{ 
    public class MapLock
    {
        public void LockAll()
        {
            PanLock = true;
            ZoomLock = true;
            RotationLock = true;
        }

        public void ReleaseAll()
        {
            PanLock = false;
            ZoomLock = false;
            RotationLock = false;
        }

        ///// <summary>
        ///// When true the user can not pan (move) the map.
        ///// </summary>
        public bool PanLock { get; set; }

        ///// <summary>
        ///// When true the user an not rotate the map
        ///// </summary>
        public bool ZoomLock { get; set; }

        ///// <summary>
        ///// When true the user can not zoom into the map
        ///// </summary>
        public bool RotationLock { get; set; }
    }
}
