namespace Xbl.Xbox360.Constants
{
    public enum BlockStatus
    {
        Unallocated = 0,
        PreviouslyAllocated = 0x40,
        Allocated = 0x80,
        NewlyAllocated = 0xC0
    }
}