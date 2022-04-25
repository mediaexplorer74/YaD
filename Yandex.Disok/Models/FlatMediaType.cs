using System;

namespace Ya.D.Models
{
    [Flags]
    public enum DiskMediaType
    {
        all = 1,
        audio = 1 << 1,
        backup = 1 << 2,
        book = 1 << 3,
        compressed = 1 << 4,
        data = 1 << 5,
        development = 1 << 6,
        diskimage = 1 << 7,
        document = 1 << 8,
        encoded = 1 << 9,
        executable = 1 << 10,
        flash = 1 << 11,
        font = 1 << 12,
        image = 1 << 13,
        settings = 1 << 14,
        spreadsheet = 1 << 15,
        text = 1 << 16,
        unknown = 1 << 17,
        video = 1 << 18,
        web = 1 << 19
    }
}
