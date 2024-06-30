using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{

    void Start(){
        NextScene();
    }

    private AsyncOperation async;

    [SerializeField]
    private GameObject loadUI;

    [SerializeField]
    private Slider slider;

    public void NextScene(){
       loadUI.SetActive(true);
       StartCoroutine("LoadData");
    }

    IEnumerator LoadData(){
        yield return new WaitForSeconds(1.0f);
        async = SceneManager.LoadSceneAsync("Main");

        while (!async.isDone){
            var progressVal = Mathf.Clamp01(async.progress / 0.9f);
            slider.value = progressVal;            
            yield return null;
        }        
    }
}