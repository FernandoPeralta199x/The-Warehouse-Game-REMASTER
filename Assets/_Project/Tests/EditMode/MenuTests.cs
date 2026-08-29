using NUnit.Framework;
using TW08.UI;
using TW08.UI.Menus;

namespace TW08.Tests.EditMode
{
    /// <summary>
    /// Regras puras das telas de menu: rótulos dos cartões de fase, cores por
    /// estado de progresso, enquadramento do scroll por teclado, rolagem em
    /// loop dos créditos e as regras de vitrine da Oficina N-8.
    /// </summary>
    public sealed class MenuTests
    {
        // ------------------------------------------ Cartões da grade de fases --

        [Test]
        public void LevelCard_ShowsBestRunOnceTheLevelWasCleared()
        {
            string label = PuzzleLevelSelectController.FormatLevelLabel(
                index: 10, displayName: "Piso Gelado", hasEntry: true, unlocked: true, bestMoves: 21, medal: 3);

            StringAssert.StartsWith("11 // PISO GELADO", label);
            StringAssert.Contains("BEST 021", label);
            StringAssert.Contains("M3", label);
        }

        [Test]
        public void LevelCard_DistinguishesLockedFromUnplayed()
        {
            string locked = PuzzleLevelSelectController.FormatLevelLabel(
                1, "Corredor Apertado", hasEntry: true, unlocked: false, bestMoves: 0, medal: 0);
            string fresh = PuzzleLevelSelectController.FormatLevelLabel(
                1, "Corredor Apertado", hasEntry: true, unlocked: true, bestMoves: 0, medal: 0);

            StringAssert.Contains("BLOQUEADO", locked);
            StringAssert.Contains("ROTA DISPONÍVEL", fresh);
        }

        [Test]
        public void LevelCard_FallsBackToAGenericNameWhenTheLevelHasNone()
        {
            string label = PuzzleLevelSelectController.FormatLevelLabel(
                4, "   ", hasEntry: true, unlocked: true, bestMoves: 0, medal: 0);

            StringAssert.StartsWith("05 // ROTA 05", label);
        }

        [Test]
        public void LevelCard_EmptySlotStaysNeutral()
        {
            Assert.AreEqual("--", PuzzleLevelSelectController.FormatLevelLabel(
                0, "qualquer", hasEntry: false, unlocked: true, bestMoves: 12, medal: 3));
            Assert.AreEqual(
                PuzzleLevelSelectController.EmptyTint,
                PuzzleLevelSelectController.LabelTint(hasEntry: false, unlocked: true, medal: 3, isNext: true));
        }

        [Test]
        public void CardTint_MedalOutranksTheNextLevelHighlight()
        {
            // Uma fase já premiada mantém a cor da medalha mesmo sendo a próxima
            // da fila: perder o registro de platina na grade seria pior do que
            // perder o realce de "jogue esta agora".
            Assert.AreEqual(
                PuzzleLevelSelectController.PlatinumTint,
                PuzzleLevelSelectController.LabelTint(true, true, medal: 3, isNext: true));
            Assert.AreEqual(
                PuzzleLevelSelectController.GoldTint,
                PuzzleLevelSelectController.LabelTint(true, true, medal: 2, isNext: false));
            Assert.AreEqual(
                PuzzleLevelSelectController.BronzeTint,
                PuzzleLevelSelectController.LabelTint(true, true, medal: 1, isNext: false));
        }

        [Test]
        public void CardTint_UnplayedNextLevelIsHighlighted()
        {
            Assert.AreEqual(
                PuzzleLevelSelectController.CurrentTint,
                PuzzleLevelSelectController.LabelTint(true, true, medal: 0, isNext: true));
            Assert.AreEqual(
                PuzzleLevelSelectController.AvailableTint,
                PuzzleLevelSelectController.LabelTint(true, true, medal: 0, isNext: false));
            Assert.AreEqual(
                PuzzleLevelSelectController.LockedTint,
                PuzzleLevelSelectController.LabelTint(true, false, medal: 0, isNext: false));
        }

        // ---------------------------------------------- Scroll por teclado --

        [Test]
        public void Scroll_DoesNotMoveWhenTheItemIsAlreadyFramed()
        {
            float top = ScrollToSelected.ComputeDesiredTop(
                itemCenter: 200f, itemHalf: 50f, viewTop: 100f, viewHeight: 400f, margin: 10f);

            Assert.AreEqual(100f, top, "Item já enquadrado não deve provocar rolagem.");
        }

        [Test]
        public void Scroll_PullsTheViewportToReachItemsAboveAndBelow()
        {
            float above = ScrollToSelected.ComputeDesiredTop(
                itemCenter: 40f, itemHalf: 30f, viewTop: 200f, viewHeight: 400f, margin: 10f);
            Assert.AreEqual(0f, above, "Item acima: a viewport sobe até o topo dele.");

            float below = ScrollToSelected.ComputeDesiredTop(
                itemCenter: 700f, itemHalf: 30f, viewTop: 0f, viewHeight: 400f, margin: 10f);
            Assert.AreEqual(340f, below, "Item abaixo: a viewport desce só o necessário.");
        }

        [Test]
        public void Scroll_NormalizedPositionIsInvertedAndClamped()
        {
            // O ScrollRect vertical usa 1 no topo e 0 embaixo.
            Assert.AreEqual(1f, ScrollToSelected.NormalizedFromTop(0f, 500f), 0.0001f);
            Assert.AreEqual(0f, ScrollToSelected.NormalizedFromTop(500f, 500f), 0.0001f);
            Assert.AreEqual(0.5f, ScrollToSelected.NormalizedFromTop(250f, 500f), 0.0001f);
            Assert.AreEqual(0f, ScrollToSelected.NormalizedFromTop(9999f, 500f), 0.0001f);
        }

        [Test]
        public void Scroll_SurvivesContentShorterThanTheViewport()
        {
            // Conteúdo menor que a viewport dá scrollable 0, que dividiria por
            // zero; a regra usa um piso de 1.
            Assert.DoesNotThrow(() => ScrollToSelected.NormalizedFromTop(120f, 0f));
        }

        // ---------------------------------------------------------- Créditos --

        [Test]
        public void Credits_ScrollWrapsAroundInsteadOfRunningOff()
        {
            Assert.AreEqual(0f, CreditsScreenController.LoopOffset(0f, 700f), 0.0001f);
            Assert.AreEqual(300f, CreditsScreenController.LoopOffset(300f, 700f), 0.0001f);
            Assert.AreEqual(50f, CreditsScreenController.LoopOffset(750f, 700f), 0.0001f);
        }

        [Test]
        public void Credits_LoopOfZeroKeepsTheTextStill()
        {
            Assert.AreEqual(0f, CreditsScreenController.LoopOffset(420f, 0f), 0.0001f);
            Assert.AreEqual(0f, CreditsScreenController.LoopOffset(-50f, 700f), 0.0001f);
        }

        // -------------------------------------------------------- Oficina N-8 --

        [Test]
        public void Shop_AffordabilityAndShortfall()
        {
            Assert.IsTrue(ShopController.CanAfford(80, 80));
            Assert.IsFalse(ShopController.CanAfford(79, 80));
            Assert.AreEqual(1, ShopController.MissingCredits(79, 80));
            Assert.AreEqual(0, ShopController.MissingCredits(200, 80), "Saldo de sobra não gera falta negativa.");
        }

        [Test]
        public void Shop_EquipRequiresStockUnlessAlreadyEquipped()
        {
            Assert.IsFalse(ShopController.CanEquipRow(owned: 0, equipped: false));
            Assert.IsTrue(ShopController.CanEquipRow(owned: 1, equipped: false));
            // Ferramenta equipada com estoque zerado ainda precisa poder ser removida.
            Assert.IsTrue(ShopController.CanEquipRow(owned: 0, equipped: true));
        }

        [Test]
        public void Shop_SlotLimitBlocksTheThirdTool()
        {
            Assert.IsTrue(ShopController.HasFreeSlot(equippedCount: 1, slots: 2));
            Assert.IsFalse(ShopController.HasFreeSlot(equippedCount: 2, slots: 2));
            Assert.IsTrue(ShopController.HasFreeSlot(equippedCount: 0, slots: 0), "Slots inválidos caem para 1.");
        }

        [Test]
        public void Shop_LabelsReadAsTheTerminalWouldPrintThem()
        {
            Assert.AreEqual("CRÉDITOS DE TURNO // 450", ShopController.FormatCredits(450));
            Assert.AreEqual("SLOTS DE FERRAMENTA // 1/2", ShopController.FormatSlots(1, 2));
            Assert.AreEqual("COMPRAR // 80", ShopController.BuyLabel(80));
            Assert.AreEqual("EQUIPADA", ShopController.EquipLabel(true));
            Assert.AreEqual("EQUIPAR", ShopController.EquipLabel(false));
            StringAssert.Contains("ESTOQUE 2", ShopController.DetailLine("Desfaz 3 movimentos.", 2, 50));
        }
    }
}
