using UnityEngine;
using Unity.Services.Multiplayer;
using System.Collections.Generic;

public class SessionListUI : MonoBehaviour
{
    [SerializeField] Transform sessionListAnchor;
    [SerializeField] SessionListUIElement sessionListElement;
    List<SessionListUIElement> sessionListElements = new();
    private void OnEnable()
    {
        Refresh();
    }
    public async void Refresh()
    {
        IList<ISessionInfo> infoList = await SessionManager.Instance.QuerySessions();
        int i;
        for (i = 0; i < infoList.Count; i++)
        {
            if (sessionListElements.Count <= i)
            {
                sessionListElements.Add(Instantiate(sessionListElement, sessionListAnchor));
            }
            sessionListElements[i].Set(infoList[i]);
            sessionListElements[i].gameObject.SetActive(true);
        }
        for (; i < sessionListElements.Count; i++)
        {
            sessionListElements[i].gameObject.SetActive(false);
        }
    }
}