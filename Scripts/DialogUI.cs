﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// A constructor for a dialog
public class Dialog
{
    public string Title = "Title";
    public string Message = "Message";
    public bool AcceptOnly = true;
    public UnityAction OnClose;
    public UnityAction OnAccepted;
}

public class DialogUI : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] Text titleUIText;
    [SerializeField] Text messageUIText;
    [SerializeField] Button closeUIButton;
    [SerializeField] Button acceptUIButton;
    [SerializeField] Button acceptOnlyUIButton;

    [SerializeField] GameObject acceptOnlyUI;
    [SerializeField] GameObject bothOptionsUI;

    Dialog dialog = new Dialog();

    public static DialogUI Instance;

    private void Awake()
    {
        Instance = this;
        closeUIButton.onClick.RemoveAllListeners();
        closeUIButton.onClick.AddListener(Hide);
        acceptUIButton.onClick.RemoveAllListeners();
        acceptUIButton.onClick.AddListener(Accept);
        acceptOnlyUIButton.onClick.RemoveAllListeners();
        acceptOnlyUIButton.onClick.AddListener(Accept);
    }

    public DialogUI SetTitle(string title)
    {
        dialog.Title = title;
        return Instance;
    }

    public DialogUI SetMessage(string message)
    {
        dialog.Message = message;
        return Instance;
    }

    public DialogUI AcceptOnly(bool _bool)
    {
        dialog.AcceptOnly = _bool;
        return Instance;
    }

    public DialogUI OnClose(UnityAction action)
    {
        dialog.OnClose = action;
        return Instance;
    }

    public DialogUI OnAccept(UnityAction action)
    {
        dialog.OnAccepted = action;
        return Instance;
    }

    public void Show()
    {
        titleUIText.text = dialog.Title;
        messageUIText.text = dialog.Message;

        if (dialog.AcceptOnly == true)
        {
            acceptOnlyUI.SetActive(true);
            bothOptionsUI.SetActive(false);
        }
        else
        {
            acceptOnlyUI.SetActive(false);
            bothOptionsUI.SetActive(true);
        }

        canvas.SetActive(true);
    }

    public void Hide()
    {
        canvas.SetActive(false);

        if (dialog.OnClose != null)
        {
            dialog.OnClose.Invoke();
        }

        dialog = new Dialog();
    }

    public void Accept()
    {
        canvas.SetActive(false);

        if (dialog.OnAccepted != null)
        {
            dialog.OnAccepted.Invoke();
        }

        dialog = new Dialog();
    }
}
