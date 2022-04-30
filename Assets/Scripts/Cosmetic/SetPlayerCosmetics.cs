using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class SetPlayerCosmetics : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField]
    private List<Transform> hats, hair, tops, pants, shoes, onsie = new List<Transform>();

    [SerializeField]
    private List<Transform> skinObjects = new List<Transform>();

    [SerializeField]
    private List<Material> hatMaterials, hairMaterials, topMaterials, pantMaterials, shoeMaterials, onsieMaterials, skinMaterials = new List<Material>();

    private List<int> randomCosmeticSelections = new List<int>(5);
    private List<int> randomMaterialSelections = new List<int>(5);

    [SerializeField]
    private const int hatChance = 50, pantsChance = 90, notOnsieChance = 95;

    public string publicCosmeticJson; // figure this out

    public bool netTest = false;
    public bool networkCosmesticsLoaded = false;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // info.Sender : is the player
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(netTest);
            stream.SendNext(publicCosmeticJson);
        }
        else
        {
            // Network player, receive data
            this.netTest = (bool)stream.ReceiveNext();
            this.publicCosmeticJson = (string)stream.ReceiveNext();

            if (!networkCosmesticsLoaded && publicCosmeticJson.Length > 0)
            {
                ApplyJsonData(JsonUtility.FromJson<JSON_PlayerClothing>(publicCosmeticJson));
            }
        }
    }

    private void Awake()
    {
        // mp: all players
        TriggerHideAllCosmetics();

        if (photonView.IsMine)
        {
            SetClothesValues();
            SetJsonValues();
            ApplyJsonData(JsonUtility.FromJson<JSON_PlayerClothing>(publicCosmeticJson));
        }

        
    }

    private void Start()
    {
        //PhotonNetwork.SendRate = 20; // 20 by default
        //PhotonNetwork.SerializationRate = 40; // 10 by default

        print($"publicCosmeticJson: {publicCosmeticJson}");
        
    }

    

    public void SetClothesValues()
    {
        TriggerHideAllCosmetics();

        FillRandomSelection(hats, hatMaterials, 0);
        FillRandomSelection(hair, hairMaterials, 1);
        FillRandomSelection(tops, topMaterials, 2);
        FillRandomSelection(pants, pantMaterials, 3);
        FillRandomSelection(shoes, shoeMaterials, 4);
        FillRandomSelection(onsie, onsieMaterials, 5);
    }

    private void SetJsonValues()
    {
        // json
        JSON_PlayerClothing jsonPlayerClothing = new JSON_PlayerClothing();
        #region assign json model
        jsonPlayerClothing.hat = randomCosmeticSelections[0];
        jsonPlayerClothing.hair = randomCosmeticSelections[1];
        jsonPlayerClothing.top = randomCosmeticSelections[2];
        jsonPlayerClothing.pants = randomCosmeticSelections[3];
        jsonPlayerClothing.shoes = randomCosmeticSelections[4];
        jsonPlayerClothing.onsie = randomCosmeticSelections[5];

        jsonPlayerClothing.hatCol = randomMaterialSelections[0];
        jsonPlayerClothing.hairCol = randomMaterialSelections[1];
        jsonPlayerClothing.topCol = randomMaterialSelections[2];
        jsonPlayerClothing.pantsCol = randomMaterialSelections[3];
        jsonPlayerClothing.shoesCol = randomMaterialSelections[4];
        jsonPlayerClothing.onsieCol = randomMaterialSelections[5];
        jsonPlayerClothing.skinCol = UnityEngine.Random.Range(0, skinMaterials.Count);
        #endregion

        if (UnityEngine.Random.Range(0, 101) <= hatChance) jsonPlayerClothing.hair = -1;
        else jsonPlayerClothing.hat = -1;
        if (UnityEngine.Random.Range(0, 101) <= pantsChance) jsonPlayerClothing.pants = -1; // jsonPlayerClothing.pants can only be the skirt (currently)
        if (UnityEngine.Random.Range(0, 101) <= notOnsieChance) jsonPlayerClothing.onsie = -1;

        publicCosmeticJson = JsonUtility.ToJson(jsonPlayerClothing);
    }

    public string GetJson(JSON_PlayerClothing _jsonPlayerClothing)
    {
        ApplyJsonData(_jsonPlayerClothing);
        return JsonUtility.ToJson(_jsonPlayerClothing);

        #region debugging
        //string json = JsonConvert.SerializeObject(jsonPlayerClothing); // MIGHT BE GETTING STUCK WHEN DOING JSON / CLASS STUFF! // (DO NOT USE NEWTONSOFT)
        //string json = JsonUtility.ToJson(jsonPlayerClothing);
        //print(json); // send over network then apply clothing on this player (for other players screens)
        //publicCosmeticJson = json; // figure it out

        //GetComponentInParent<NetworkCosmetics>().SendJsonOverNetwork(json);
        //photonView.RPC("SyncJsonString", RpcTarget.All, json);
        #endregion
    }

    

    private void Update()
    {
        if (photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                netTest = !netTest;
                SetJsonValues();
                ApplyJsonData(JsonUtility.FromJson<JSON_PlayerClothing>(publicCosmeticJson));
            }
        }
    }

    private void TriggerHideAllCosmetics()
    {
        HideAllCosmetics(hats);
        HideAllCosmetics(hair);
        HideAllCosmetics(tops);
        HideAllCosmetics(pants);
        HideAllCosmetics(shoes);
        HideAllCosmetics(onsie);
    }

    private void HideAllCosmetics(List<Transform> _cosmeticList)
    {
        foreach (Transform x in _cosmeticList)
        {
            x.gameObject.SetActive(false);
        }
    }

    private void FillRandomSelection(List<Transform> _models, List<Material> _mats, int _randomSelectionsIndex)
    {
        int rand = UnityEngine.Random.Range(0, _models.Count);
        int randMaterial = UnityEngine.Random.Range(0, _mats.Count);
        // Debug.Log($"list: {_models}, {_models[rand].name}");
        randomCosmeticSelections.Insert(_randomSelectionsIndex, rand);
        randomMaterialSelections.Insert(_randomSelectionsIndex, randMaterial);
    }

    void ApplyJsonData(JSON_PlayerClothing _jsonPlayerClothing)
    {
        foreach (Transform x in skinObjects) x.GetComponent<SkinnedMeshRenderer>().material = skinMaterials[_jsonPlayerClothing.skinCol];

        if (_jsonPlayerClothing.onsie != -1)
        {
            onsie[_jsonPlayerClothing.onsie].gameObject.SetActive(true);
            onsie[_jsonPlayerClothing.onsie].GetComponent<SkinnedMeshRenderer>().material = onsieMaterials[_jsonPlayerClothing.onsieCol];
            return;
        }

        if (_jsonPlayerClothing.hat != -1)
        {
            hats[_jsonPlayerClothing.hat].gameObject.SetActive(true);
            hats[_jsonPlayerClothing.hat].GetComponent<SkinnedMeshRenderer>().material = hatMaterials[_jsonPlayerClothing.hatCol];
        }

        if (_jsonPlayerClothing.hair != -1)
        {
            hair[_jsonPlayerClothing.hair].gameObject.SetActive(true);
            hair[_jsonPlayerClothing.hair].GetComponent<SkinnedMeshRenderer>().material = hairMaterials[_jsonPlayerClothing.hairCol];
        }

        if (_jsonPlayerClothing.top != -1)
        {
            tops[_jsonPlayerClothing.top].gameObject.SetActive(true);
            tops[_jsonPlayerClothing.top].GetComponent<SkinnedMeshRenderer>().material = topMaterials[_jsonPlayerClothing.topCol];
        }

        if (_jsonPlayerClothing.pants != -1)
        {
            pants[_jsonPlayerClothing.pants].gameObject.SetActive(true);
            pants[_jsonPlayerClothing.pants].GetComponent<SkinnedMeshRenderer>().material = pantMaterials[_jsonPlayerClothing.pantsCol];
        }

        if (_jsonPlayerClothing.shoes != -1)
        {
            shoes[_jsonPlayerClothing.shoes].gameObject.SetActive(true);
            shoes[_jsonPlayerClothing.shoes].GetComponent<SkinnedMeshRenderer>().material = shoeMaterials[_jsonPlayerClothing.shoesCol];
        }
    }

}

[Serializable]
public class JSON_PlayerClothing
{
    public int hat, hair, top, pants, shoes, onsie;
    public int hatCol, hairCol, topCol, pantsCol, shoesCol, onsieCol, skinCol;
}