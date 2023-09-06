using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    [SerializeField] private Transform CanvasTrans;

    private int CurLevel = 1;
    public GameObject wrongGo;

    public GameObject rightGo;

    private RectTransform wrongTrans = null;

    private int findNum = 0;
    private bool isShowButton = false;

    Transform panelTrans;  //界面
    Transform passTrans; //通关
    List<string> riskInfoList;

    private void Start()
    {
        //加载panel
        ShowPanel();
    }

    private void ShowPanel()
    {
        if (panelTrans == null)
        {
            string prefabName = "Panel" + CurLevel.ToString();
            riskInfoList = RiskInfo.RiskTexts[CurLevel - 1];
            string assetPath = string.Format("Panel/Levels/{0}", prefabName);
            LoadPanel(assetPath, CanvasTrans, ref panelTrans);
            if (panelTrans == null)
            {
                Debug.LogError("load panel failed");
                return;
            }

            //控件注册
            BindBtn();

            SetImageScale();
        }
        else  //如果当前有就关闭
        {
            Destroy(panelTrans.gameObject);
            panelTrans = null;
            ShowPanel();
        }
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
        if (panelTrans.Find("Image/ButtonTest").TryGetComponent<Button>(out var btnTest))
        {
            btnTest.onClick.AddListener(ButtonTestClickHandler);
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

    private void ButtonTestClickHandler()
    {
        for (int i = 0; i < riskInfoList.Count; i++)
        {
           var trans = panelTrans.Find("Image/Btn" + (i + 1).ToString());
           if (trans.TryGetComponent<Image>(out Image img))
           {
                //img.enabled = !img.enabled;
                if (isShowButton)
                {
                    img.color = new Color(1, 0, 0, 1);

                }
                else
                {
                    img.color = Color.clear;

                }
                isShowButton = !isShowButton;
            }
            else
           {
               Debug.LogError("Button not found.");
           }
        }
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
            Text curFindNum = panelTrans.Find("Image/Text_LeftRisk/Text_LeftNum").GetComponent<Text>();
            curFindNum.text = $"{findNum}/{riskInfoList.Count}";

            Slider slider = panelTrans.Find("Image/Slider").GetComponent<Slider>();
            slider.value = findNum;

            Text rightTip = panelTrans.Find("Image/Text_RightTip").GetComponent<Text>();

            //string fieldname = string.Format("risk_{0}_info", v);
            //Type type = typeof(Level2);
            //FieldInfo field = type.GetField(fieldname, BindingFlags.Static | BindingFlags.Public);
            //string value = (string)field.GetValue(null);
            //Debug.Log($"{fieldname}{ value}");
            rightTip.text = riskInfoList[v];


            //todolkk:12通关，恭喜
            if (findNum >= 1)//riskInfoList.Count)
            {
                //panelTrans.Find("RawImage").gameObject.SetActive(true); 
                OnLevelPass();
            }
        }
    }

    private void OnLevelPass()
    {
        if (passTrans == null)
        {
            LoadPanel("Panel/PassPanel", CanvasTrans, ref passTrans);
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
        if (riskInfoList.Count <= CurLevel)
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
