using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    [SerializeField] private Transform BottomTrans;
    [SerializeField] private Transform MidTrans;
    [SerializeField] private Transform TopTrans;


    private int CurLevel = 1;
    public GameObject wrongGo;

    public GameObject rightGo;

    private RectTransform wrongTrans = null;

    private int findNum = 0;
    //private bool isShowButton = false;

    Transform panelTrans;  //界面
    Transform passTrans; //通关
    Transform levelInfoTrans;//关卡信息
    List<string> riskInfoList;

    public float time;

    private void Start()
    {
        //加载panel
        ShowPanel();
        InvokeRepeating("IncrementTime", 1f, 1f);
    }

    private void ShowPanel()
    {
        if (panelTrans == null)
        {
            string prefabName = "Panel" + CurLevel.ToString();
            riskInfoList = RiskInfo.RiskTexts[CurLevel - 1];
            string assetPath = string.Format("Panel/Levels/{0}", prefabName);
            LoadPanel(assetPath, BottomTrans, ref panelTrans);
            if (panelTrans == null)
            {
                Debug.LogError("load panel failed");
                return;
            }

            //show level info
            if (levelInfoTrans == null)
            {
                LoadPanel("Panel/LevelInfo", MidTrans, ref levelInfoTrans);
            }
            //update level info
            UpdateLevelInfo();

            //控件注册
            BindBtn();

            SetImageScale();

            Reset();


        }
        else  //如果当前有就关闭
        {
            Destroy(panelTrans.gameObject);
            panelTrans = null;
            ShowPanel();
        }
    }

    private void Reset()
    {
        findNum = 0;
        time = 0;
        IncrementTime();
    }

    private void BindBtn()
    {
        if (panelTrans.Find("Image/BtnAll").TryGetComponent<Button>(out var btnAll))
        {
            btnAll.onClick.AddListener(ButtonAllClickHandler);
        }
        else
        {
            Debug.LogError("Button not found.");
        }

        for (int i = 0; i < riskInfoList.Count; i++)
        {
            var str = "Image/Btn" + (i + 1).ToString();
            var trans = panelTrans.Find(str);
            if (trans == null)
            {
                Debug.LogError("trans is null."  + str);

            }
            if (trans.TryGetComponent<Button>(out var btn))
            {
                int index = i;
                btn.onClick.AddListener(() => ButtonClickHandler(index, trans));
            }
            else
            {
                Debug.LogError("Button not found.");
            }
        }
    }

    private void SetImageScale()
    {
        Transform imgTrans = panelTrans.Find("Image");
        RectTransform rt = imgTrans.GetComponent<RectTransform>();
        float width = rt.rect.width;
        float height = rt.rect.height;
        //获取屏幕（canvas）宽高
        float ratioWidth = width / Screen.width;
        float ratioHeight = height / Screen.height;
        bool widthMore = ratioWidth > ratioHeight;
        float ratio = ratioWidth > ratioHeight ? ratioWidth : ratioHeight;

        imgTrans.localScale /= ratio;
    }

    private void ButtonClickHandler(int v, Transform transform)
    {
        if (wrongTrans != null && wrongTrans.gameObject.activeSelf)
        {
            return;
        }
        Debug.Log("on click : " + v);
        if (transform.Find("right") == null)
        {
            GameObject go = Instantiate(rightGo);
            go.transform.SetParent(transform, false);
            go.transform.name = "right";
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale /= 2;

            findNum += 1;

            //更新关卡信息
            UpdateLevelInfo(v);


            //string fieldname = string.Format("risk_{0}_info", v);
            //Type type = typeof(Level2);
            //FieldInfo field = type.GetField(fieldname, BindingFlags.Static | BindingFlags.Public);
            //string value = (string)field.GetValue(null);
            //Debug.Log($"{fieldname}{ value}");


            //todolkk:12通关，恭喜
            if (findNum >= 1)//riskInfoList.Count)
            {
                //panelTrans.Find("RawImage").gameObject.SetActive(true); 
                OnLevelPass();
            }
        }
    }


    private void UpdateLevelInfo(int index = -1)
    {
        if (levelInfoTrans == null)
        {
            Debug.LogErrorFormat("level info trans is null");
            return;
        }

        Text curFindNum = levelInfoTrans.Find("Image/Text_LeftRisk/Text_LeftNum").GetComponent<Text>();
        curFindNum.text = $"{findNum}/{riskInfoList.Count}";

        Text levelTxt = levelInfoTrans.Find("Image/CurLevelText").GetComponent<Text>();
        levelTxt.text = $"当前进行：第{CurLevel}关，请识别隐患并点击";

        Slider slider = levelInfoTrans.Find("Image/Slider").GetComponent<Slider>();
        slider.value = findNum;

        Transform rightTipTrans = levelInfoTrans.Find("Image/Text_RightTip");
        if (index >= 0)
        {
            rightTipTrans.gameObject.SetActive(true);
            Text rightTip = rightTipTrans.GetComponent<Text>();
            rightTip.text = riskInfoList[index];
        }
        else
        {
            rightTipTrans.gameObject.SetActive(false);

        }

    }

    void IncrementTime()
    {
        bool updateTime = false;
        if (passTrans == null)
        {
            updateTime = true;
        }
        else
        {
            if (!passTrans.gameObject.activeSelf)
            {
                updateTime = true;
            }
        }
        if (updateTime)
        {

            if (levelInfoTrans == null)
            {
                Debug.LogErrorFormat("level info trans is null");
                return;
            }

            Text curFindNum = levelInfoTrans.Find("Image/CountDownText").GetComponent<Text>();
            curFindNum.text = $"已用时：{time}";
            time++;
        }

    }



    private void OnLevelPass()
    {
        if (passTrans == null)
        {
            LoadPanel("Panel/PassPanel", TopTrans, ref passTrans);
            var btn = passTrans.Find("Button").GetComponent<Button>();
            btn.onClick.AddListener(() => GoToNextLevel());
        }
        else
        {
            passTrans.gameObject.SetActive(true);
            passTrans.SetAsLastSibling();
        }
    }
    private void GoToNextLevel()
    {
        passTrans.gameObject.SetActive(false);
        CurLevel++;
        Debug.Log($" curLevel :{CurLevel}   count: {RiskInfo.RiskTexts.Count}");
        if (CurLevel > RiskInfo.RiskTexts.Count)
        {
            if (passTrans != null)
            {
                var btn = passTrans.Find("Button");
                btn.gameObject.SetActive(false);
                //全部通关

            }

        }
        else
        {
            ShowPanel();

        }
}

    private void HidePanel()
    {
        Destroy(panelTrans.gameObject);
    }

    private void ButtonAllClickHandler()
    {
        Debug.Log("Mouse Position: " + Input.mousePosition);
        if (wrongTrans != null && wrongTrans.gameObject.activeSelf)
        {
            return;
        }
        RectTransform parentRectTransform = panelTrans.Find("Image").GetComponent<RectTransform>();
        Vector2 localMousePosition;

        if (wrongTrans == null)
        {

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, Input.mousePosition, null, out localMousePosition))
            {
                Debug.Log("ButtonAllClickHandler" + localMousePosition);//���������Ϊ����

                GameObject go = Instantiate(wrongGo, parentRectTransform.TransformPoint(localMousePosition), Quaternion.identity) as GameObject;
                wrongTrans = go.GetComponent<RectTransform>();
                wrongTrans.SetParent(parentRectTransform, false);
                wrongTrans.anchoredPosition = localMousePosition;
            }
            else
            {
                Debug.LogError("Error converting mouse position to local position.");
            }

        }
        else
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, Input.mousePosition, null, out localMousePosition))
            {
                wrongTrans.anchoredPosition = localMousePosition;
                wrongTrans.gameObject.SetActive(true);
            }

        }
        //wrongGo.transform.localPosition = Input.mousePosition;
    }


    void LoadPanel(string assetPath, Transform parent, ref Transform panelTrans)
    {

        GameObject obj = Resources.Load<GameObject>(assetPath);
        Debug.Log("asset loaded: " + assetPath);
        GameObject go = Instantiate(obj);
        var trans = go.transform;
        trans.SetParent(parent, false);
        panelTrans = trans;

        //string bundlePath = "Assets/AssetBundles/level" + CurLevel.ToString();
        //AssetBundle myLoadedAssetBundle = AssetBundle.LoadFromFile(bundlePath);

        //if (myLoadedAssetBundle == null)
        //{
        //    Debug.Log("Failed to load AssetBundle!" + AssetName);
        //    return ;
        //}

        //// Load an asset from the bundle
        //GameObject obj = myLoadedAssetBundle.LoadAsset<GameObject>(AssetName);
        //if (obj == null)
        //{
        //    Debug.Log("Failed to LoadAsset!  " + AssetName);
        //    return;
        //}
        // Instantiate it
        // Always unload assetBundles when you are done with them.
        //myLoadedAssetBundle.Unload(false);
    }

    private void OnDestroy()
    {
        //
    }

}
