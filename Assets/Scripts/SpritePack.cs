using UnityEngine;

[CreateAssetMenu(menuName = "UnityMania/Sprite Pack")]
public class SpritePack : ScriptableObject
{
    public string packName;
    public Sprite receptor;
    public Sprite note;

    public Sprite[] holdTop;
    public Sprite[] holdCenter;
    public Sprite[] holdBottom;
}