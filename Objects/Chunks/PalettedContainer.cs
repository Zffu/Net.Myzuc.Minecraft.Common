namespace Net.Myzuc.Minecraft.Common.Objects.Chunks
{
    /// <summary>
    /// https://minecraft.wiki/w/Java_Edition_protocol/Chunk_format#Paletted_Container_structure
    /// </summary>
    public class PalettedContainer
    {
        public int EntryCount { get; set; }
        public byte BitsPerEntry { get; set; }
        public bool IsBiomeBased { get; set; }

        public IInnerPalette InnerPalette; // We make an inner to allow to destroy and recreate another without updating the PalettedContainer refs
        
        public long[] DataArray { get; set; }

        public PalettedContainer(int[] typeArray, bool isBiomeBased)
        {
            EntryCount = typeArray.Length;
            
            BitsPerEntry = (byte) PaletteUtility.GetPaletteNeededBits(typeArray.Length);
            IsBiomeBased = isBiomeBased;

            InnerPalette = IInnerPalette.FromTypeArray(typeArray, isBiomeBased);

            int entriesPerLong = 64 / BitsPerEntry;
            DataArray = new long[typeArray.Length + (entriesPerLong - 1) / entriesPerLong];
        }

        public int GetEntry(int index)
        {
            return InnerPalette.GetEntry(index, this);
        }

        public void SetEntry(int index, int value, int bitsPerEntry)
        {
            if(InnerPalette.DoesRequireRebuilding(bitsPerEntry, IsBiomeBased))
            {
                var palette = InnerPalette.GatherEntries(EntryCount, this);
                
                InnerPalette = IInnerPalette.FromPalette(palette, IsBiomeBased);
            }
            
            InnerPalette.SetEntry(index, value, this);
        }
    }

    public interface IInnerPalette
    {
        public int GetEntry(int index, PalettedContainer parent);
        public void SetEntry(int index, int value, PalettedContainer parent);
        public bool DoesRequireRebuilding(int newBitPerEntry, bool isBiome);
        public int[] GatherEntries(int entryCount, PalettedContainer parent);
        
        public static IInnerPalette FromTypeArray(int[] typeArray, bool isBiome)
        {
            if (typeArray.Length == 1) return new SingleValuedContainer(typeArray[0]);

            var bitsPer = PaletteUtility.GetPaletteNeededBits(typeArray.Length);

            if (isBiome)
            {
                if (bitsPer <= 3) return new IndirectContainer(typeArray.Length);
                return new DirectContainer(isBiome);
            }

            if (bitsPer <= 8) return new IndirectContainer(typeArray.Length);
            return new DirectContainer(isBiome);
        }

        public static IInnerPalette FromPalette(int[] palette, bool isBiome)
        {
            if (palette.Length == 1) return new SingleValuedContainer(palette[0]);

            var bitsPer = PaletteUtility.GetPaletteNeededBits(palette.Length);

            if (isBiome)
            {
                if (bitsPer <= 3) return new IndirectContainer(palette);
                return new DirectContainer(isBiome);
            }

            if (bitsPer <= 8) return new IndirectContainer(palette);
            return new DirectContainer(isBiome);
        }
    }
    
    public enum PaletteKind
    {
        SingleValued, // SingleValuedContainer
        Indirect, // IndirectContainer
        Direct // void
    }
    
    public class SingleValuedContainer: IInnerPalette
    {
        public int Value { get; set; }

        public SingleValuedContainer(int value)
        {
            Value = value;
        }
        
        public int GetEntry(int index, PalettedContainer parent)
        {
            return Value;
        }

        public void SetEntry(int index, int value, PalettedContainer parent)
        {
            Value = value; // We assume that it's still the same value since we check for BPE before.
        }

        public int[] GatherEntries(int entryCount, PalettedContainer parent)
        {
            int[] values = new int[entryCount];
            Array.Fill(values, Value);

            return values;
        }

        public bool DoesRequireRebuilding(int newBitPerEntry, bool isBiome)
        {
            return newBitPerEntry != 0;
        }
    }

    public class IndirectContainer: IInnerPalette
    {
        public int[] Palette { get; set; }

        public IndirectContainer(int paletteLength)
        {
            Palette = new int[paletteLength];
        }

        public IndirectContainer(int[] palette)
        {
            Palette = palette;
        }

        public int GetEntry(int index, PalettedContainer parent)
        {
            if (index >= 0 && index < Palette.Length) return Palette[index]; // Potentially useless check there since we should already check that outside
            return -1; 
        }

        public void SetEntry(int index, int value, PalettedContainer parent)
        {
            Palette[index] = value;
        }

        public int[] GatherEntries(int entryCount, PalettedContainer parent)
        {
            return Palette; // We assume that both Palette and entryCount are the same
        }

        public bool DoesRequireRebuilding(int newBitPerEntry, bool isBiome)
        {
            return isBiome switch
            {
                true => newBitPerEntry <= 3,
                false => newBitPerEntry <= 8,
            };
        }
    }

    public class DirectContainer : IInnerPalette
    {
        public bool IsBiome { get; set; }

        public DirectContainer(bool isBiome)
        {
            IsBiome = isBiome;
        }
        
        public int GetEntry(int index, PalettedContainer parent)
        {
            var entriesPerLong = 64 / parent.BitsPerEntry;
            var mask = ((long)1 << parent.BitsPerEntry) - 1;
            var longIndex = index / entriesPerLong;
            var bitIndex = index % entriesPerLong * parent.BitsPerEntry;

            return (int)((parent.DataArray[longIndex] >> bitIndex) & mask);
        }

        public void SetEntry(int index, int value, PalettedContainer parent)
        {
            var entriesPerLong = 64 / parent.BitsPerEntry;
            var mask = ((long)1 << parent.BitsPerEntry) - 1;
            var longIndex = index / entriesPerLong;
            var bitIndex = index % entriesPerLong * parent.BitsPerEntry;

            parent.DataArray[longIndex] &= ~(mask << bitIndex);
            parent.DataArray[longIndex] |= (long)value << bitIndex;
        }

        public int[] GatherEntries(int entryCount, PalettedContainer parent)
        {
            int[] entries = new int[entryCount];

            for (int i = 0; i < entryCount; ++i)
            {
                entries[i] = GetEntry(i, parent);
            }

            return entries;
        }

        public bool DoesRequireRebuilding(int newBitPerEntry, bool isBiome)
        {
            // We can simply check here if the size is bigger than what we need for indirect to handle Notchian client
            return isBiome switch
            {
                true => newBitPerEntry > 3,
                false => newBitPerEntry > 8
            };
        }
    }

}
