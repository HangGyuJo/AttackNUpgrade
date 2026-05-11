using Unity.Netcode.Components;

public class OwnerTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}