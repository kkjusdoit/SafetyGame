using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
// using QFramework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using YooAsset;

public class Game : MonoBehaviour
{
    [SerializeField] private Transform BottomTrans;
    [SerializeField] private Transform MidTrans;
    [SerializeField] private Transform TopTrans;

    private int CurLevel = 1;
    public GameObject wrongGo;

    public GameObject rightGo;

    private RectTransform wrongTrans = null;

    private int _findNum = 0;
    //private bool isShowButton = false;

    Transform panelTrans;  //界面
    Transform passTrans; //通关
    Transform levelInfoTrans;//关卡信息
    List<string> riskInfoList;

    public float time;

    private ResourcePackage package;

    private IEnumerator Start()
    {
        yield return InitYooAsset();
        
        // StartCoroutine(LoadBundle("level1" ,"LevelInfo"));
        // ResKit.Init();
        Debug.Log("start load");
        
        var op = package.RequestPackageVersionAsync();

        yield return op;
        yield return package.UpdatePackageManifestAsync(op.PackageVersion);

        //加载panel
        yield return ShowPanel();
        InvokeRepeating("IncrementTime", 1f, 1f);
        

        
    }

    private IEnumerator InitYooAsset()
    {
        // 初始化资源系统
        YooAssets.Initialize();
        package = YooAssets.CreatePackage("DefaultPackage");

        //// 获取指定的资源包，如果没有找到会报错
        package = YooAssets.GetPackage("DefaultPackage");
        // 创建默认的资源包
        // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
        YooAssets.SetDefaultPackage(package);
        

        var webFileSystem = FileSystemParameters.CreateDefaultWebFileSystemParameters();
        var initParameters = new WebPlayModeParameters();
        initParameters.WebFileSystemParameters = webFileSystem;
        var initOperation = package.InitializeAsync(initParameters);
        yield return initOperation;
    
        if(initOperation.Status == EOperationStatus.Succeed)
            Debug.Log("资源包初始化成功！");
        else 
            Debug.LogError($"资源包初始化失败：{initOperation.Error}");

        //
        // if(initOperation.Status == EOperationStatus.Succeed)
        //     Debug.Log("资源包初始化成功！");
        // else 
        //     Debug.LogError($"资源包初始化失败：{initOperation.Error}");
    }
    


    private IEnumerator ShowPanel()
    {
        if (panelTrans == null)
        {
            string prefabName = "Panel" + CurLevel.ToString();
            riskInfoList = RiskInfo.RiskTexts[CurLevel - 1];
            // string assetPath = string.Format("Panel/Levels/{0}", prefabName);
            yield return LoadPanel(prefabName, BottomTrans, (loadedTrasform) => panelTrans = loadedTrasform);
            if (panelTrans == null)
            {
                Debug.LogError("load panel failed");
                yield return null;
            }

            //show level info
            if (levelInfoTrans == null)
            {
                yield return LoadPanel("LevelInfo", MidTrans, (trans) => levelInfoTrans = trans);
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
            StartCoroutine(ShowPanel());
        }
    }

    private void Reset()
    {
        Debug.Log("reset: " + _findNum);
        _findNum = 0;
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
        if (passTrans != null && passTrans.gameObject.activeSelf)
        {
            Debug.LogError("passTrans.gameObject");
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

            _findNum += 1;
            Debug.Log("find num:" + _findNum);

            //更新关卡信息
            UpdateLevelInfo(v);

            //todolkk:12通关，恭喜
            if (_findNum >= riskInfoList.Count)
            {
                StartCoroutine(OnLevelPass());
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

        Text curFindNum = levelInfoTrans.Find("Slider/Text_LeftRisk/Text_LeftNum").GetComponent<Text>();
        var txt = $"{_findNum}/{riskInfoList.Count}";
        curFindNum.text = txt;
        Debug.Log("findNum/ total  " + txt);

        Text levelTxt = levelInfoTrans.Find("CurLevelText").GetComponent<Text>();
        levelTxt.text = $"当前进行：第{CurLevel}关，请识别隐患并点击";

        // Slider slider = levelInfoTrans.Find("Slider").GetComponent<Slider>();
        // slider.value = _findNum;

        Transform rightTipTrans = levelInfoTrans.Find("Slider/Text_RightTip");
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

            Text curFindNum = levelInfoTrans.Find("CountDownText").GetComponent<Text>();
            curFindNum.text = $"已用时：{time}";
            time++;
        }

    }

    private void OnPassPanelLoaded(Transform trans)
    {
        passTrans = trans;
        Debug.Log("load level pass succ");
        var btn = passTrans.Find("Button").GetComponent<Button>();

        btn.onClick.AddListener(() => GoToNextLevel());
    }



    private IEnumerator OnLevelPass()
    {
        if (passTrans == null)
        {
            yield return LoadPanel("PassPanel", TopTrans, OnPassPanelLoaded);

        }
        else
        {
            passTrans.gameObject.SetActive(true);
            passTrans.SetAsLastSibling();
        }

        var isPassAll = CurLevel >= RiskInfo.RiskTexts.Count;
        passTrans.Find("RawImage").gameObject.SetActive(isPassAll);
        passTrans.Find("Button").gameObject.SetActive(!isPassAll);
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
                Debug.LogError("Congratualations all levels passed");
            }

        }
        else
        {
            StartCoroutine(ShowPanel());

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


    IEnumerator LoadPanel(string assetName, Transform parent, Action<Transform> onLoaded)
    {
        Debug.Log("begin load panel  " + assetName);
        AssetHandle handle = package.LoadAssetAsync<GameObject>(assetName);
        yield return handle;
        GameObject go = handle.InstantiateSync();
        Debug.Log($"Prefab name is {go.name}");
        var trans = go.transform;
        trans.SetParent(parent, false);
        onLoaded?.Invoke(trans);
    }
    
    
    private void OnDestroy()
    {
        //销毁资源包对象
        DestroyPackage();
    }
    private IEnumerator DestroyPackage()
    {
        // 先销毁资源包
        package = YooAssets.GetPackage("DefaultPackage");
        DestroyOperation operation = package.DestroyAsync();
        yield return null;
    
        // 然后移除资源包
        if (YooAssets.RemovePackage("DefaultPackage"))
        {
            Debug.Log("移除成功！");
        }
    }
}
