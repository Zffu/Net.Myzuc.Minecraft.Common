namespace Net.Myzuc.Minecraft.Common.Objects.Chunks
{
    public class PaletteUtility
    {
        static PaletteKind GetKindForBitsPerEntry(int bitsPerEntry, bool isBiome)
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
    }    
}
