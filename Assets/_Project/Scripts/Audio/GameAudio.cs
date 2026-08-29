using UnityEngine;

namespace TW08.Audio
{
    /// <summary>
    /// Acesso ao catálogo de áudio de qualquer lugar, sem exigir referência
    /// serializada.
    ///
    /// Existe porque menu, loja e narrativa ficavam mudos: eram sistemas sem
    /// campo para o catálogo, e ligar som neles exigiria refazer a fiação de
    /// todas as cenas. O catálogo mora em Resources e é carregado uma vez.
    ///
    /// Nada aqui lança quando o catálogo falta: som é decoração, e um projeto
    /// sem áudio gerado precisa continuar jogável.
    /// </summary>
    public static class GameAudio
    {
        public const string ResourcePath = "TW08_AudioCatalog";

        private static TW08AudioCatalog cached;
        private static bool searched;

        public static TW08AudioCatalog Catalog
        {
            get
            {
                if (cached != null)
                {
                    return cached;
                }

                // Uma tentativa por sessão: Resources.Load falhando a cada
                // clique de menu seria custo por nada.
                if (!searched)
                {
                    searched = true;
                    cached = Resources.Load<TW08AudioCatalog>(ResourcePath);
                }

                return cached;
            }
        }

        public static void Play(AudioEvent audioEvent)
        {
            if (audioEvent != null && AudioService.Instance != null)
            {
                AudioService.Instance.PlayOneShot(audioEvent);
            }
        }

        /// <summary>Toca depois de um atraso, para separar causa e efeito.</summary>
        public static void PlayDelayed(AudioEvent audioEvent, float delaySeconds)
        {
            if (audioEvent == null || AudioService.Instance == null)
            {
                return;
            }

            AudioService.Instance.PlayOneShotDelayed(audioEvent, delaySeconds);
        }

        // Atalhos dos eventos de interface, usados por menu, loja e narrativa.
        public static void Focus() => Play(Catalog?.UiFocus);
        public static void Confirm() => Play(Catalog?.UiConfirm);
        public static void Back() => Play(Catalog?.UiBack);
        public static void Denied() => Play(Catalog?.UiDenied);
        public static void TerminalBoot() => Play(Catalog?.TerminalBoot);
        public static void ShopPurchase() => Play(Catalog?.ShopPurchase);
        public static void CreditsTick() => Play(Catalog?.CreditsTick);

        /// <summary>Marcador de fala do personagem que está falando.</summary>
        public static void Voice(string speakerId) => Play(Catalog?.VoiceFor(speakerId));

        /// <summary>Limpa o cache — usado ao regenerar o catálogo no editor.</summary>
        public static void Invalidate()
        {
            cached = null;
            searched = false;
        }
    }
}
