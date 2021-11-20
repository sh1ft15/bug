using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CharObj", order = 2)]

public class CharObj : ScriptableObject {
    public Sprite sprite;
    public string charName;
    public int spriteDir;
    public List<HostObj> hosts;
}
