using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * 用于加载各个战斗场景（通过 yaml 加载）
 */
public class CombactList : MonoBehaviour
{
    public class CombactListData
    {
        public int combactId = 0;

        public class armyData
        {
            public int characterId = 0;
            public float positionX = 0f;
            public float positionY = 0f;
            public float positionZ = 0f;
            public float rotaX = 0f;
            public float rotaY = 0f;
            public float rotaZ = 0f;
            public int Owner = 1;
        }

        public List<armyData> army;
        public armyData mainCharacter;
    }

    private List<CombactListData> combactListData;
    private characters __characters;
    public GameObject mainCharacter;
    public GameObject mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        loadCombactList();
        __characters = GetComponent<characters>();
    }

    void loadCombactList()
    {
        const string filePath = "characters/CombactList";
        var textFile = Resources.Load<TextAsset>(filePath);
        Debug.Log(textFile.text);
        var deserializer = new YamlDotNet.Serialization.Deserializer();

        combactListData = deserializer.Deserialize<List<CombactListData> >(textFile.text);
        Debug.Log("CombactList: " + combactListData[0].army[0].rotaY);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void loadCombact(int combactId)
    {
        foreach (CombactListData.armyData army in combactListData[combactId].army)
        {
            int typeIdx = army.characterId;
            Vector3 position = new Vector3() { x = army.positionX, y= army.positionY, z = army.positionZ };
            Quaternion rota_ = new Quaternion();
            rota_.eulerAngles = new Vector3(army.rotaX, army.rotaY, army.rotaZ);
            int Owner = army.Owner;

            __characters.addInstantCharObj(typeIdx, position, rota_, Owner);
        }
        mainCharacter.transform.position = new Vector3(
            combactListData[combactId].mainCharacter.positionX,
            combactListData[combactId].mainCharacter.positionY,
            combactListData[combactId].mainCharacter.positionZ);
        Quaternion rota = new Quaternion();
        rota.eulerAngles = new Vector3(
            combactListData[combactId].mainCharacter.rotaX,
            combactListData[combactId].mainCharacter.rotaY,
            combactListData[combactId].mainCharacter.rotaZ);
        mainCharacter.transform.rotation = rota;

        //让镜头移动到主角上方 
        mainCamera.GetComponent<main_camera>().moveTowards(mainCharacter.transform.position);
    } 
}
