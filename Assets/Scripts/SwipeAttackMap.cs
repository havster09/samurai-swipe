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
        //  Dir         Light                Medium               Heavy
        //  ─────────── ──────────────────── ──────────────────── ────────────────────
        //  Up          heroAttackFour       heroAttackFour       heroAttackSeven
        //  Down        heroAttackOne        heroAttackOne        heroDoubleSlashLow
        //  Right       heroAttackTwo        heroAttackThree      heroDashAttack
        //  Left        heroAttackSix        heroAttackFive       heroDoubleSlashMid
        //  UpRight     heroAttackSix        heroAttackFour       heroDoubleSlashHigh
        //  UpLeft      heroAttackTwo        heroAttackFour       heroDoubleSlashHigh
        //  DownRight   heroAttackOne        heroAttackFive       heroDoubleSlashLow
        //  DownLeft    heroAttackOne        heroAttackFive       heroDoubleSlashLow
        //
        public static string Resolve(ImprovedSwipeDetector.SwipeDirection dir, float velocity)
        {
            VelocityTier tier = GetTier(velocity);

            switch (dir)
            {
                case ImprovedSwipeDetector.SwipeDirection.Up:
                    switch (tier)
                    {
                        case VelocityTier.Heavy:  return "heroAttackSeven";   // step strong slash
                        case VelocityTier.Medium: return "heroAttackFour";    // high slash
                        default:                  return "heroAttackFour";    // high slash
                    }

                case ImprovedSwipeDetector.SwipeDirection.Down:
                    switch (tier)
                    {
                        case VelocityTier.Heavy:  return "heroDoubleSlashLow"; // double low
                        case VelocityTier.Medium: return "heroAttackOne";      // slash down
                        default:                  return "heroAttackOne";      // slash down
                    }

                case ImprovedSwipeDetector.SwipeDirection.Right:
                    switch (tier)
                    {
                        case VelocityTier.Heavy:  return "heroDashAttack";    // dash attack
                        case VelocityTier.Medium: return "heroAttackThree";   // dash slash
                        default:                  return "heroAttackTwo";     // side slash
                    }

                case ImprovedSwipeDetector.SwipeDirection.Left:
                    switch (tier)
                    {
                        case VelocityTier.Heavy:  return "heroDoubleSlashMid"; // double mid
                        case VelocityTier.Medium: return "heroAttackFive";     // crouch slash
                        default:                  return "heroAttackSix";      // crouch slash high
                    }

                case ImprovedSwipeDetector.SwipeDirection.UpRight:
                    switch (tier)
                    {
                        case VelocityTier.Heavy:  return "heroDoubleSlashHigh"; // double high
                        case VelocityTier.Medium: return "heroAttackFour";      // high slash
                        default:                  return "heroAttackSix";       // crouch slash high
                    }

                case ImprovedSwipeDetector.SwipeDirection.UpLeft:
                    switch (tier)
                    {
                        case VelocityTier.Heavy:  return "heroDoubleSlashHigh"; // double high
                        case VelocityTier.Medium: return "heroAttackFour";      // high slash
                        default:                  return "heroAttackTwo";       // side slash
                    }

                case ImprovedSwipeDetector.SwipeDirection.DownRight:
                case ImprovedSwipeDetector.SwipeDirection.DownLeft:
                    switch (tier)
                    {
                        case VelocityTier.Heavy:  return "heroDoubleSlashLow"; // double low
                        case VelocityTier.Medium: return "heroAttackFive";     // crouch slash
                        default:                  return "heroAttackOne";      // slash down
                    }

                default:
                    return "heroAttackOne";
            }
        }
    }
}
