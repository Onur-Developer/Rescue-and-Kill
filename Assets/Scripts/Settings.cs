using UnityEngine;



[CreateAssetMenu(fileName = "new Setting",menuName = "Setting")]
public class Settings : ScriptableObject
{
   public bool isJuice;
   public bool[] levels;
}
