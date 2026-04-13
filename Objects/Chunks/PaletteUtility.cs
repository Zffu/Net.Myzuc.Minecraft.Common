namespace Net.Myzuc.Minecraft.Common.Objects.Chunks
{
    public class PaletteUtility
    {
        public static PaletteKind GetKindForBitsPerEntry(int bitsPerEntry, bool isBiome)
        {
            if (bitsPerEntry == 0) return PaletteKind.SingleValued;

            switch (isBiome)
            {
                case true:
                    if (bitsPerEntry <= 3) return PaletteKind.Indirect;
                    break;
                case false:
                    if(bitsPerEntry <= 8) return PaletteKind.Indirect; // Minimum here is 4 but checking for minimum is probably not needed
                    break;
            }
            
            return PaletteKind.Direct;
        }

        /**
         * Gets the amount of bits needed for a given amount of entries
         * https://github.com/GlowstoneMC/Glowstone/blob/dev/src/main/java/net/glowstone/util/VariableValueArray.java#L46
         */
        public static int GetPaletteNeededBits(int entryCount)
        {
            var count = 0;

            do
            {
                count++;
                entryCount >>>= 1;
            } while (entryCount != 0);

            return count;
        }
    }    
    
    
}
