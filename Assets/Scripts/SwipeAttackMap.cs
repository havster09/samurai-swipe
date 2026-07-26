namespace Assets.Scripts
{
    /// <summary>
    /// Pure mapping table: swipe direction + velocity tier + end slope → animator trigger name.
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

        // ── Damage per trigger name ───────────────────────────────────────────────
        // Single source of truth for attack damage values. Hero.GetAttackTypeAndDamage
        // calls this instead of maintaining its own string-match chains.
        public static int DamageForTrigger(string trigger)
        {
            switch (trigger)
            {
                case "heroDoubleSlashHigh":
                case "heroDoubleSlashMid":
                case "heroDoubleSlashLow":
                case "heroAttackSeven":
                    return DamageForTier(VelocityTier.Heavy);   // 150

                case "heroAttackThree":
                case "heroAttackFour":
                case "heroAttackFive":
                case "heroDashAttack":
                    return DamageForTier(VelocityTier.Medium);  // 100

                default:
                    return DamageForTier(VelocityTier.Light);   // 60
            }
        }

        // ── Direction → animator trigger ─────────────────────────────────────────
        //
        //  Dir           Light                Medium               Heavy
        //  ───────────── ──────────────────── ──────────────────── ──────────────────────────
        //  Up            heroAttackFour       heroAttackFour       heroAttackSix
        //  Down          heroAttackOne        heroAttackOne        heroAttackSeven
        //  Right         heroAttackTwo        heroAttackThree*     heroDashAttack*    * momentum
        //  Left          heroAttackTwo        heroAttackFive       doubleSlash(slope)
        //  UpRight       heroAttackThree      heroAttackThree      doubleSlash(slope)
        //  UpLeft        heroAttackTwo        heroAttackFive       doubleSlash(slope)
        //  DownRight     heroAttackTwo        heroAttackFive       doubleSlash(slope)
        //  DownLeft      heroAttackTwo        heroAttackFive       doubleSlash(slope)
        //
        //  doubleSlash(slope):  endSlope Up → High | Neutral → Mid | Down → Low
        //
        public static string Resolve(
            ImprovedSwipeDetector.SwipeDirection dir,
            float velocity,
            ImprovedSwipeDetector.EndSlope endSlope = ImprovedSwipeDetector.EndSlope.Neutral)
        {
            VelocityTier tier = GetTier(velocity);

            switch (dir)
            {
                case ImprovedSwipeDetector.SwipeDirection.Up:
                    switch (tier)
                    {
                        case VelocityTier.Heavy:  return "heroAttackSix";
                        default:                  return "heroAttackFour";
                    }

                case ImprovedSwipeDetector.SwipeDirection.Down:
                    switch (tier)
                    {
                        case VelocityTier.Heavy:  return "heroAttackSeven";
                        default:                  return "heroAttackOne";
                    }

                case ImprovedSwipeDetector.SwipeDirection.Right:
                    switch (tier)
                    {
                        case VelocityTier.Heavy:  return "heroDashAttack";
                        case VelocityTier.Medium: return "heroAttackThree";
                        default:                  return "heroAttackTwo";
                    }

                case ImprovedSwipeDetector.SwipeDirection.Left:
                    switch (tier)
                    {
                        case VelocityTier.Heavy:  return DoubleSlashForSlope(endSlope);
                        case VelocityTier.Medium: return "heroAttackFive";
                        default:                  return "heroAttackTwo";
                    }

                case ImprovedSwipeDetector.SwipeDirection.UpRight:
                    switch (tier)
                    {
                        case VelocityTier.Heavy:  return DoubleSlashForSlope(endSlope);
                        default:                  return "heroAttackThree";
                    }

                case ImprovedSwipeDetector.SwipeDirection.UpLeft:
                    switch (tier)
                    {
                        case VelocityTier.Heavy:  return DoubleSlashForSlope(endSlope);
                        case VelocityTier.Medium: return "heroAttackFive";
                        default:                  return "heroAttackTwo";
                    }

                case ImprovedSwipeDetector.SwipeDirection.DownRight:
                case ImprovedSwipeDetector.SwipeDirection.DownLeft:
                    switch (tier)
                    {
                        case VelocityTier.Heavy:  return DoubleSlashForSlope(endSlope);
                        case VelocityTier.Medium: return "heroAttackFive";
                        default:                  return "heroAttackTwo";
                    }

                default:
                    return "heroAttackOne";
            }
        }

        /// <summary>
        /// Maps the ending slope of a heavy swipe to the matching double slash variant.
        /// </summary>
        private static string DoubleSlashForSlope(ImprovedSwipeDetector.EndSlope slope)
        {
            switch (slope)
            {
                case ImprovedSwipeDetector.EndSlope.Up:   return "heroDoubleSlashHigh";
                case ImprovedSwipeDetector.EndSlope.Down: return "heroDoubleSlashLow";
                default:                                  return "heroDoubleSlashMid";
            }
        }
    }
}
