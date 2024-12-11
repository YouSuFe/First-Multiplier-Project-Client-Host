using Unity.Netcode;
using UnityEngine;
/* 
 * Summary
 * 
 * we run over a coin, we have to tell the server or we have to wait for the server to realize
 * that we've collected it and that can feel a bit laggy because you'll move over a coin.
 * It'll still take a little bit before it disappears.
 * So what we can do is we can hide it client side immediately.
 * Then when the server gets back to us saying, Yep, you collected it and now it's respawned over here,
 * we can then re-enable it.
*/
public abstract class Coin : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    protected int coinValue = 10;

    // To prevent the possibility of collecting the same coin at the same time
    protected bool alreadyCollected;

    public abstract int Collect();

    public void SetValue(int value)
    {
        coinValue = value;
    }

    protected void Show(bool show)
    {
        spriteRenderer.enabled = show;
    }
}
