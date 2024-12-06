using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Radio : AInteractableObject
{
    [SerializeField] private List<GameObject> _radioWheels;
    [SerializeField] private LobbyInteraction _lobbyInteraction;

    private List<float> _targetRot;
    private List<float> _currentRot;
    private float _rotSpeed = 100f;
    private bool _WheelsMoving = false;

    private void Start()
    {
        _targetRot = new List<float>();
        _currentRot = new List<float>();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Radio pulsada");
    }

    private void Update()
    {
        if (_WheelsMoving)
        {
            for (int i = 0; i < _radioWheels.Count; i++)
            {
                if (_radioWheels[i] != null && (_targetRot[i] - _currentRot[i] > 0.1f))
                {
                    RotateWheels(i);
                }
            }

            if (AllWheelsStopped())
            {
                _WheelsMoving = false;
            }
        }
    }

    private void RotateWheels(int i)
    {
        float step = Mathf.Min(Time.deltaTime * _rotSpeed, _targetRot[i] - _currentRot[i]); 
        _radioWheels[i].transform.rotation = Quaternion.AngleAxis(step, Vector3.forward) * _radioWheels[i].transform.rotation;
        _currentRot[i] += step;
    }

    private bool AllWheelsStopped()
    {
        for (int i = 0; i < _targetRot.Count; i++)
        {
            if (Mathf.Abs(_targetRot[i] - _currentRot[i]) > 0.1f)
                return false;
        }
        return true;
    }

    public void OnButtonCreateLobby()
    {
        Debug.Log("Crear lobby pulsado");

        if (_lobbyInteraction != null)
        {
            _lobbyInteraction.HostButton();
        }

        _targetRot.Clear();
        _currentRot.Clear();

        for (int i = 0; i < _radioWheels.Count; i++)
        {
            if (_radioWheels[i] != null)
            {
                _targetRot.Add(Random.Range(45f, 270f));
                _currentRot.Add(0f);
            }
        }

        _WheelsMoving = true;
    }

    public void OnButtonJoinLobby()
    {
        // Join lobby
        Debug.Log("Join lobby pulsado");

        // te deja escribir en el cuadro de texto (se activa o como lo haga)
        _lobbyInteraction.ClientButton();
    }


}
