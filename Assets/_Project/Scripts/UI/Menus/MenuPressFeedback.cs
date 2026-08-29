using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TW08.UI.Menus
{
    /// <summary>
    /// Pulso de confirmação em qualquer botão de menu, no mouse e no teclado/gamepad.
    ///
    /// Fica num componente próprio porque o EventSystem só entrega
    /// <c>submit</c> ao objeto selecionado e <c>pointerDown</c> ao alvo do raycast:
    /// escutar de um pai não recebe os dois. Também não usa
    /// <c>onClick.AddListener</c> — os controladores de seleção chamam
    /// <c>RemoveAllListeners</c> ao religar a tela e apagariam o efeito.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MenuPressFeedback : MonoBehaviour, IPointerDownHandler, ISubmitHandler
    {
        [SerializeField] private Selectable target;

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponent<Selectable>();
            }
        }

        public void OnPointerDown(PointerEventData eventData) => Pulse();

        public void OnSubmit(BaseEventData eventData) => Pulse();

        private void Pulse()
        {
            if (target != null && !target.IsInteractable())
            {
                MenuFeedback.Denied(this);
                return;
            }

            MenuFeedback.Click(this);
        }
    }
}
