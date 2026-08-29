using TW08.Motion;
using UnityEngine.UI;

namespace TW08.UI.Hud
{
    /// <summary>
    /// Rótulo numérico que conta do valor antigo até o novo em vez de trocar de
    /// número na marra.
    ///
    /// Guarda o valor lógico separado do texto exibido: se um tween for
    /// interrompido no meio, a próxima contagem parte do número correto e não
    /// do que estava pintado na tela naquele frame.
    /// </summary>
    public sealed class AnimatedCounter
    {
        private readonly string format;
        private readonly float duration;
        private readonly Ease ease;

        private Text label;
        private MotionHandle handle;
        private int current;
        private bool primed;

        public AnimatedCounter(string valueFormat = "{0}", float countDuration = 0.32f, Ease countEase = Ease.OutCubic)
        {
            format = string.IsNullOrEmpty(valueFormat) ? "{0}" : valueFormat;
            duration = countDuration;
            ease = countEase;
        }

        /// <summary>Último valor lógico, independente do que o tween já pintou.</summary>
        public int Value => current;

        /// <summary>True depois do primeiro valor: antes disso não há de onde contar.</summary>
        public bool Primed => primed;

        public void Attach(Text target)
        {
            if (label == target)
            {
                return;
            }

            Stop();
            label = target;
            primed = false;
            current = 0;
        }

        /// <summary>Escreve o valor sem animar — abertura de fase, reset, troca de cena.</summary>
        public void SetImmediate(int value)
        {
            handle?.Kill();
            handle = null;
            current = value;
            primed = true;

            if (label != null)
            {
                label.text = string.Format(format, value);
            }
        }

        /// <summary>
        /// Anima até <paramref name="value"/>. Devolve true quando o número
        /// realmente mudou, para o chamador decidir se acompanha com um pulso.
        /// </summary>
        public bool Set(int value)
        {
            if (label == null)
            {
                current = value;
                primed = true;
                return false;
            }

            if (!primed)
            {
                SetImmediate(value);
                return false;
            }

            if (value == current)
            {
                return false;
            }

            int from = current;
            current = value;
            handle?.Kill();
            handle = UIMotion.CountTo(label, from, value, duration, format, ease);
            return true;
        }

        /// <summary>Encerra o tween aplicando o valor final — chamar em OnDisable.</summary>
        public void Stop()
        {
            handle?.Complete();
            handle = null;
        }
    }
}
