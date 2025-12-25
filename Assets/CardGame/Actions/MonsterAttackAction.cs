using System;
using Controllers;
using DG.Tweening;
using UnityEngine;

namespace Actions
{
    /// <summary>
    /// Visual game action: animasikan kartu monster penyerang menabrak target
    /// (monster lawan atau direct attack), lalu jalankan logika battle di PlayerController.
    /// </summary>
    public sealed class MonsterAttackAction : GameAction
    {
        private readonly PlayerController attackerController;
        private readonly PlayerController defenderController;
        private readonly int attackerSlot;
        private readonly int targetSlot;
        private readonly Transform attackerTransform;
        private readonly Transform targetTransform;
        private readonly float totalDuration;
        private readonly float impactOffset;

        public MonsterAttackAction(
            PlayerController attackerController,
            PlayerController defenderController,
            int attackerSlot,
            int targetSlot,
            Transform attackerTransform,
            Transform targetTransform,
            float totalDuration = 0.5f,
            float impactOffset = 0.2f)
        {
            this.attackerController = attackerController;
            this.defenderController = defenderController;
            this.attackerSlot = attackerSlot;
            this.targetSlot = targetSlot;
            this.attackerTransform = attackerTransform;
            this.targetTransform = targetTransform;
            this.totalDuration = Mathf.Max(0.05f, totalDuration);
            this.impactOffset = Mathf.Max(0f, impactOffset);
        }

        public override void Execute(Action onFinished)
        {
            // Jika controller tidak tersedia, langsung jalankan logika battle tanpa animasi.
            if (attackerController == null || defenderController == null)
            {
                SafeApplyBattleLogic();
                onFinished?.Invoke();
                return;
            }

            // Jika transform penyerang tidak ada, tidak bisa animasi; langsung resolve.
            if (attackerTransform == null)
            {
                SafeApplyBattleLogic();
                onFinished?.Invoke();
                return;
            }

            Vector3 startPos = attackerTransform.position;

            // Tentukan posisi tabrakan: sedikit sebelum posisi target,
            // atau maju sedikit jika tidak ada target (direct attack).
            Vector3 attackPos;
            if (targetTransform != null)
            {
                Vector3 targetPos = targetTransform.position;
                Vector3 dir = (targetPos - startPos).normalized;
                attackPos = targetPos - dir * impactOffset;
            }
            else
            {
                // Direct attack: gerak ke arah "atas" canvas / dunia.
                attackPos = startPos + new Vector3(0f, 0.5f, 0f);
            }

            float forwardDuration = totalDuration * 0.5f;
            float returnDuration = totalDuration - forwardDuration;

            bool battleLogicApplied = false;

            Sequence seq = DOTween.Sequence();

            seq.Append(attackerTransform
                .DOMove(attackPos, forwardDuration)
                .SetEase(Ease.OutQuad));

            seq.AppendCallback(() =>
            {
                if (!battleLogicApplied)
                {
                    SafeApplyBattleLogic();
                    battleLogicApplied = true;
                }
            });

            seq.Append(attackerTransform
                .DOMove(startPos, returnDuration)
                .SetEase(Ease.InQuad));

            seq.OnComplete(() =>
            {
                if (!battleLogicApplied)
                {
                    SafeApplyBattleLogic();
                }

                onFinished?.Invoke();
            });
        }

        private void SafeApplyBattleLogic()
        {
            try
            {
                if (attackerController != null && defenderController != null)
                {
                    attackerController.AttackWithMonster(attackerSlot, targetSlot, defenderController);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MonsterAttackAction] Error applying battle logic: {ex.Message}");
            }
        }
    }
}
