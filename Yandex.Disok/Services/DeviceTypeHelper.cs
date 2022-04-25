using Windows.System.Profile;
using Windows.UI.ViewManagement;

namespace Ya.D.Services
{
    // source from https://gist.github.com/wagonli/40d8a31bd0d6f0dd7a5d
    public enum DeviceType
    {
        Phone,
        Desktop,
        Tablet,
        IoT,
        SurfaceHub,
        Other
    }

    public static class DeviceTypeHelper
    {
        public static DeviceType GetDeviceFormFactorType()
        {
            switch (AnalyticsInfo.VersionInfo.DeviceFamily)
            {
                case "Windows.Mobile":
                    return DeviceType.Phone;
                case "Windows.Desktop":
                    return UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Mouse ? DeviceType.Desktop : DeviceType.Tablet;
                case "Windows.Universal":
                    return DeviceType.IoT;
                case "Windows.Team":
                    return DeviceType.SurfaceHub;
                default:
                    return DeviceType.Other;
            }
        }
    }
}