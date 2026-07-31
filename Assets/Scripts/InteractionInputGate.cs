using UnityEngine;
using UnityEngine.InputSystem;

public static class InteractionInputGate
{
    private static int ultimoFrameF = -1;
    private static int ultimoFrameZ = -1;

    public static bool TryConsumeF()
    {
        if (Keyboard.current == null ||
            !Keyboard.current.fKey.wasPressedThisFrame)
        {
            return false;
        }

        if (ultimoFrameF == Time.frameCount)
            return false;

        ultimoFrameF = Time.frameCount;
        return true;
    }

    public static bool TryConsumeZ()
    {
        if (Keyboard.current == null ||
            !Keyboard.current.zKey.wasPressedThisFrame)
        {
            return false;
        }

        if (ultimoFrameZ == Time.frameCount)
            return false;

        ultimoFrameZ = Time.frameCount;
        return true;
    }
}