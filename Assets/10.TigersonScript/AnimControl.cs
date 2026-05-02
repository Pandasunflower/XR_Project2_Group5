using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimControl : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        // 抓取模型身上的 Animator 元件
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // 當按下鍵盤的「空白鍵」時
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 觸發名為 DoAction 的開關，讓箭頭放行
            anim.SetTrigger("DoAction"); 
            
            // 備註：如果你剛剛參數類型選的是 Bool，請把上一行換成這行：
            // anim.SetBool("DoAction", true);
        }
    }
}
