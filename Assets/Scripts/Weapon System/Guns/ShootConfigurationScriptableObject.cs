using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName ="Shoot Config", menuName = "Guns/Shoot Configuration", order = 2)]
public class ShootConfigurationScriptableObject : ScriptableObject
{
    public LayerMask Hitmask;
    public float FireRate = 0.25f;
    public float RecoilRecoverySpeed = 1f;
    public float MaxSpeedTime = 1f;
    public BulletSpreadType Spreadtype = BulletSpreadType.Simple;
    [Header("Simple Spread")]
    public Vector3 Spread = new Vector3(0.1f, 0.1f, 0.1f);

    [Header("Texture-Based Spread")]
    [Range(0.001f, 5f)]
    public float SPreadMultiplier = 0.1f;
    public Texture2D SpreadTexture;


    public Vector3 GetSpread(float ShootTime = 0)
    {
        Vector3 spread = Vector3.zero;

        if(Spreadtype == BulletSpreadType.Simple)
        {
            spread = Vector3.Lerp(Vector3.zero,
                new Vector3(
                Random.Range(
                        -Spread.x,
                        Spread.x
                        ),
                    Random.Range(
                        -Spread.y,
                        Spread.y
                        ),
                    Random.Range(
                        -Spread.z,
                        Spread.z
                        )),
                    Mathf.Clamp01(ShootTime/MaxSpeedTime)
                    );
        }
        else if(Spreadtype == BulletSpreadType.TextureBased)
        {
            spread = GetTextureDirection(ShootTime);
            spread *= SPreadMultiplier;
        }

        return spread;
    }
    private Vector3 GetTextureDirection(float ShootTime)
    {
        Vector2 halfSize = new Vector2(SpreadTexture.width / 2f, SpreadTexture.height / 2f);
        int halfSquareExtents = Mathf.CeilToInt(
            Mathf.Lerp(
                0.01f,
                halfSize.x,
                Mathf.Clamp01(ShootTime / MaxSpeedTime)));

        int minX = Mathf.FloorToInt(halfSize.x) - halfSquareExtents;
        int minY = Mathf.FloorToInt(halfSize.y) - halfSquareExtents;

        Color[] sampleColors = SpreadTexture.GetPixels(
            minX,
            minY,
            halfSquareExtents * 2,
            halfSquareExtents * 2);

        float[] colorAsGrey = System.Array.ConvertAll(sampleColors, (color) => color.grayscale);
        float totalGreyValue = colorAsGrey.Sum();

        float grey = Random.Range(0, totalGreyValue);
        int i = 0;
        for(; i < colorAsGrey.Length; i++)
        {
            grey -= colorAsGrey[i];
            if(grey <= 0)
            {
                break;
            }
        }

        int x = minX + i % (halfSquareExtents * 2);
        int y = minY + i / (halfSquareExtents * 2);

        Vector2 targetPosition = new Vector2(x, y);
        Vector2 direction = (targetPosition - halfSize) / halfSize.x;
        return direction;
    }
}
