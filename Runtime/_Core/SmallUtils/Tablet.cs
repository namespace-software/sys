using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tablet : MonoBehaviour
{
    [SerializeField] private int _docIndex = 0;
    [SerializeField] private List<GameObject> _documents;
    [SerializeField] private Button _nextBtn;
    [SerializeField] private Button _PreviousBtn;
    private bool _activated;

    public void NextSlide()
    {
        _documents[_docIndex].SetActive(false);
        _docIndex++;
        _documents[_docIndex].SetActive(true);


        CheckNextStepPossiblility();
    }

    public void PreviousSlide()
    {
        _documents[_docIndex].SetActive(false);
        _docIndex--;
        _documents[_docIndex].SetActive(true);

        CheckNextStepPossiblility();
    }

    public void FirstAction()
    {
        if (_activated) return;
        //Debug.LogError("FirstAction()");
        _documents[_docIndex].SetActive(true);
        _activated = true;
    }

    private void CheckNextStepPossiblility()
    {
        _PreviousBtn.interactable = _docIndex != 0;
        _nextBtn.interactable = _docIndex != _documents.Count - 1;
    }
}
