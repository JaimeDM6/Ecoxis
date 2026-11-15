using UnityEngine;
using UnityEngine.EventSystems;

public class BotonTactilFenn : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum Accion { Izquierda, Derecha, Salto }
    public Accion accion;
    public Personaje personaje;
    private bool pulsado;
    private int dedo;

    public void OnPointerDown(PointerEventData evento)
    {
        if (pulsado || personaje == null || !personaje.isActiveAndEnabled || Time.timeScale <= 0f) return;
        pulsado = true;
        dedo = evento.pointerId;
        if (accion == Accion.Izquierda) personaje.PulsarIzquierda(true);
        if (accion == Accion.Derecha) personaje.PulsarDerecha(true);
        if (accion == Accion.Salto) personaje.Jump();
    }

    public void OnPointerUp(PointerEventData evento)
    {
        if (pulsado && evento.pointerId == dedo) Soltar();
    }

    public void OnPointerExit(PointerEventData evento)
    {
        if (pulsado && evento.pointerId == dedo) Soltar();
    }

    private void Soltar()
    {
        pulsado = false;
        if (personaje == null) return;
        if (accion == Accion.Izquierda) personaje.PulsarIzquierda(false);
        if (accion == Accion.Derecha) personaje.PulsarDerecha(false);
    }

    private void OnDisable() { Soltar(); }
    private void OnApplicationFocus(bool foco) { if (!foco) Soltar(); }
    private void OnApplicationPause(bool pausa) { if (pausa) Soltar(); }
}
