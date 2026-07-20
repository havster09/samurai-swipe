namespace Assets.Scripts
{
    /// <summary>
    /// Pure mapping table: swipe direction + velocity tier → animator trigger name.
    /// Edit this file to remap any swipe without touching controller or GOAP logic.
    /// </summary>
    public static class SwipeAttackMap
    {
        // ── Velocity tiers ────────────────────────────────────────────────────────
        public const float HeavyThreshold  = 1500f; // px/s
        public const float MediumThreshold =  600f; // px/s

        public static VelocityTier GetTier(float velocity)
        {
            if (velocity >= HeavyThreshold)  return VelocityTier.Heavy;
            if (velocity >= MediumThreshold) return VelocityTier.Medium;
            return VelocityTier.Light;
        }

        public enum VelocityTier { Light, Medium, Heavy }

        // ── Damage per tier ───────────────────────────────────────────────────────
        public static int DamageForTier(VelocityTier tier)
        {
            switch (tier)
            {
                case VelocityTier.Heavy:  return 150;
                case VelocityTier.Medium: return 100;
                default:                  return  60;
            }
        }

        // ── Direction → animator trigger ─────────────────────────────────────────
        //
        //  Dir         Light               Medium              Heavy
        //  ─────────── ─────────────────── ─────────────────── ───────────────────
        //  Up          heroAttackFour      heroAttackFour      heroAttackSeven
        //  Down        heroAttackOne       heroAttackOne       heroDoubleSlashLow
        //  Left/Right  heroAttackTwo       heroAttackThree     heroDashAttack
        //  UpDiag      heroAttackSix       heroAttackFour      heroDoubleSlashHigh
        //  DownDiag    heroAttackOne       heroAttackFive      heroDoubleSlashLow
        //
        public static string Resolve(ImprovedSwipeDetector.SwipeDirection dir, float velocity)
        {
            VelocityTier tier = GetTier(velocity);

            switch (dir)
            {
                case ImprovedSwipeDetector.SwipeDirection.Up:
                    switch (tier)
                    {
                        case VelocityTier.Heavy:  return "heroAttackSeven";
                        default:                  return "heroAttackFour";
                    }

                case ImprovedSwipeDetector.SwipeDirection.Down:
                    switch (tier)
                    {
                        case VelocityTier.Heavy:  return "heroDoubleSlashLow";
                        default:                  return "heroAttackOne";
                    }

                case ImprovedSwipeDetector.SwipeDirection.Left:
                case ImprovedSwipeDetector.SwipeDirection.Right:
                    switch (tier)
                    {
                        case VelocityTier.Heavy:  return "heroDashAttack";
                        case VelocityTier.Medium: return "heroAttackThree";
                        default:                  return "heroAttackTwo";
                    }

                case ImprovedSwipeDetector.SwipeDirection.UpRight:
                case ImprovedSwipeDetector.SwipeDirection.UpLeft:
                    switch (tier)
                    {
                        case VelocityTier.Heavy:  return "heroDoubleSlashHigh";
                        case VelocityTier.Medium: return "heroAttackFour";
                        default:                  return "heroAttackSix";
                    }

                case ImprovedSwipeDetector.SwipeDirection.DownRight:
                case ImprovedSwipeDetector.SwipeDirection.DownLeft:
                    switch (tier)
                    {
                        case VelocityTier.Heavy:  return "heroDoubleSlashLow";
                        case VelocityTier.Medium: return "heroAttackFive";
                        default:                  return "heroAttackOne";
                    }

                default:
                    return "heroAttackOne";
            }
        }
    }
}
