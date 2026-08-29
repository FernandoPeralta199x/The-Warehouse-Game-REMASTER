using System.Globalization;
using UnityEngine;

namespace TW08.UI.Hud
{
    /// <summary>
    /// Formatação de todo texto numérico do HUD.
    ///
    /// Está separada dos controllers porque as constantes de formato são
    /// consumidas por <c>UIMotion.CountTo</c>, que interpola o número e reaplica
    /// o formato a cada frame: o rótulo animado e o rótulo estático precisam
    /// nascer da mesma string, senão o valor final "salta" ao terminar o tween.
    /// </summary>
    public static class HudFormat
    {
        /// <summary>Formato do contador de movimentos animado.</summary>
        public const string MovesValueFormat = "MOVIMENTOS {0:000}";

        /// <summary>Formato do total de créditos do turno.</summary>
        public const string CreditsFormat = "+{0} CRÉDITOS";

        /// <summary>Formato do saldo acumulado da Oficina N-8.</summary>
        public const string BalanceFormat = "SALDO {0} CR";

        /// <summary>Formato do contador de usos restantes de uma ferramenta.</summary>
        public const string ToolUsesFormat = "x{0}";

        /// <summary>Formato do velocímetro da corrida.</summary>
        public const string SpeedFormat = "VEL {0:000}";

        /// <summary>Unidades por segundo por ponto do velocímetro.</summary>
        private const float SpeedScale = 14f;

        public static string LevelTitle(string sectorId, string displayName)
        {
            string sector = string.IsNullOrWhiteSpace(sectorId) ? "S--" : sectorId.Trim();
            string name = string.IsNullOrWhiteSpace(displayName) ? "ROTA SEM NOME" : displayName.Trim();
            return $"{sector} // {name}".ToUpperInvariant();
        }

        public static string Operator(string characterId)
        {
            string id = string.IsNullOrWhiteSpace(characterId) ? "--" : characterId.Trim();
            return "OPERADOR // " + id.ToUpperInvariant();
        }

        /// <summary>Rótulo composto legado, usado por cenas sem contador dedicado.</summary>
        public static string MoveSummary(int moves, int undoCount, int redoCount)
        {
            return $"MOVIMENTOS {Clamp(moves):000}   UNDO {Clamp(undoCount):00}   REDO {Clamp(redoCount):00}";
        }

        /// <summary>Histórico separado do contador animado de movimentos.</summary>
        public static string MoveHistory(int undoCount, int redoCount)
        {
            return $"UNDO {Clamp(undoCount):00}   REDO {Clamp(redoCount):00}";
        }

        public static string Targets(int platinumLimit, int goldLimit)
        {
            return $"PLAT {Clamp(platinumLimit):000} // GOLD {Clamp(goldLimit):000}";
        }

        /// <summary>Chip curto de ranking mostrado no painel superior do puzzle.</summary>
        public static string RankingChip(bool assisted)
        {
            return assisted ? "TURNO ASSISTIDO" : "TURNO LIMPO";
        }

        /// <summary>Linha longa de ranking mostrada junto da barra de ferramentas.</summary>
        public static string RankingLine(bool assisted)
        {
            return assisted ? "MODO ASSISTIDO // FORA DO RANKING" : "TURNO LIMPO // RANKING ATIVO";
        }

        public static string DoorNotice(string groupId, bool open)
        {
            string id = string.IsNullOrWhiteSpace(groupId) ? "--" : groupId.Trim().ToUpperInvariant();
            return open ? $"PORTA {id} ABERTA" : $"PORTA {id} FECHADA";
        }

        /// <summary>Valor assinado do extrato: o sinal precisa aparecer sempre.</summary>
        public static string Signed(int amount)
        {
            return amount < 0
                ? amount.ToString(CultureInfo.InvariantCulture)
                : "+" + amount.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Cronômetro da corrida. Sempre em cultura invariante: um separador
        /// decimal trocado pela localidade do jogador quebraria a leitura de
        /// tempos e a comparação visual com o recorde.
        /// </summary>
        public static string Time(float seconds)
        {
            seconds = Mathf.Max(0f, seconds);
            int minutes = Mathf.FloorToInt(seconds / 60f);
            float remainder = seconds - minutes * 60f;
            return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00.000}", minutes, remainder);
        }

        public static string BestTime(float seconds)
        {
            return seconds > 0f ? "BEST " + Time(seconds) : "BEST --:--.---";
        }

        public static string Lap(int currentLap, int totalLaps)
        {
            int total = Mathf.Max(1, totalLaps);
            int current = Mathf.Clamp(currentLap, 1, total);
            return $"VOLTA {current:00}/{total:00}";
        }

        public static string Position(int position, int racerCount)
        {
            int count = Mathf.Max(1, racerCount);
            return position > 0 ? $"POS {position:00}/{count:00}" : $"POS --/{count:00}";
        }

        public static string CargoIntegrity(float normalizedIntegrity, bool lost)
        {
            if (lost)
            {
                return "CARGA // PERDIDA";
            }

            int percent = Mathf.RoundToInt(Mathf.Clamp01(normalizedIntegrity) * 100f);
            return $"CARGA // {percent:000}%";
        }

        public static string Item(string displayName)
        {
            return string.IsNullOrWhiteSpace(displayName)
                ? "ITEM // --"
                : "ITEM // " + displayName.Trim().ToUpperInvariant();
        }

        /// <summary>Converte velocidade física em leitura inteira do velocímetro.</summary>
        public static int SpeedReading(float unitsPerSecond)
        {
            return Mathf.Max(0, Mathf.RoundToInt(Mathf.Max(0f, unitsPerSecond) * SpeedScale));
        }

        public static string Speed(float unitsPerSecond)
        {
            return string.Format(CultureInfo.InvariantCulture, SpeedFormat, SpeedReading(unitsPerSecond));
        }

        public static string Credits(int amount)
        {
            return string.Format(CultureInfo.InvariantCulture, CreditsFormat, amount);
        }

        public static string Balance(int amount)
        {
            return string.Format(CultureInfo.InvariantCulture, BalanceFormat, amount);
        }

        private static int Clamp(int value)
        {
            return value < 0 ? 0 : value;
        }
    }
}
