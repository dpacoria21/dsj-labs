using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

// En el editor de Windows, Vuforia abre su driver del simulador (UnityDriver.dll) por nombre y no lo encuentra
// porque la carpeta del paquete no está en la ruta de búsqueda de DLLs. El Play Mode falla con
// "DRIVER_CONFIG_LOAD_ERROR". Cargar la DLL con su ruta completa al abrir el proyecto lo soluciona.
// No afecta al APK: en Android Vuforia usa la cámara real del dispositivo.
[InitializeOnLoad]
public static class VuforiaDriverPreload
{
    static VuforiaDriverPreload() => Load();

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibrary(string path);

    public static void Load()
    {
        if (Application.platform != RuntimePlatform.WindowsEditor)
            return;
        var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.ptc.vuforia.engine");
        if (package == null)
            return;
        string driver = Path.Combine(package.resolvedPath, "Vuforia", "Plugins", "Windows", "x64", "UnityDriver.dll");
        if (File.Exists(driver) && LoadLibrary(driver) == IntPtr.Zero)
            Debug.LogWarning("[Lab09] No se pudo cargar " + driver + " (error " + Marshal.GetLastWin32Error() + ")");
    }
}
