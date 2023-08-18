using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    [SerializeField] private Transform panel1;

    private int CurLevel = 1;
    public GameObject wrongGo;

    public GameObject rightGo;

    private RectTransform wrongTrans = null;

    private int findNum = 0;
    private bool isShowButton = false;


    private void Start()
    {
        // 获取panel宽高
        SetImageScale();



        //if (ratioHeight <= 1 && ratioHeight <= 1)
        //{
        //    //选个大的


        //}
        //else
        //{
        //    float = 
        //}

        if (panel1.Find("Image/BtnAll").TryGetComponent<Button>(out var btnAll))
        {
            btnAll.onClick.AddListener(ButtonAllClickHandler);
        }
        else
        {
            Debug.LogError("Button not found.");
        }
        if (panel1.Find("Image/ButtonTest").TryGetComponent<Button>(out var btnTest))
        {
            btnTest.onClick.AddListener(ButtonTestClickHandler);
        }
        else
        {
            Debug.LogError("Button not found.");
        }
        

        for (int i = 0; i < Level2.totalRiskNum; i++)
        {
            var trans = panel1.Find("Image/Btn" + (i + 1).ToString());
            if (trans.TryGetComponent<Button>(out var btn))
            {
                int index = i + 1;
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
        Transform imgTrans = panel1.Find("Image");
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
        for (int i = 0; i < Level2.totalRiskNum; i++)
        {
           var trans = panel1.Find("Image/Btn" + (i + 1).ToString());
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

        //SetImageScale();
        //以1920x1080为基准，屏幕根据宽度的不同等比例缩放，适配控件的大小
        //public void RectAdaptX(RectTransform rect)
        //{

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
            Text curFindNum = panel1.Find("Image/Text_LeftRisk/Text_LeftNum").GetComponent<Text>();
            curFindNum.text = $"{findNum}/{Level2.totalRiskNum}";

            Slider slider = panel1.Find("Image/Slider").GetComponent<Slider>();
            slider.value = findNum;

            Text rightTip = panel1.Find("Image/Text_RightTip").GetComponent<Text>();

            string fieldname = string.Format("risk_{0}_info", v);
            Type type = typeof(Level2);
            FieldInfo field = type.GetField(fieldname, BindingFlags.Static | BindingFlags.Public);
            string value = (string)field.GetValue(null);
            Debug.Log($"{fieldname}{ value}");
            rightTip.text = value;


            //todo:12通关，恭喜
            if (findNum >= Level2.totalRiskNum)
            {
                panel1.Find("RawImage").gameObject.SetActive(true); 
            }
        }
    }

    private void ButtonAllClickHandler()
    {
        Debug.Log("Mouse Position: " + Input.mousePosition);
        if (wrongTrans != null && wrongTrans.gameObject.activeSelf)
        {
            return;
        }
        RectTransform parentRectTransform = panel1.Find("Image").GetComponent<RectTransform>();
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
}
