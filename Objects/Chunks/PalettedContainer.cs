namespace Net.Myzuc.Minecraft.Common.Objects.Chunks
{
    /// <summary>
    /// https://minecraft.wiki/w/Java_Edition_protocol/Chunk_format#Paletted_Container_structure
    /// </summary>
    public class PalettedContainer
    {
        public byte BlockCount { get; set; } = 0;
    }

    public enum PaletteKind
    {
        SingleValued, // SingleValuedContainer
        Indirect, // IndirectContainer
        Direct // void
    }
    
    public class SingleValuedContainer
    {
        public int Value { get; set; } = 0;
    }

    public class IndirectContainer
    {
        public int PaletteLength { get; set; } = 0;
        public int[] Palette { get; set; } = new int[] { };
    }
    
}
