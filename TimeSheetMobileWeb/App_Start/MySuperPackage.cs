using System;

[assembly: WebActivator.PreApplicationStartMethod(
    typeof(TimeSheetMobileWeb.App_Start.MySuperPackage), "PreStart")]

namespace TimeSheetMobileWeb.App_Start {
    public static class MySuperPackage {
        public static void PreStart() {
            MVCControlsToolkit.Core.Extensions.Register();
        }
    }
}